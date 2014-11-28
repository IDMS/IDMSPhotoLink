/*
 * Copyright ObjEx, Inc. ©  2014, All Rights Reserved
 * Licensed to Recipients under GNU GPL 3.0
 * 
 * ObjEx, Inc. is an Arizona Corporation and can be reached at
 * www.obj-ex.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.Web;

namespace IDMSPhotoLink
{
    /// <summary>
    /// Interface to the Topic (publish and subscribe) features in the Azure Service Bus
    /// </summary>
    /// <remarks>
    /// This class has a constructor but for performance purposes on a web site, use the static GetTopicService method.  
    /// This method will construct the class and add it to the default memory cache.  Subsequent calls will 
    /// attempt to retreive the item from the class saving two round-trip calls to the Service Bus.  Items remain
    /// in the cache until 30 miniutes have expired since their last use.
    /// </remarks>
    [Serializable]
    public class TopicServices
    {
        private int WaitTimeMS;
        private SubscriptionClient subscriptionClient;
        private string _TopicName;
        private TopicClient topicClient;
        private NamespaceManager namespaceManager = null;


        /// <summary>
        /// Construct a TopicServices class object and return it to the caller.<para>Setting SubscriptionName to null will create a writer.
        /// Give it a value to subscribe abd create a reader.</para>
        /// </summary>
        /// <remarks>
        /// Checks the cache for an interface to this topic.  If it finds one it is returned, if not it creates
        /// one, adds it to the cache and then returns it.  
        /// </remarks>
        /// <param name="TopicName"> The name of the Topic queue </param>
        /// <param name="SubscriptionName"> The name of the subscription.  Set to null for to get a wrie only object </param>
        /// <param name="WaitTimeMS"></param>
        /// <returns></returns>
        public static TopicServices GetTopicService(string TopicName, string SubscriptionName = null, int WaitTimeMS = 2000)
        {
            System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy CachingPolicy;
            string strWork;

            if (SubscriptionName == null) strWork = TopicName.ToLower();
            else strWork = TopicName + "~" + SubscriptionName.ToLower();

            TopicServices ts = (TopicServices)cache.Get(strWork);

            if (ts == null)
            {

                ts = new TopicServices(TopicName, WaitTimeMS);

                if (SubscriptionName != null)
                {
                    try
                    {
                        ts.AccessOrCreateSubscription(SubscriptionName);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                CachingPolicy = new System.Runtime.Caching.CacheItemPolicy();
                CachingPolicy.SlidingExpiration = new TimeSpan(0, 30, 0);  // keep for 30 minutes since last access
                CachingPolicy.RemovedCallback = new System.Runtime.Caching.CacheEntryRemovedCallback(ts.RemovalHandler);
                cache.Set(strWork, ts, CachingPolicy, null);  // will replace if found
                //cache.Remove(strKey);  // merely to test the delegate
                

            }


            return ts;
        }

        private TopicServices(string TopicName, int WaitTimeMS = 2000)
        {
            this._TopicName = TopicName.ToLower();
            this.WaitTimeMS = WaitTimeMS;
            
            this.namespaceManager = NamespaceManager.Create();
            AccessOrCreateTopic();

        }

        public void RemovalHandler(System.Runtime.Caching.CacheEntryRemovedArguments args)
        {
            if (subscriptionClient != null)
            {
                try
                {
                    TopicServices x = (TopicServices)args.CacheItem.Value;
                    NamespaceManager m = NamespaceManager.Create();
                    m.DeleteSubscription(subscriptionClient.TopicPath, subscriptionClient.Name);
                }
                catch
                { }
            }

            return;
        
        }
        

        public string TopicName
        {
            get
            {
                return _TopicName;
            }
        }

        public string CurrentSubscriptionName
        {
            get
            {
                if (subscriptionClient != null)
                {
                    return subscriptionClient.Name;
                }

                return "";
            }
        }

        /// <summary>
        /// Deletes topics who meet both the MaximumAge and MaxMessageCount criterea.
        /// If both are zero, MaximumAge = TimeSpan.Zero and MaxMessageCount = 0 then all topics
        /// and their messages will be deleted.
        /// </summary>
        /// <param name="MaximumAge">Deletes topics who have not been accessed for this amount of time</param>
        /// <param name="MaxSubscriptionCount">Deletes topics whose subscription count is less than or equal to this number</param>
        public static void CleanUpTopics(TimeSpan MaximumAge, int MaxSubscriptionCount = 0)
        {
            NamespaceManager nm = NamespaceManager.Create();

            IEnumerable<TopicDescription> topics = nm.GetTopics();

            foreach (TopicDescription subdesc in topics)
            {
                TimeSpan age = DateTime.UtcNow - subdesc.AccessedAt;

                if (age > MaximumAge && subdesc.SubscriptionCount <= MaxSubscriptionCount)
                {
                    nm.DeleteTopic(subdesc.Path);
                }

            }
        }


        /// <summary>
        /// Deletes subscriptions who meet both the MaximumAge and MaxMessageCount criterea.
        /// If both are zero, MaximumAge = TimeSpan.Zero and MaxMessageCount = 0 then all subscriptions
        /// and their messages will be deleted.
        /// </summary>
        /// <param name="MaximumAge">Deletes subscriptions who have not been accessed for this amount of time</param>
        /// <param name="MaxMessageCount">Deletes subscriptions whose message count is less than or equal to this number</param>
        public void CleanUpSubscriptions(TimeSpan MaximumAge, int MaxMessageCount = 0)
        {
            IEnumerable<SubscriptionDescription> subscriptions = namespaceManager.GetSubscriptions(TopicName);

            foreach (SubscriptionDescription subdesc in subscriptions)
            {
                TimeSpan age = DateTime.UtcNow - subdesc.AccessedAt;

                if (age > MaximumAge && subdesc.MessageCount <= MaxMessageCount)
                {
                    namespaceManager.DeleteSubscription(TopicName, subdesc.Name);
                }

            }
        }

        /// <summary>
        /// Deletes the topic from the service bus.  The object is essentially unusable until 
        /// AccessOrCreateTopic is called to recreate the topic in the service bus.
        /// </summary>
        /// <returns></returns>
        
        /// <summary>
        /// Prepares the interface for use.
        /// <remarks>This is called automatically in the constructor.  It is exposed
        /// for use if you want to delete and recreate the topic.</remarks>
        /// </summary>
        private void AccessOrCreateTopic()
        {
            TopicDescription topicdescWork;
            topicClient = null;
            subscriptionClient = null;

            while (topicClient == null)
            {
                try
                {
                    // Open or Create as needed
                    if (namespaceManager.TopicExists(TopicName))
                    {
                        System.Diagnostics.Debug.WriteLine(Environment.NewLine + "Getting Topic" + Environment.NewLine);
                        topicdescWork = namespaceManager.GetTopic(TopicName);
                    }
                    else
                    {
                        TopicDescription Topicdesc = new TopicDescription(TopicName);
                        Topicdesc.DefaultMessageTimeToLive = new TimeSpan(1, 0, 0);
                        Topicdesc.EnableBatchedOperations = false;
                        Topicdesc.MaxSizeInMegabytes = 1024;

                        System.Diagnostics.Debug.WriteLine(Environment.NewLine + "Creating Topic" + Environment.NewLine);
                        topicdescWork = namespaceManager.CreateTopic(Topicdesc);
                    }


                    topicClient = TopicClient.Create(TopicName);

                }
                catch (MessagingException e)
                {
                    if (e.IsTransient) Task.Delay(1000);  // If this is a transient error delay a bit and try again
                    else throw e; // no recovery
                }
            }
        }
        private void AccessOrCreateSubscription(string subscriptionname)
        {
            subscriptionClient = null;

            while (subscriptionClient == null)
            {
                try
                {
                    if (!namespaceManager.SubscriptionExists(TopicName, subscriptionname.ToLower()))
                    {
                        namespaceManager.CreateSubscription(TopicName, subscriptionname.ToLower());
                    }
                    subscriptionClient = SubscriptionClient.Create(TopicName, subscriptionname.ToLower());
                }
                catch (MessagingException e)
                {
                    if (e.IsTransient) Task.Delay(1000);  // If this is a transient error delay a bit and try again
                    else throw e; // no recovery
                }
            }
        }

        public string SendTopicMessage(string message, string id = null, string label = null)
        {
            while (true)
            {
                if (subscriptionClient != null) throw new Exception("This TopicServices Object is read only.");

                try
                {
                    BrokeredMessage msg = CreateStandardMessage(id, label, message);
                    System.Diagnostics.Debug.WriteLine(Environment.NewLine + "Sending Message" + Environment.NewLine);
                    this.topicClient.Send(msg);
                    return "OK";
                }
                catch (MessagingException e)
                {
                    if (e.IsTransient) Task.Delay(1000);  // If this is a transient error delay a bit and try again
                    else throw e; // no recovery

                }
            } 
        }

        public string ReadSubscriptionMessage()
        {
            // For PeekLock mode (default) where applications require "at least once" delivery of messages 

            if (subscriptionClient == null) throw new Exception("This TopicServices Object is write only.");

            BrokeredMessage message = null;
            while (true)
            {
                try
                {
                    message = subscriptionClient.Receive(TimeSpan.FromMilliseconds(WaitTimeMS));

                    if (message != null)
                    {
                        string response = message.GetBody<string>();
                        message.Complete();
                        return response;
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (MessagingException e)
                {
                    if (e.IsTransient) Task.Delay(1000);  // If this is a transient error delay a bit and try again
                    else throw e; // no recovery
                }
            }
        }
        
        private BrokeredMessage CreateStandardMessage(string messageId, string messageLabel, string messageBody)
        {
            
            if (messageBody == null) throw new NullReferenceException("The message body must not be null");
            BrokeredMessage message = new BrokeredMessage(messageBody);
            if (messageId != null) message.MessageId = messageId;
            if (messageLabel != null) message.Label = messageLabel;
            return message;
        }
    }
}