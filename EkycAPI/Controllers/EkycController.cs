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
//using System.Web.Http.Routing;

namespace EkycAPI.Controllers
{
    public class EkycController : ApiController
    {
        readonly string stan = new Random().Next(111111, 999999).ToString();
        string reqst;
        // GET api/values
        [HttpPost]
        [Route("api/Ekyc/GetOTP")]
        public HttpResponseMessage GetOTP(JObject GetOTPRequest)
        {
            try
            {
                string txnId = DateTime.Now.ToString("yyyyMMddTHHmmssfff") + stan;
                EkycService.Service1Client objser = new EkycService.Service1Client();

                string json_data = string.Empty;
                string status = string.Empty;
                string ErrorMsg = string.Empty;
                string ErrorCode = string.Empty;

                Dictionary<string, string> objDictionary;

                json_data = JsonConvert.SerializeObject(GetOTPRequest);

                var ReqParam = JsonConvert.DeserializeObject<GetOTP>(json_data);

                objDictionary = objser.GetOTP(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.txnId,"", ReqParam.customerId, "", "1", "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");

                if (objDictionary.ContainsKey("Success"))
                {
                    status = "Success";
                    txnId = objDictionary["TransactionId"];
                }
                else
                {
                    if (objDictionary.ContainsKey("ErrorCode"))
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                        ErrorCode = objDictionary["ErrorCode"];
                    }
                    else
                    {
                        status = "Fail";
                      ErrorMsg = objDictionary["Error"]; 
                    }


                }



                var GetOTPResponse = new
                {

                    status = status,
                    //txnId = ReqParam.txnId,
                    //  txnId = objDictionary["TransactionId"],
                    txnId= txnId,
                    ErrorCode = ErrorCode,   // return Error cod from ekyc service

                    ErrorMsg = ErrorMsg,


                };

                string GetOTPResp = JsonConvert.SerializeObject(GetOTPResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(GetOTPResp, Encoding.UTF8, "application/json")
                };

            }
            catch (Exception ex)
            {
                string strException = ex.Message + " StackTrace = " + ex.StackTrace + " Method = " + ex.TargetSite + "InnerException = " + ex.InnerException;
                //dbentity.USP_EKYCResponseLog(strAadhaarNo1, txnId1, customerId1, status, ErrorCode, ErrorMsg, " ", ex.ToString(), DateTime.Now.ToString());
                return new HttpResponseMessage()
                {


                    Content = new StringContent(strException, Encoding.UTF8, "application/json")
                };

            }

        }


        
        [HttpPost]
        [Route("api/Ekyc/AuthViaOTP")]
        public HttpResponseMessage AuthViaOTP(JObject AuthViaOTPRequest)
        {
            try
            {
                EkycService.Service1Client objser = new EkycService.Service1Client();

                string json_data = string.Empty;
                string status = string.Empty;
                string ErrorMsg = string.Empty;
                string ErrorCode = string.Empty;
                string Result = string.Empty;
                Dictionary<string, string> objDictionary;

                json_data = JsonConvert.SerializeObject(AuthViaOTPRequest);

                var ReqParam = JsonConvert.DeserializeObject<EkycViaOTP>(json_data);

                objDictionary = objser.AuthenticationViaOTP(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.OTP, ReqParam.txnId, "", ReqParam.customerId, "", "0", "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




                if (objDictionary.ContainsKey("Success"))
                {
                    status = "Success";
                    Result = objDictionary["Success"];

                }
                else
                {
                    if (objDictionary.ContainsKey("ErrorCode"))
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                        ErrorCode = objDictionary["ErrorCode"];
                    }
                    else
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                    }
                }


                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.LoadXml(objDictionary["XmlResponse"]);
                //XmlElement EkycResponse = xmlDoc.DocumentElement;

                //string txn = EkycResponse.Attributes["txn"].Value;
                var AuthViaOTPResponse = new
                {

                    status = status,
                    //Txn = txn,

                    statusmsg = Result,
                    ErrorCode = ErrorCode,

                    ErrorMsg = ErrorMsg,


                };

                string AuthViaOTPResp = JsonConvert.SerializeObject(AuthViaOTPResponse);


                return new HttpResponseMessage()
                {
                    Content = new StringContent(AuthViaOTPResp, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                string strException = ex.Message + " StackTrace = " + ex.StackTrace + " Method = " + ex.TargetSite + "InnerException = " + ex.InnerException;
                //dbentity.USP_EKYCResponseLog(strAadhaarNo1, txnId1, customerId1, status, ErrorCode, ErrorMsg, " ", ex.ToString(), DateTime.Now.ToString());
                return new HttpResponseMessage()
                {


                    Content = new StringContent(strException, Encoding.UTF8, "application/json")
                };

            }

        }


        [HttpPost]
        [Route("api/Ekyc/EkycViaOTP")]
        public HttpResponseMessage EkycViaOTP(JObject EkycViaOTPRequest)
        {
            try
            {
                EkycService.Service1Client objser = new EkycService.Service1Client();

                string json_data = string.Empty;
                string status = string.Empty;
                string ErrorMsg = string.Empty;
                string TxnID = string.Empty;
                string ErrorCode = string.Empty;

                string AadhaarNumber = string.Empty;
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
                //string TransactionCode = string.Empty;
                string TimeToLive = string.Empty;
                string UIDToken = string.Empty;
                string customerId = string.Empty;


                Dictionary<string, string> objDictionary;


                json_data = JsonConvert.SerializeObject(EkycViaOTPRequest);

                var ReqParam = JsonConvert.DeserializeObject<EkycViaOTP>(json_data);

                objDictionary = objser.EkycViaOTP(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.OTP, ReqParam.txnId, "", ReqParam.customerId, "", "1", "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




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
                        //TxnID = objDictionary["TransactionId"],
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
                    // string ErrorCode
                    if (objDictionary.ContainsKey("ErrorCode"))
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                        ErrorCode = objDictionary["ErrorCode"];
                    }
                    else
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                    }

                    var EkycViaOTPResponse = new
                    {

                        status = status,

                        // ErrorCode = objDictionary["ErrorCode"],  //Return ErrorCode from Ekyc service
                        ErrorCode = ErrorCode,
                        ErrorMsg = ErrorMsg,


                    };
                    string EkycViaOTPResp = JsonConvert.SerializeObject(EkycViaOTPResponse);


                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(EkycViaOTPResp, Encoding.UTF8, "application/json")
                    };
                }
            }
            catch (Exception ex)
            {
                string strException = ex.Message + " StackTrace = " + ex.StackTrace + " Method = " + ex.TargetSite + "InnerException = " + ex.InnerException;
                //dbentity.USP_EKYCResponseLog(strAadhaarNo1, txnId1, customerId1, status, ErrorCode, ErrorMsg, " ", ex.ToString(), DateTime.Now.ToString());
                return new HttpResponseMessage()
                {


                    Content = new StringContent(strException, Encoding.UTF8, "application/json")
                };

            }

        }


           
           
        


        [HttpPost]
        [Route("api/Ekyc/EkycViaBiometric")]
        public HttpResponseMessage EkycViaBiometric(JObject EkycViaBiometricRequest)
        {
            reqst = EkycViaBiometricRequest.ToString();
            //string filename1 = @"D:\request";
            //System.IO.File.AppendAllText(filename1, reqst);
           
              //  ExceptionLogging.SendErrorToText(reqst);
           
            try
            { 
            EkycService.Service1Client objser = new EkycService.Service1Client();

            string json_data = string.Empty;
            string status = string.Empty;
            string ErrorMsg = string.Empty;
            string TxnID = string.Empty;
            string ErrorCode = string.Empty;

            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(EkycViaBiometricRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                objDictionary = objser.EkycViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc,ReqParam.dpid,ReqParam.dc,ReqParam.ci,ReqParam.strTerminalId,ReqParam.ts,ReqParam.strTransactionId,"101",ReqParam.BiometricType, ReqParam.customerId,ReqParam.UserDetailId, ReqParam.Consent, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");
               // objDictionary = objser.EkycViaBiometric("Aadhaar", "490886247732", "HL3zOql3/S6/t0nUeENpaclhUesMKNyNlENxMQySR5FAh957AAXRhXwgeyDQ8qt+yGp3xBSLHDn3voHG9wSRyqWHqb8GPc54yxGKvHcgUvPSYUccHex4zB9qR/ODxoik0djo77fL93bYQSZH63rWco5/OyDpv6DB97k9p/SY0iDnUGc88sseIPsvBL1FoNt3SNh6TylREozedql5ktXHyibtiqRs5mXXEpNbK2G7PdfTE/stsews2Rw2cHFDFfdnUrPRteJ8wJzUPGo9HsTbS16GDBLTSLmxStJKaLLr3qSEaNk6xGscuSJ5WOYG+kBJ9rZQeILuVvHby18wchQB6Q==", "MjAyMi0xMC0xN1QxNDowNToyN2Ic+t7IW1OTdCN7sMrN+nuGjKNCok285s55EDn/kBzOLMZg11kSYBd+V6V3ZfedtxSZM/B5mEc3GPVNIHois8XiSdfXbQDtdpC1odI7Mc1IIyWJrknYIuvPIFKRN4gwE0jqWsc4C5kyw3C3ubEe6+Tu6OzBM8mUoUmYUQ7OSXTYzM6hXhbMUX/JQWoFvgBAq0Z7HdZbpnsAm948ZVB57f9Uq80lxw9F+WVG/Uz2TP9G435gkuRRJqjkGqsn4Zg4HFmySlL6+4yA34cn94n7W7ha/ifwGtxbNZVScVO0gW8rGsM12v7YJ1le5ByDWvRjWVvP845DaapRk2xhwKE2dC0ESOx60i43Wp8MLDWUKcBrtw/llnv7/N1lMPjSU9roPkesMB84iSPesRcAAzUqOhDOO5UoqzUb8sba7JNbsIL0K3ApG6jTAk4OwkuIAE5PfnCl1IxGwXDUSiqSTBmRDr8x7/6mEo1+5AC35eSkZL/7aZ76jsb5uStD2x7hvj3asDpMyyol113mdgRfrf2cd95bu7yOsGFPTlIy+oQ8T7JAG4dcTAgW6xNGcYv0J9QwKRrRpop6Y8aKiyuT2wDcujByIBwk/yW3K/Jm8IS3UH53neYQS2nOw92LO5AGSVOXe2FJPG7gC7CCWztVh9fUJfQzOgYs4klrVsMY7DZMmiaB4NfeAO05dq8b6icfHLQ9GyLjv3LvsB6vJsVKFBtwG1lpvl2UgLLHwGvzYjKtCqakwGFL5/Jj1kRDqW+Huwpf6kYzIw3pqQgLoUpVQeJFTx89Jcg4uUbmJB89U5Jsl3KOXDttMHkZjgisytEv46OO8Gpsu12QvZIfITE8Myjs+huJr8V4yWGKry1/ZbADUYkeNdXrGA4x3x7w/QR3y8ttWoV1beFsQOykhRvOv1w1MHcBvUtPH5yUVx7eIaWsPBmKTncTtsGD2tiwOk8+O/HTGnr9KYJFY2a/SBFl4TMCEpfwQWuDTtlzxxnWsjLZaxV0Gjzo7pitWKPBb4VEC7OYuLlHu5ZY/3bt4ShW/UgN5WkegjsRXhkkqhIshjSrRbMqrkjx8PDKonUcmbmrlsml8joYzwXjVknd9Q+XzpNf1IcSGnKWH/ICSOU8JxZp5jdBTlCJCIRy/cW46BLoKAk4dVEjf6a6CThNAb0DIPF57zK3KhOzBzIXaN4zC7yyonQC5jXiBY3dnlgIXMYnLioKFp9dvsDc4yKvLi1Hr1E/MecIwB/UHPKS+V0Sgo8MhuhTfOmxmdV+T9MkazAjgCohs9EQweTIqFQcxfEIqTNPXqyXmfkHemaOcpFfbo0Mx3pJKpM6DeRTWwSrC45+K8wJA6MPqhdMSdwqc5wGMh9FmOW+Is3Znu26kiAeQsoCxQQFzjfCDKylE8x40A1dSs0C1uc2jrGe3BeFl3nAbi0eGEYGgIlc475eQaY8uDexgZCM+sMF0jmNXb5ldi5/seg29kVrqH3nZ3PU2EjMU5m189lbm8vR+dL9gA==", "HEXw1O7fmmnAalg6c8/NfcDp1fpGgO6R5qy/3segC3WL4LxdrvgxcRzAlZMsXktv", "SGI.WIN.001", "1.0.3", "HU20", "MIIDkTCCAnmgAwIBAgIEB2bqgjANBgkqhkiG9w0BAQUFADCBvzEmMCQGCSqGSIb3DQEJARYXa3VuZGFqQHNlY3VnZW5pbmRpYS5jb20xDzANBgNVBAcTBk11bWJhaTEsMCoGA1UEAxMjRFMgU0VDVUdFTiBJTkRJQSBQUklWQVRFIExJTUlURUQgMDQxJjAkBgNVBAoTHVNFQ1VHRU4gSU5ESUEgUFJJVkFURSBMSU1JVEVEMQswCQYDVQQLEwJJVDEUMBIGA1UECBMLTWFoYXJhc2h0cmExCzAJBgNVBAYTAklOMB4XDTIyMTAxNDA1MzMwNFoXDTIyMTExMzA1MzMwNFowVTESMBAGA1UEAxMJU0dJUkRVU0VSMQwwCgYDVQQKEwNBVUExDjAMBgNVBAcTBWxvY2FsMRQwEgYDVQQIEwtNYWhhcmFzaHRyYTELMAkGA1UEBhMCSU4wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCwW7ZgTrSql1/TOyeqOsB8XGYS7uLDqyZmbka4Y4l9civsLKf8JXo/RA5I9AwonBCnAnb3cUT/98Qz/9bY/90BKEKlz+qvIQuYxkQEpsg392FRTnn0CPXJRpirVzvEG7gtxzACd20ikbn+gflDZkNyP2ETfwWg/TsV+flNzky/oAcZOBd46r1SK+kVyC0Kp0X1aTBFAoGaq+ftsR0vMq/bPJ8ElEVpeouF8MA6BkGOSbJxlPL+HyDz7v2MUUX8dzrh8EuSf4bKhzYDcKsOi4EBkB52aIUsMKdAkBejf5aKYA46dqXYs2DneJwV3P5LXTpnBkpGC/9pLMy4xHeu6Qs/AgMBAAEwDQYJKoZIhvcNAQEFBQADggEBABERZDKqxSXIzxKArlx7GvwgXWU2WZHBOVLb3lceZdQb4DHjRtaepqu5u8KdIXOz+btiLtjaZ6iqPadu/slCsubWoqfDDHj0A1JZ6tmtRJXaePaXv7wLw5iuPUFzJGWJLmX9gsNrEkjA2gJj5Q0chvKlVBZlxeKQVn0k2zXmguomRetJmwkNfOI9N0oC67msKsOf/iDIow9GoI7vCYr38ng82sFOP4hMQunHNBr4AO5jhGDwaeui48y8os13yEB9QeaqtIbbpXV0X+uSC0x0Dqiuy8iX2no3OewCF3J095TILIoWtczHjNCW8FZfDfO0w2CxrvcR0RBtBNBxDU3sb1w=", "SECUGEN.SGI", "8af61197-b31c-11e7-90aa-1418775b2036", "20221023", "H54170200483", "2022-10-17T02:12:06", "UKC:20221017T0212061456806", "101", "FINGERPRINT", "123", 0, "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");





                if (objDictionary.ContainsKey("Success"))
                {
                    status = "Success";
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(objDictionary["XmlResponse"]);
                    //XmlElement EkycResponse = xmlDoc.DocumentElement;

                    //TxnID = EkycResponse.Attributes["txn"].Value;




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
                       // TxnID = objDictionary["TransactionId"],
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
                    //status = "Fail";
                    //ErrorMsg = objDictionary["Error"];
                    if (objDictionary.ContainsKey("ErrorCode"))
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                        ErrorCode = objDictionary["ErrorCode"];
                    }
                    else
                    {
                        status = "Fail";
                        ErrorMsg = objDictionary["Error"];
                    }


                    var EkycViaBioResponse = new
                    {

                        status = status,

                        //  ErrorCode = objDictionary["ErrorCode"],  //Return ErrorCode from Ekyc service
                        ErrorCode = ErrorCode,
                        ErrorMsg = ErrorMsg,


                    };
                    string EkycViaBioResp = JsonConvert.SerializeObject(EkycViaBioResponse);


                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(EkycViaBioResp, Encoding.UTF8, "application/json")
                    };
                }

                }
                catch (Exception ex)
                {
                    string strException = ex.Message + " StackTrace = " + ex.StackTrace + " Method = " + ex.TargetSite + "InnerException = " + ex.InnerException;
                    //dbentity.USP_EKYCResponseLog(strAadhaarNo1, txnId1, customerId1, status, ErrorCode, ErrorMsg, " ", ex.ToString(), DateTime.Now.ToString());
                    return new HttpResponseMessage()
                    {

                        
                    Content = new StringContent(strException, Encoding.UTF8, "application/json")
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
            string Result = string.Empty;
            Dictionary<string, string> objDictionary;

            json_data = JsonConvert.SerializeObject(AuthViaBioRequest);

            var ReqParam = JsonConvert.DeserializeObject<EkycViaBiometric>(json_data);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            objDictionary = objser.AuthenticationViaBiometric(ReqParam.uidType, ReqParam.strAadhaarNo, ReqParam.strEncryptedSKey, ReqParam.encryptedPID, ReqParam.sha256ofPidXML, ReqParam.rdsId, ReqParam.rdsVer, ReqParam.mi, ReqParam.mc, ReqParam.dpid, ReqParam.dc, ReqParam.ci, ReqParam.strTerminalId, ReqParam.ts, ReqParam.strTransactionId, "101", ReqParam.BiometricType, ReqParam.customerId, "1", "Alpha", "wSGDktYOjB1/d9ghvBaKrQ==", "");




            if (objDictionary.ContainsKey("Success"))
            {
                status = "Success";
                Result = objDictionary["Success"];
            }
            else
            {
                if (objDictionary.ContainsKey("ErrorCode"))
                {
                    status = "Fail";
                    ErrorMsg = objDictionary["Error"];
                    ErrorCode = objDictionary["ErrorCode"];
                }
                else
                {
                    status = "Fail";
                    ErrorMsg = objDictionary["Error"];
                }
            }


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(objDictionary["XmlResponse"]);
            XmlElement EkycResponse = xmlDoc.DocumentElement;

             TxnID = EkycResponse.Attributes["txn"].Value;
            var GetAuthResponse = new
            {

                status = status,
                statusmsg = Result,

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
