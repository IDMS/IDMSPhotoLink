using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IDMSPhotoLink.Controllers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;


namespace IDMSPhotoLink.Tests
{
    [TestClass]
    public class EmployeePicsController_Tests
    {
        // http://idmsphotolink.azurewebsites.net/api/EmployeePics?UserID=hebert&SubscriptionName=topic1&WaitMS=2000

        static private bool? _console_present;
        //static string strDefaultUri = "http://idmsphotolink.azurewebsites.net/";
        static string strDefaultUri = "http://localhost:65095/";

        
        [TestMethod]
        public void TestPost()
        {
            Task t = TestPostAsync(strDefaultUri, "TestUser", 23, "Tom", "Jones");
            t.Wait();
        }
        [TestMethod]
        public void TestGet()
        {
            Task t = TestGetAsync(strDefaultUri, "TestUser", "topic1", 1000);
            t.Wait();
        }

        public static async Task<bool> TestPostAsync(string strUri, string TopicName, int EmployeeID, string FirstName, string LastName)
        {
            using (var client = new HttpClient())
            {
                Uri requestUri = new Uri(strUri);

                client.BaseAddress = new Uri(strUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                IDMSPhotoLink.EmployeeViewedPostRequest request = new EmployeeViewedPostRequest();
                request.UserID = TopicName;
                request.ViewedEmployeeID = EmployeeID;
                request.firstName = FirstName;
                request.lastName = LastName;

                // HTTP POST

                HttpResponseMessage response = await client.PostAsJsonAsync<IDMSPhotoLink.EmployeeViewedPostRequest>("api/EmployeePics", request);

                if (response.IsSuccessStatusCode)
                {
                    IDMSPhotoLink.EmployeeViewedPostResponse postresponse = await response.Content.ReadAsAsync<IDMSPhotoLink.EmployeeViewedPostResponse>();
                    Writeline(postresponse.message);
                }
                else
                {
                    string msg = string.Format("Failed status code: {0}, URL used:{1}, Message received: {2}",
                        response.StatusCode, response.RequestMessage, response.ReasonPhrase);
                    Writeline(msg);
                    Console.ReadKey();
                }

            }
            return true;
        }

        public static async Task<bool> TestGetAsync(string strUri, string TopicName, string SubscriptionName, int WaitMS = 5000)
        {
            using (var client = new HttpClient())
            {
                Uri requestUri = new Uri(strUri + string.Format("api/EmployeePics?UserID={0}&SubscriptionName={1}&WaitMS={2}",
                    TopicName.ToLower(), SubscriptionName.ToLower(), WaitMS));

                client.BaseAddress = new Uri(strUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET

                string response = await client.GetStringAsync(requestUri);

                if (response.Contains("TIMEOUT")) Writeline("TIMEOUT");
                else Writeline(response);

                if (response.Contains("ERROR")) return false;
                else return true;

            }
        }

        public static bool console_present
        {
            get
            {
                if (_console_present == null)
                {
                    try { int window_height = Console.WindowHeight; }
                    catch { _console_present = false; }
                    _console_present = true;
                }
                return _console_present.Value;
            }
        }

        public static void Writeline(string message)
        {
            Console.WriteLine(message);
        }

    }
}
