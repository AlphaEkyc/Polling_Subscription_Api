using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AadhaarStatusService.Api.Models.Dto
{
    public class ProteanSubscriptionRequest
    {
        [JsonPropertyName("data")]
        public DataBlock Data { get; set; } = default;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    public class DataBlock
    {
        [JsonPropertyName("signature")]
        public string Signaturee { get; set; } = string.Empty;

        [JsonPropertyName("header")]
        public HeaderBlock Header { get; set; } = default;

        [JsonPropertyName("msg")]
        public MsgBlock Msg { get; set; } = default;
    }

    public class HeaderBlock
    {
        [JsonPropertyName("ver")]
        public string Ver { get; set; } = string.Empty;

        [JsonPropertyName("msgId")]
        public string MsgId { get; set; } = string.Empty;

        [JsonPropertyName("msgTs")]
        public string MsgTs { get; set; } = string.Empty;

        [JsonPropertyName("ac")]
        public string Ac { get; set; } = string.Empty;

        [JsonPropertyName("sa")]
        public string Sa { get; set; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("isMessageEncrypted")]
        public bool IsMessageEncrypted { get; set; }

        [JsonPropertyName("lk")]
        public string Lk { get; set; } = string.Empty;
    }

    public class MsgBlock
    {
        [JsonPropertyName("notifyEndpoint")]
        public string NotifyEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("schedule")]
        public string Schedule { get; set; } = string.Empty;

        [JsonPropertyName("UserId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("txxnId")]
        public string TxnId { get; set; } = string.Empty;


    }

    public class UidaiMsg
    {
        public string notifyEndpoint { get; set; }
        public string startDate { get; set; }
        public string schedule { get; set; }
    }


}
