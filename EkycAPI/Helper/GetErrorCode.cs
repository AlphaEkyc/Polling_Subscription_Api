using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Helper
{
    public class GetErrorCode
    {

       public static  string GetErrorMessage(string errorCode)
        {
            switch (errorCode)
            {
                case "0":
                    return "Success";

                case "A10":
                    return "Invalid JSON Format or datatype validation failed";

                case "P10":
                    return "Validation failed for the Polling API";

                case "P11":
                    return "Validation constraint violated for polling request data";

                case "P12":
                    return "Error during Digital Signature Verification";

                case "P13":
                    return "DB audit failed for request";

                case "P14":
                    return "DB audit failed for response";

                case "P15":
                    return "Duplicate msgID. msgID already exists in DB";

                case "P16":
                    return "Error during IP Address Verification";

                case "P17":
                    return "Error during parsing Polling JSON Response";

                case "P18":
                    return "Error during sending polling request to UIDAI";

                case "P19":
                    return "HMAC validation failed for Header + Message";

                case "P20":
                    return "Invalid AUA Code";

                case "P21":
                    return "Invalid SUBAUA Code";

                case "P22":
                    return "Invalid License Key";

                case "P23":
                    return "Invalid Version";

                case "P24":
                    return "Invalid Message ID";

                case "P25":
                    return "Invalid Message Timestamp";

                case "P26":
                    return "Invalid Action";

                case "P27":
                    return "Invalid Transaction ID";

                case "P28":
                    return "Invalid Request HMAC";

                case "P29":
                    return "Invalid Message Data";

                case "P30":
                    return "Invalid Algorithm";

                case "P31":
                    return "Invalid Encryption";

                case "P32":
                    return "Invalid Request Session Key";

                case "P33":
                    return "Invalid Thumbprint";

                case "P34":
                    return "Invalid IV";

                case "P35":
                    return "Polling JSON Schema validation failed";

                default:
                    return $"Unknown error code: {errorCode}";
            }
          }
      }
}