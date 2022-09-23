using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Models
{
    public class clsEkyc
    {




    }

    public class GetOTP
    {
        public string uidType { get; set; }
        public string strAadhaarNo { get; set; }

        public string txnId { get; set; }

        public string customerId { get; set; }
        
      
    }
    public class EkycViaOTP
    {
        public string uidType { get; set; }
        public string strAadhaarNo { get; set; }

        public string OTP { get; set; }

        public string txnId { get; set; }

        public string customerId { get; set; }
    }

}