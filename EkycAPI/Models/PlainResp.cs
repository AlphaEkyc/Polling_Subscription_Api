using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Models
{
    public class PlainResp
    {
        public string sid { get; set; }

        public string status { get; set; }

        public string rejectCode { get; set; }

        public string udrn { get; set; }
    }
}