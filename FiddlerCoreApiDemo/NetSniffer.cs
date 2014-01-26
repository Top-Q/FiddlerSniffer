using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using FiddlerCoreApiDemo.HttpClasses;
using System.Linq.Expressions;
using System.IO;

namespace FiddlerCoreApiDemo
{
    public enum ELogicFiltersConcatination
    {
        AND, OR
    }

    public class NetSniffer
    {        
        private int port;
        private bool captureHTTPS;
        private bool registerAsSystemProxy;
        private Dictionary<int, HttpRequest> httpRequestsDictionary;
        private List<HttpRequest> httpRequests;
        private List<string> filters;

        public NetSniffer(int port, bool captureHTTPS, bool registerAsSystemProxy)
        {
            this.port = port;
            this.captureHTTPS = captureHTTPS;
            this.registerAsSystemProxy = registerAsSystemProxy;
            httpRequests = new List<HttpRequest>();
            httpRequestsDictionary = new Dictionary<int, HttpRequest>();
            filters = new List<string>();
            attachEventListeners();
        }       

        public void StartCapturing()
        {
            Console.WriteLine("Starting Fiddler core...");
            try
            {
                Fiddler.FiddlerApplication.Startup(port, FiddlerCoreStartupFlags.Default);
            }
            catch (Exception e)
            {                
                Console.WriteLine("Could not start Fidller Core.");
            }
        }

        private void attachEventListeners()
        {
            Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS)
            {
                HttpRequest request = new HttpRequest(oS);                
                httpRequestsDictionary.Add(oS.id, request);
            };

            Fiddler.FiddlerApplication.BeforeResponse += delegate(Fiddler.Session oS)
            {
                HttpResponse response = new HttpResponse(oS);
                int sessionId = oS.id;
                HttpRequest req;
                if (httpRequestsDictionary.TryGetValue(sessionId, out req))
                {
                    req.Response = response;
                }
            };
        }        

        public void ShutDown()
        {
            Console.WriteLine("Shutting down....");
            try
            {
                Fiddler.FiddlerApplication.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine("Shut down failed. " + e.Message);
            }
        }

        //public string DecryptString(string encryptedString)
        //{
        //    string decryptedString = String.Empty;

        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream(HttpServerUtility.UrlTokenDecode(encryptedString)))
        //        {
        //            using (var zip = new Ionic.Zlib.DeflateStream(ms, Ionic.Zlib.CompressionMode.Decompress, CompressionLevel.BestCompression, true))
        //            {
        //                using (BinaryReader reader = new BinaryReader(zip))
        //                {
        //                    decryptedString = Encoding.UTF8.GetString(reader.ReadBytes(10000));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //ignore
        //    }
        //    return decryptedString;
        //}

        public List<HttpRequest> GetFilltredCapturedTraffic(ELogicFiltersConcatination filtersConcatination)
        {
            buildFullHttpRequestListFromDictionary();
            List<HttpRequest> fillteredData = new List<HttpRequest>();
            string filter = ConcatFilters(filtersConcatination);
            Expression<Func<HttpRequest, bool>> e;

            if (filter == string.Empty)
            {
                return httpRequests;
            }

            try
            {
                e = System.Linq.Dynamic.DynamicExpression.ParseLambda<HttpRequest, bool>(filter);
            }
            catch (Exception)
            {
                // Failed to construct lambda expression from string.
                return null;
            }

            // construct query.
            var numQuery =
                from request in httpRequests
                where e.Compile()(request)
                select request;

            //  Execute query.
            foreach (HttpRequest request in numQuery)
            {
                // Added each filltred in request.
                fillteredData.Add(request);
            }

            return fillteredData;
        }

        private void buildFullHttpRequestListFromDictionary()
        {
            foreach (KeyValuePair<int, HttpRequest> entry in httpRequestsDictionary)
            {
                httpRequests.Add(entry.Value);
            }
        }

        /// <summary>
        /// Creates one long string every filter concated by ANDS.
        /// </summary>
        /// <returns>a string query representing all filters.</returns>
        private string ConcatFilters(ELogicFiltersConcatination filtersConcatination)
        {
            StringBuilder concatFilter = new StringBuilder();
            string filtersConcatinationString = String.Empty;
            int lastConcatinationLen = 0;

            switch (filtersConcatination)
            {
                case ELogicFiltersConcatination.AND:
                    filtersConcatinationString = " and ";
                    lastConcatinationLen = 5;
                    break;
                case ELogicFiltersConcatination.OR:
                    filtersConcatinationString = " or ";
                    lastConcatinationLen = 4;
                    break;
                default:
                    break;
            }

            if (filters.Count > 0)
            {
                foreach (string filter in filters)
                {
                    concatFilter.Append(filter);
                    //concatFilter.Append(" and ");
                    concatFilter.Append(filtersConcatinationString);
                }

                // Remove last appended " and ".
                //concatFilter.Remove(concatFilter.Length - 5, 5);
                concatFilter.Remove(concatFilter.Length - lastConcatinationLen, lastConcatinationLen);
            }
            return concatFilter.ToString();
        }

        public void AddFilter(string filter)
        {
            if (filters.Contains(filter))
            { return; }

            filters.Add(filter);
        }

        public void RemoveFilter(string filter)
        {
            if (filters.Contains(filter))
            {
                filters.Remove(filter);
            }
        }

        public List<string> Filters
        {
            get { return filters; }
            set { filters = value; }
        }
    }
}
