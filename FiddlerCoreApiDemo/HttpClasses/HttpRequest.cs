using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FiddlerCoreApiDemo.HttpClasses
{
    public enum EMethod
    {
        GET, POST
    }

    [DataContract]
    public class HttpRequest
    {
        #region DataMembers
        [DataMember]
        private int port;
        [DataMember]
        private EMethod method;
        [DataMember]
        private string domain;
        [DataMember]
        private string body;
        [DataMember]
        private string query;
        [IgnoreDataMember]
        private Dictionary<string, string> requestParams;
        [DataMember]
        private HttpResponse response;
        [DataMember]
        private int id;
        #endregion

        #region C'tor
        public HttpRequest(Fiddler.Session oS)
        {
            id = oS.id;
            string fullUri = oS.fullUrl;
            Uri uri = new Uri(fullUri);
            port = uri.Port;
            domain = uri.GetLeftPart(UriPartial.Path);            
            string reqBody = oS.GetRequestBodyAsString();
            requestParams = new Dictionary<string, string>();
            if (reqBody.Length != 0)
            {
                method = EMethod.POST;
                body = reqBody;
                extractParams(body);
            }
            else
            {
                method = EMethod.GET;
                query = uri.Query;
                extractParams(query);
            }            
            
        }
        #endregion

        #region Private methods
        private void extractParams(string q)
        {
            string[] lines = q.Split('&');
            foreach (string line in lines)
            {
                string tempLine = line;
                if (tempLine.Contains("?"))
                {
                    tempLine = tempLine.Replace("?", "");
                }
                if (tempLine.Contains("="))
                {
                    string[] lineParts = tempLine.Split('=');
                    requestParams.Add(Uri.UnescapeDataString(lineParts[0]), Uri.UnescapeDataString(lineParts[1]));
                }
            }
        }
        #endregion

        #region Properties
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public Dictionary<string, string> RequestParams
        {
            get { return requestParams; }            
        }

        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        public EMethod Method
        {
            get { return method; }
            set { method = value; }
        }

        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        public HttpResponse Response
        {
            get { return response; }
            set { response = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        #endregion
    }
}
