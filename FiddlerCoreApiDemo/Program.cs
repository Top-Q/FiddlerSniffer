using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Security.Policy;
using FiddlerCoreApiDemo.HttpClasses;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Json;

namespace FiddlerCoreApiDemo
{
    class Program
    {

        private static List<string> capturingData = new List<string>();
        private static string snifferLogsDir = @"C:\SnifferLogs";
        private static int capturingTime = 30000;
        //private static List<string> filters = new 

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                snifferLogsDir = args[0];
                capturingTime = Int32.Parse(args[1]);
            }

            Console.WriteLine("Start capruring....");
            NetSniffer netSniffer = new NetSniffer(64444, true, true);
            netSniffer.AddFilter("Domain.Contains(\"related\")");
            netSniffer.StartCapturing();

            Thread.Sleep(capturingTime);

            List<HttpRequest> requests = netSniffer.GetFilltredCapturedTraffic(ELogicFiltersConcatination.AND);
            reportNetworkStats(requests);            
            netSniffer.ShutDown();
            exportCapturedRequestsToJson(requests);
            Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();                        
            
        }

        private static void exportCapturedRequestsToJson(List<HttpRequest> requests)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<HttpRequest>));
            ser.WriteObject(ms, requests);

            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);

            File.WriteAllText(snifferLogsDir + "\\CapturedRequestsJson.txt", sr.ReadToEnd());                        
        }

        private static void reportNetworkStats(List<HttpRequest> requests)
        {
            if (requests.Count == 0)
            {
                Console.WriteLine("Failed to capture any requests.");
                capturingData.Add("Failed to capture any requests.");
            }
            else
            {
                Console.WriteLine("Captured " + requests.Count.ToString() + " requests.");
                capturingData.Add("Captured " + requests.Count.ToString() + " requests.");
                foreach (HttpRequest req in requests)
                {
                    string encryptedData = String.Empty;
                    Console.WriteLine("Request Number : " + req.Id);
                    capturingData.Add("Request Number : " + req.Id);
                    Console.WriteLine("Host : " + req.Domain);
                    capturingData.Add("Host : " + req.Domain);
                    Console.WriteLine("Port : " + req.Port);
                    capturingData.Add("Port : " + req.Port);
                    Dictionary<string, string> reqParams = req.RequestParams;
                    foreach (KeyValuePair<string, string> entry in reqParams)
                    {                        
                        Console.WriteLine("Key : " + entry.Key + " = Value : " + entry.Value);
                        capturingData.Add("Key : " + entry.Key + " = Value : " + entry.Value);
                    }
                    if (req.Response != null)
                    {
                        int resStatusCode = req.Response.StatusCode;
                        Console.WriteLine("Response status code : " + resStatusCode);
                        capturingData.Add("Response status code : " + resStatusCode);                        
                    }
                    else
                    {
                        Console.WriteLine("Didn't get response for request number : " + req.Id);
                        capturingData.Add("Didn't get response for request number : " + req.Id);
                    }
                    Console.WriteLine("End of request --------------------------");
                    capturingData.Add("End of request --------------------------");
                    Console.WriteLine("\n\n\n");
                    capturingData.Add("\n");
                }
            }
            writeCapturingLog();
        }

        private static void writeCapturingLog()
        {            
            if (!Directory.Exists(snifferLogsDir))
            {
                Directory.CreateDirectory(snifferLogsDir);
            }
            File.WriteAllText(snifferLogsDir + "\\CapturingLog.txt", convertCapturingData());
        }

        private static string convertCapturingData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in capturingData)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}
