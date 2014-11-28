/*
 * Copyright ObjEx, Inc. ©  2014, All Rights Reserved
 * Licensed to Recipients under GNU GPL 3.0
 * 
 * ObjEx, Inc. is an Arizona Corporation and can be reached at
 * www.obj-ex.com
 * 
 */

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace IDMSPhotoLink.Controllers
{
    public class EmployeePicsController : ApiController
    {
        // GET: api/EmployeePics
        //  public EmployeeViewedPostResponse PostEmployeePics([FromBody]EmployeeViewedPostRequest eventViewed)
        [HttpGet]
        public IHttpActionResult Get([FromUri]string UserID, [FromUri]string SubscriptionName, [FromUri]int WaitMS = 5000)
        {
            EmployeeViewedGetResponse response = new EmployeeViewedGetResponse();
            response.PictureURL = "Photos/no_image.jpg";

            try
            {
                TopicServices svc = TopicServices.GetTopicService(UserID.ToLower(), SubscriptionName.ToLower(), WaitMS);  // 


                System.Guid guid = System.Guid.NewGuid();

                string strReadEvent = null;
                strReadEvent = svc.ReadSubscriptionMessage();

                if (strReadEvent == null)
                {
                    response.status = "TIMEOUT";
                    response.ReplyTime = DateTime.Now;
                    response.UserID = UserID;
                    response.EmployeeID = 0;
                    return Ok(response);

                }

                EmployeeViewedPostRequest request;
                try
                {
                    request = JsonConvert.DeserializeObject<EmployeeViewedPostRequest>(strReadEvent);
                }
                catch
                {
                    response.status = "ERROR";
                    response.ReplyTime = DateTime.Now;
                    response.UserID = UserID;
                    response.EmployeeID = 0;
                    response.message = "Invalid Message Format Read from the subscription. Unable to deseriaize the content into an EmployeeViewedPostRequest.";
                    return Ok(response);
                }

                if (request.UserID.ToLower() != UserID.ToLower())
                {
                    response.status = "ERROR";
                    response.ReplyTime = DateTime.Now;
                    response.UserID = UserID;
                    response.EmployeeID = 0;
                    response.message = "Invalid Message Format Read from the subscription. UserID does not match the UserID read from the subscription.";
                    return Ok(response);
                }

                string strPictureURL = string.Format(System.Web.HttpRuntime.AppDomainAppPath + @"Photos\{0}.jpg", request.ViewedEmployeeID.ToString().PadLeft(4, '0'));
//                string xxx = System.Web.HttpRuntime;
                response.ReplyTime = DateTime.Now;
                response.UserID = UserID;
                response.EmployeeID = request.ViewedEmployeeID;
                response.firstName = request.firstName;
                response.lastName = request.lastName;
                response.PictureURL = string.Format("Photos/{0}.jpg", response.EmployeeID.ToString().PadLeft(4, '0'));
                response.VideoUri = "";

                if (System.IO.File.Exists(strPictureURL)) response.status = "OK";
                else
                {
                    response.status = "OK";
                    response.PictureURL = "Photos/no_image.jpg";
                }
                
                return Ok(response);

            }

            catch (System.UnauthorizedAccessException ex)
            {
                response.message = ex.Message;  //just to get rid of the warning
                response.ReplyTime = DateTime.Now;
                response.status = "AUTHERROR";
                response.message = string.Format("Web service is using a bad service bus connection string.");
                this.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                this.ActionContext.Response.ReasonPhrase = "Bad Connection String";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.ReplyTime = DateTime.Now;
                response.status = "ERROR";
                response.message = string.Format("PostEmployeePics exception: {0}", ex.Message);
                return Ok(response);

            }

        }

        // POST: api/EmployeePics
        [HttpPost]
        public EmployeeViewedPostResponse PostEmployeePics([FromBody]EmployeeViewedPostRequest eventViewed)
        {
            EmployeeViewedPostResponse response = new EmployeeViewedPostResponse();
            try
            {
                TopicServices svc = TopicServices.GetTopicService(eventViewed.UserID, null, 5000);  // 

                System.Guid guid = System.Guid.NewGuid();

                // message, id, label
                //string strTopicSaveMessage = svc.SendTopicMessage(eventViewed.UserID + "," + eventViewed.ViewedEmployeeID.ToString()
                //    , guid.ToString(), "EmployeeViewEvent");

                string strTopicSaveMessage = svc.SendTopicMessage(JsonConvert.SerializeObject(eventViewed),
                    guid.ToString(), "EmployeeViewEvent");


                if (strTopicSaveMessage == "OK")
                {
                    response.PostTime = DateTime.Now;
                    response.status = "OK";
                    response.message = string.Format("Message posted: User {0} viewed employee number {1}",
                        eventViewed.UserID, eventViewed.ViewedEmployeeID.ToString());
                    return response;
                }
                else
                {
                    response.PostTime = DateTime.Now;
                    response.status = "ERROR";
                    response.message = string.Format("Unexpected response from the service: {0}", strTopicSaveMessage);
                    return response;
                }
            }

            catch (System.UnauthorizedAccessException ex)
            {
                response.message = ex.Message;  //just to get rid of the warning
                response.PostTime = DateTime.Now;
                response.status = "AUTHERROR";
                response.message = string.Format("Web service is using a bad service bus connection string.");
                this.ActionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                this.ActionContext.Response.ReasonPhrase = "Bad Connection String";

                return response;
            }
            catch (Exception ex)
            {
                response.PostTime = DateTime.Now;
                response.status = "ERROR";
                response.message = string.Format("PostEmployeePics exception: {0}", ex.Message);
                return response;

            }

        }


        //// DELETE: api/EmployeePics/5
        //[HttpDelete]
        //public void Delete(int id)
        //{
        //    throw new NotImplementedException();
        //}
    }
    
}
