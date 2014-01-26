using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FiddlerCoreApiDemo.HttpClasses
{
    [DataContract]
    public class HttpResponse
    {
        [DataMember]
        private int statusCode;
        [IgnoreDataMember]
        private Encoding contentType;
        [DataMember]
        private string data;

        public HttpResponse(Fiddler.Session oS)
        {
            statusCode = oS.responseCode;
            data = oS.GetResponseBodyAsString();
            contentType = oS.GetResponseBodyEncoding();
        }

        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public Encoding ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        public string Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
