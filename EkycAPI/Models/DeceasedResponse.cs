using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Models
{
    public class DeceasedResponse
    {
        public string id { get; set; }

        public string version { get; set; }

        public string requestTime { get; set; }

        public string transactionId { get; set; }

        public string thumbprint { get; set; }

        public string sessionKey { get; set; }

        public string hmac { get; set; }

        public string response { get; set; }

        public string signature { get; set; }

        public DeceasedError error { get; set; }
    }


    public class DeceasedError
    {
        public string code { get; set; }

        public string message { get; set; }
    }
}