using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Models
{
    public class PollingInput
    {
        public string TxnId { get; set; }

        public string MsgTs { get; set; }
        public string UserId { get; set; }
    }
}