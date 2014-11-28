using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IDMSPhotoLink.Tests;


namespace IDMSPhotoLink.SendTest
{

    class Program
    {
        private static bool keepRunning = true;
        private static bool cancel_notrequested = true;
        //static string strDefaultUri = "http://idmsphotolink.azurewebsites.net/";
        static string strDefaultUri = "http://localhost:65095/";


        static void Main(string[] args)
        {

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
            {
                Console.WriteLine("Cancelling when the cuurent operation completes or times out.  Please wait...");
                e.Cancel = true;
                cancel_notrequested = false;
            };

            // Three arguments either send or receive, the topic name and the subscription name (receive only)
            if (args.Length != 3 && args.Length != 5)
            {
                Console.WriteLine("Bad arguments");
                return;
            }

            string strCommand = args[0].ToLower();

            if (strCommand != "send" && strCommand != "receive")
            {
                Console.WriteLine("Bad command, must be send or receive");
                return;
            }

            if (strCommand == "send")
            {
                 if (args.Length != 5)
                 {
                     Console.WriteLine("Send command must have four arguments, UserName, EmployeeID, firstName, fastName");
                     return;
                 }
                int nEmloyeeID;
                try 
                {
                    nEmloyeeID = int.Parse(args[2]);
                }
                catch
                {
                    Console.WriteLine("Unable to parse the value provided for employee id: {0}", args[2]);
                    return;
                }

                EmployeePicsController_Tests.TestPostAsync(strDefaultUri, args[1], nEmloyeeID, args[3], args[4]).Wait();
                return;
            }

            if (strCommand == "receive")
            {

                while (keepRunning && cancel_notrequested)
                {
                    keepRunning = EmployeePicsController_Tests.TestGetAsync(strDefaultUri, args[1], args[2]).Result;
                }
                return;
            }


        }

    }
}



