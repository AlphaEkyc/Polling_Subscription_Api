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
            string ErrorCode = string.Empty;
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
                ErrorCode= objDictionary["ErrorCode"];
            }
                


            var GetOTPResponse = new
            {

                status = status,
                txnId = ReqParam.txnId,

                ErrorCode = ErrorCode,   // return Error cod from ekyc service

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
            string ErrorCode = string.Empty;
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
                ErrorCode = objDictionary["ErrorCode"];
            }


            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            //XmlElement EkycResponse = xmlDoc.DocumentElement;

            //string txn = EkycResponse.Attributes["txn"].Value;
            var AuthViaOTPResponse = new
            {

                status = status,
                //Txn = txn,
               

                ErrorCode = ErrorCode,

                ErrorMsg = ErrorMsg,


            };

            string AuthViaOTPResp = JsonConvert.SerializeObject(AuthViaOTPResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(AuthViaOTPResp, Encoding.UTF8, "application/json")
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
            string TxnID = string.Empty;

            string  AadhaarNumber = string.Empty;
            string Name = string.Empty;
            string DOB = string.Empty;
            string Gender = string.Empty;
            string Phone = string.Empty;
            string Email = string.Empty;
            string CareOfPerson = string.Empty;
            string Landmark = string.Empty;
            string House = string.Empty;
            string Locality = string.Empty;
            string City = string.Empty;
            string Street = string.Empty;
            string District = string.Empty;
            string SubDistrict = string.Empty;
            string State = string.Empty;
            string PinCode = string.Empty;
            string PostOfficeName = string.Empty;
            string Photo = string.Empty;
            string Country = string.Empty;
            string AadhaarPrint = string.Empty;
            string TransactionCode = string.Empty;
            string TimeToLive = string.Empty;
            string UIDToken = string.Empty;
            string customerId = string.Empty;


            Dictionary<string, string> objDictionary;


            json_data = JsonConvert.SerializeObject(EkycViaOTPRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaOTP>(json_data);

            objDictionary = objser.EkycViaOTP(ReqParam.uidType, ReqParam.strAadhaarNo,ReqParam.OTP, ReqParam.txnId,"", ReqParam.customerId, "", 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");


           

            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(objDictionary["XmlResponse"]);
                XmlElement EkycResponse = xmlDoc.DocumentElement;

                TxnID = EkycResponse.Attributes["txn"].Value;

              


                var EkycViaOTPResponse = new
                {

                    status = status,
                    txnId = TxnID,
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
                    customerId = ReqParam.customerId,
                    

                };

                string EkycViaOTPResponseResp = JsonConvert.SerializeObject(EkycViaOTPResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(EkycViaOTPResponseResp, Encoding.UTF8, "application/json")
                };






            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];

                var EkycViaOTPResponse = new
                {

                    status = status,
                   
                     ErrorCode = objDictionary["ErrorCode"],  //Return ErrorCode from Ekyc service

                    ErrorMsg = ErrorMsg,


                };
                string EkycViaOTPResp = JsonConvert.SerializeObject(EkycViaOTPResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(EkycViaOTPResp, Encoding.UTF8, "application/json")
                };

            }


           
           
        }


        [HttpPost]
        [Route("api/Ekyc/EkycViaBiometric")]
        public HttpResponseMessage EkycViaBiometric(JObject EkycViaBiometricRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;
            string TxnID = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(EkycViaBiometricRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);

            objDictionary = objser.EkycViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc,ReqParam.dpid,ReqParam.dc,ReqParam.ci,ReqParam.strTerminalId,ReqParam.ts,ReqParam.strTransactionId,"101",ReqParam.BiometricType, ReqParam.customerId,0,"Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");





            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(objDictionary["XmlResponse"]);
                XmlElement EkycResponse = xmlDoc.DocumentElement;

                TxnID = EkycResponse.Attributes["txn"].Value;




                var EkycViaBioResponse = new
                {

                    status = status,
                    txnId = TxnID,
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
                    customerId = ReqParam.customerId,


                };

                string EkycViaBioResp = JsonConvert.SerializeObject(EkycViaBioResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(EkycViaBioResp, Encoding.UTF8, "application/json")
                };






            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];

                var EkycViaBioResponse = new
                {

                    status = status,

                    ErrorCode = objDictionary["ErrorCode"],  //Return ErrorCode from Ekyc service

                    ErrorMsg = ErrorMsg,


                };
                string EkycViaBioResp = JsonConvert.SerializeObject(EkycViaBioResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(EkycViaBioResp, Encoding.UTF8, "application/json")
                };

            }




        }


        [HttpPost]
        [Route("api/Ekyc/AuthViaBiometric")]
        public HttpResponseMessage AuthViaBiometric(JObject AuthViaBioRequest)
        {
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;
            string ErrorCode = string.Empty;
            string TxnID = string.Empty;
            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(AuthViaBioRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);

            objDictionary = objser.AuthenticationViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc, ReqParam.dpid, ReqParam.dc, ReqParam.ci, ReqParam.strTerminalId, ReqParam.ts, ReqParam.strTransactionId, "101", ReqParam.BiometricType, ReqParam.customerId, 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";

            }
            else
            {
                status = "Fail";
                ErrorMsg = objDictionary["Error"];
                ErrorCode= objDictionary["ErrorCode"];
            }


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            XmlElement EkycResponse = xmlDoc.DocumentElement;

             TxnID = EkycResponse.Attributes["txn"].Value;
            var GetAuthResponse = new
            {

                status = status,
                

                txnId = TxnID,
               

                ErrorCode = ErrorCode,

                ErrorMsg = ErrorMsg,


            };

            string GetAuthResp = JsonConvert.SerializeObject(GetAuthResponse);


            return new HttpResponseMessage()
            {
                Content = new StringContent(GetAuthResp, Encoding.UTF8, "application/json")
            };


        }



    }
}
