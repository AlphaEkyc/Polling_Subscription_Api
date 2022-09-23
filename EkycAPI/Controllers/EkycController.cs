using EkycAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;

namespace EkycAPI.Controllers
{
    public class EkycController : ApiController
    {
        // GET api/values
        [HttpPost]
        [Route("api/Ekyc/GetOTP")]
        public HttpResponseMessage GetOTP(JObject GetOTPRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();
            
            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;
            Dictionary<string, string> objDictionary;

          json_data = JsonConvert.SerializeObject(GetOTPRequest);

            var ReqParam = JsonConvert.DeserializeObject<GetOTP>(json_data);

            objDictionary =  objser.GetOTP(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.txnId, ReqParam.customerId, "", 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");

            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";
            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
            }
                


            var GetOTPResponse = new
            {

                status = status,
                txnId = ReqParam.txnId,

                ErrorCode = "",

                ErrorMsg = ErrorMsg,


            };

            string GetOTPResp = JsonConvert.SerializeObject(GetOTPResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(GetOTPResp, Encoding.UTF8, "application/json")
            };


        }


        [HttpPost]
        [Route("api/Ekyc/EkycViaOTP")]
        public HttpResponseMessage EkycViaOTP(JObject EkycViaOTPRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(EkycViaOTPRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaOTP>(json_data);

            objDictionary = objser.EkycViaOTP(ReqParam.uidType, ReqParam.strAadhaarNo,ReqParam.OTP, ReqParam.txnId,"", ReqParam.customerId, "", 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");


           

            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";

            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
            }



            var GetOTPResponse = new
            {

                status = status,
                txnId = ReqParam.txnId,
                
                ErrorCode = "",

                ErrorMsg = ErrorMsg,


            };

            string GetOTPResp = JsonConvert.SerializeObject(GetOTPResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(GetOTPResp, Encoding.UTF8, "application/json")
            };


        }


        [HttpPost]
        [Route("api/Ekyc/xml")]
        public HttpResponseMessage xml(string xml)
        {
            return null;
        }

    }
}
