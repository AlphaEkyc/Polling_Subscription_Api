using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Models
{
    public class SubscriptionProtocolResponse
    {
        public ResponseBlock Response { get; set; }
        public string TxnId { get; set; }
        public string RespTimestamp { get; set; }
    }

    public class ResponseBlock
    {
        public string Signature { get; set; }
        public ResponseHeader Header { get; set; }
        public ResponseMsg Msg { get; set; }
    }

    public class ResponseHeader
    {
        public string Ver { get; set; }
        public string MsgId { get; set; }
        public string MsgTs { get; set; }
        public string Ac { get; set; }
        public string Sa { get; set; }
        public string Action { get; set; }
        public bool IsMessageEncrypted { get; set; }
    }

    public class ResponseMsg
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }


    public class PollingResult
    {
        public string TxnId { get; set; }
        public string RecordPending { get; set; }
        public List<UidStatus> Uids { get; set; } = new List<UidStatus>();
    }

    public class UidStatus
    {
        public string Token { get; set; }
        public string Status { get; set; }
    }

}