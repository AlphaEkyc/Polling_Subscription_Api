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
        [Route("api/Ekyc/AuthViaOTP")]
        public HttpResponseMessage AuthViaOTP(JObject AuthViaOTPRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(AuthViaOTPRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaOTP>(json_data);

            objDictionary = objser.AuthenticationViaOTP(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.OTP, ReqParam.txnId, "", ReqParam.customerId, "", 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";

            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
            }


            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            //XmlElement EkycResponse = xmlDoc.DocumentElement;

            //string txn = EkycResponse.Attributes["txn"].Value;
            var GetOTPResponse = new
            {

                status = status,
                //Txn = txn,
                message = objDictionary["Success"],

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


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            XmlElement EkycResponse = xmlDoc.DocumentElement;

            string txn = EkycResponse.Attributes["txn"].Value;
            var GetOTPResponse = new
            {

                status = status,
                //txnId = ReqParam.txnId,
                //  name = ReqParam.
                

            Txn = txn,
            AadhaarNumber = objDictionary["AadhaarNumber"],
               Name = objDictionary["Name"],
                DOB = objDictionary["DOB"],
               Gender = objDictionary["Gender"],
               Phone = objDictionary["Phone"],
                Email = objDictionary["Email"],
                CareOfPerson = objDictionary["CareOfPerson"],
               Landmark = objDictionary["Landmark"],
                House = objDictionary["House"],
                Locality = objDictionary["Locality"],
                City = objDictionary["city"],
                Street = objDictionary["Street"],
                District = objDictionary["District"],
                SubDistrict = objDictionary["SubDistrict"],
                State = objDictionary["State"],
                PinCode = objDictionary["PinCode"],
                PostOfficeName = objDictionary["PostOfficeName"],
                Photo = objDictionary["Photo"],
                Country = objDictionary["Country"],
                AadhaarPrint = objDictionary["AadhaarPrint"],
                TransactionCode = objDictionary["TransactionCode"],
                TimeToLive = objDictionary["TimeTolive"],
                UIDToken = objDictionary["UIDToken"],
                Customerid = objDictionary["cbsCustomerID"],

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
        [Route("api/Ekyc/Biometric")]
        public HttpResponseMessage EkycViaBiometric(JObject EkycViaBiometricRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(EkycViaBiometricRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);

            objDictionary = objser.EkycViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc,ReqParam.dpid,ReqParam.dc,ReqParam.ci,ReqParam.strTerminalId,ReqParam.ts,ReqParam.strTransactionId,"101",ReqParam.BiometricType, ReqParam.customerId,0,"Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";

            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
            }


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            XmlElement EkycResponse = xmlDoc.DocumentElement;

            string txn = EkycResponse.Attributes["txn"].Value;
            var GetOTPResponse = new
            {

                status = status,
                //txnId = ReqParam.txnId,
                //  name = ReqParam.


                Txn = txn,
                AadhaarNumber = objDictionary["AadhaarNumber"],
                Name = objDictionary["Name"],
                DOB = objDictionary["DOB"],
                Gender = objDictionary["Gender"],
                Phone = objDictionary["Phone"],
                Email = objDictionary["Email"],
                CareOfPerson = objDictionary["CareOfPerson"],
                Landmark = objDictionary["Landmark"],
                House = objDictionary["House"],
                Locality = objDictionary["Locality"],
                City = objDictionary["city"],
                Street = objDictionary["Street"],
                District = objDictionary["District"],
                SubDistrict = objDictionary["SubDistrict"],
                State = objDictionary["State"],
                PinCode = objDictionary["PinCode"],
                PostOfficeName = objDictionary["PostOfficeName"],
                Photo = objDictionary["Photo"],
                Country = objDictionary["Country"],
                AadhaarPrint = objDictionary["AadhaarPrint"],
                TransactionCode = objDictionary["TransactionCode"],
                TimeToLive = objDictionary["TimeTolive"],
                UIDToken = objDictionary["UIDToken"],
                Customerid = objDictionary["cbsCustomerID"],

                ErrorCode = "",

                ErrorMsg = ErrorMsg,


            };

            string GetEkycResp = JsonConvert.SerializeObject(GetOTPResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(GetEkycResp, Encoding.UTF8, "application/json")
            };


        }


        [HttpPost]
        [Route("api/Ekyc/Biometric")]
        public HttpResponseMessage AuthViaBiometric(JObject EkycViaBiometricRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(EkycViaBiometricRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);

            objDictionary = objser.EkycViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc, ReqParam.dpid, ReqParam.dc, ReqParam.ci, ReqParam.strTerminalId, ReqParam.ts, ReqParam.strTransactionId, "101", ReqParam.BiometricType, ReqParam.customerId, 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";

            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
            }


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            XmlElement EkycResponse = xmlDoc.DocumentElement;

            string txn = EkycResponse.Attributes["txn"].Value;
            var GetOTPResponse = new
            {

                status = status,
                //txnId = ReqParam.txnId,
                //  name = ReqParam.


                Txn = txn,
                AadhaarNumber = objDictionary["AadhaarNumber"],
                Name = objDictionary["Name"],
                DOB = objDictionary["DOB"],
                Gender = objDictionary["Gender"],
                Phone = objDictionary["Phone"],
                Email = objDictionary["Email"],
                CareOfPerson = objDictionary["CareOfPerson"],
                Landmark = objDictionary["Landmark"],
                House = objDictionary["House"],
                Locality = objDictionary["Locality"],
                City = objDictionary["city"],
                Street = objDictionary["Street"],
                District = objDictionary["District"],
                SubDistrict = objDictionary["SubDistrict"],
                State = objDictionary["State"],
                PinCode = objDictionary["PinCode"],
                PostOfficeName = objDictionary["PostOfficeName"],
                Photo = objDictionary["Photo"],
                Country = objDictionary["Country"],
                AadhaarPrint = objDictionary["AadhaarPrint"],
                TransactionCode = objDictionary["TransactionCode"],
                TimeToLive = objDictionary["TimeTolive"],
                UIDToken = objDictionary["UIDToken"],
                Customerid = objDictionary["cbsCustomerID"],

                ErrorCode = "",

                ErrorMsg = ErrorMsg,


            };

            string GetAuthResp = JsonConvert.SerializeObject(GetOTPResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(GetAuthResp, Encoding.UTF8, "application/json")
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
