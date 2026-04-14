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
        public string UserDetailId { get; set; }


    }
    public class EkycViaOTP
    {
        public string uidType { get; set; }
        public string strAadhaarNo { get; set; }

        public string OTP { get; set; }

        public string txnId { get; set; }

        public string customerId { get; set; }
        public string UserDetailId { get; set; }
    }

    public class EkycViaBiometric
    {
        public string uidType { get; set; }
        public string strAadhaarNo { get; set; }
        public string strEncryptedSKey { get; set; }
        public string encryptedPID { get; set; }
        public string sha256ofPidXML { get; set; }
        public string rdsId { get; set; }
        public string rdsVer { get; set; }
        public string mi { get; set; }
        public string mc { get; set; }
        public string dpid { get; set; }
        public string dc { get; set; }
        public string ci { get; set; }
        public string strTerminalId { get; set; }
        public string ts { get; set; }
        public string strTransactionId { get; set; }
        public string BiometricType { get; set; }

        public string ApplicationtxnId { get; set; }
        public string customerId { get; set; }

        public string UserDetailId { get; set; }
        public string Consent { get; set; }
        public string vendorId { get; set; }
        public string vendorPassword { get; set; }
        public string IPAddress { get; set; }






    }

}