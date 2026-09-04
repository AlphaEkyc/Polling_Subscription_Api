using EkycAPI.Helper;
using EkycAPI.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;



namespace EkycAPI.Controllers
{
    [RoutePrefix("api/Deceased")]
    public class DeceasedController : ApiController
    {



        // GET: Deceased
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register()
        {
            try
            {
                return Ok("Deceased Register API working");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("RegisterAPI")]
        public async Task<IHttpActionResult> RegisterAPI(
         [FromBody] RequestModel inputRequestEncrypted)
        {
            try
            {

                ApiTraceLogger logger = new ApiTraceLogger("Deceased_RegisterAPI");
                logger.Step("RegisterAPI Started");
                string token = null;
                string refKey = null;
                string sha256ofData = string.Empty; string bankthumbprint = string.Empty;
                string jsonBody = string.Empty; string uidaithumbprint = string.Empty;
                string Response = string.Empty;
                string exceptionDetails = string.Empty;
                string errorMessage = "Error occurred while calling UIDAI API.";
                string RespSid = string.Empty;
                string RespStatus = string.Empty;
                string RespRejectCode = string.Empty;





                if (Request.Headers.Contains("Token"))
                {
                    token = Request.Headers.GetValues("Token").FirstOrDefault();
                }

                if (Request.Headers.Contains("RefKey"))
                {
                    refKey = Request.Headers.GetValues("RefKey").FirstOrDefault();
                }

                // Temporary logging/check
                System.Diagnostics.Debug.WriteLine("Token received: " + !string.IsNullOrEmpty(token));
                System.Diagnostics.Debug.WriteLine("RefKey received: " + !string.IsNullOrEmpty(refKey));

                string InputRequest = inputRequestEncrypted.inputRequestEncrypted;

                if (string.IsNullOrEmpty(InputRequest))
                {
                    return BadRequest("inputRequestEncrypted is required.");
                }


                try
                {
                    InputRequest = Encoding.UTF8.GetString(
                        Convert.FromBase64String(InputRequest)
                    );
                }
                catch (FormatException)
                {
                    return BadRequest("inputRequestEncrypted is not a valid Base64 string.");
                }

                logger.Step("Input Request");

                logger.Data(
                    "InputRequestEncrypted",
                    InputRequest ?? "NULL");

                string msgId = Guid.NewGuid().ToString();

                string ReqMsgTs = DateTime.Now.ToString(
                    "yyyy-MM-ddTHH:mm:ss.fffzzz"
                );

                string DateTxn = DateTime.Now.ToString("yyyy-MM-dd");



                string licensekey = ConfigurationManager.AppSettings["Uidai.LicenseKey"];

                string auacode = ConfigurationManager.AppSettings["Uidai.SubAuaCode"];

                string UIDAICertificate = ConfigurationManager.AppSettings["UIDAICertificate"];

                string UIDAIURL = ConfigurationManager.AppSettings["Uidai.Deceased.RegisterUrl"];

                var connstr = ConfigurationManager.ConnectionStrings["CapriEkycDb"].ConnectionString;


                string BankEncryptionCertificate = ConfigurationManager.AppSettings["BankEncryptionCertificate"];

                string EncrPWD = ConfigurationManager.AppSettings["EncrPWD"];


                var customersdata = System.Text.Json.JsonSerializer.Deserialize<List<Requestdata>>(InputRequest);

                if (customersdata == null || customersdata.Count == 0)
                {
                    return BadRequest("Invalid deceased request data.");
                }

                string Name = string.Empty;
                string DOB = string.Empty;
                string DOD = string.Empty;
                string UDRN = string.Empty;
                string Gender = string.Empty;
                string UID = string.Empty;
                
                foreach (var customer in customersdata)
                {
                    //Name = customer.name;
                    //DOB = customer.dob;
                    //DOD = customer.dod;
                    //UDRN = customer.udrn;
                    //Gender = customer.gender;
                    //AadhaarNo = customer.uid;

                    UDRN = customer.udrn;
                    UID = customer.uid;
                    Name = customer.name;
                    Gender = customer.gender;
                    DOB = customer.dob;
                    DOD = customer.dob;


                }

                string maskeduid = string.Empty;

                if (!string.IsNullOrEmpty(UID) && UID.Length >= 12)
                {
                    maskeduid = new string('*', 8) + UID.Substring(8);
                }

                Encryption objEncryption = new Encryption();


                X509Certificate2 cert = new X509Certificate2(BankEncryptionCertificate, EncrPWD, X509KeyStorageFlags.MachineKeySet);

                bankthumbprint = cert.Thumbprint;
                logger.Step("Bank Certificate");

                logger.Data("Bank Certificate Thumbprint", bankthumbprint);
                logger.Data("Bank Certificate Subject", cert.Subject);
                logger.Data("Bank Certificate Valid From", cert.NotBefore.ToString("O"));
                logger.Data("Bank Certificate Valid To", cert.NotAfter.ToString("O"));
                logger.Data("Bank Certificate Has Private Key", cert.HasPrivateKey.ToString());

                // UIDAI Staging Certificate
                string pem = File.ReadAllText(UIDAICertificate);

                pem = pem
                    .Replace("-----BEGIN CERTIFICATE-----", "")
                    .Replace("-----END CERTIFICATE-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

                byte[] certificateBytes = Convert.FromBase64String(pem);

                X509Certificate2 certuidai = new X509Certificate2(certificateBytes);

                uidaithumbprint = certuidai.Thumbprint;

                logger.Step("UIDAI Certificate");

                logger.Data("UIDAI Certificate Thumbprint", uidaithumbprint);
                logger.Data("UIDAI Certificate Subject", certuidai.Subject);
                logger.Data("UIDAI Certificate Valid From", certuidai.NotBefore.ToString("O"));
                logger.Data("UIDAI Certificate Valid To", certuidai.NotAfter.ToString("O"));



                byte[] aeskey = Encryption.GenerateSKey();

                byte[] iv = new byte[16];
                logger.Data("IV Length", iv.Length.ToString());

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(iv);
                }

                string IV = Convert.ToBase64String(iv);
                logger.Step("Encryption Parameters");

                logger.Data("Message ID", msgId);
                logger.Data("Request Message Timestamp", ReqMsgTs);
                logger.Data("IV", IV);
                logger.Data("AES Key Length", aeskey.Length.ToString());

                string EncryptedSKey = Encryption.EncryptSessionKey_WithOAEP_SHA256(aeskey, UIDAICertificate, iv);
                logger.Data("Encrypted Session Key", EncryptedSKey);
                logger.Data(
                 "Encrypted Session Key Length",
                 Convert.FromBase64String(EncryptedSKey).Length.ToString()
                );
                Dictionary<string, string> objDictionary = objEncryption.EncryptDataUsingAesGcm(aeskey, InputRequest, iv);

                string encryptedData = objDictionary["EncPID"];
                logger.Data("Encrypted Data", encryptedData);

                using (SHA256 hash = SHA256Managed.Create())
                {
                    byte[] plaintext =
                        hash.ComputeHash(
                            Encoding.UTF8.GetBytes(InputRequest));

                    sha256ofData =
                        objEncryption.Sha256_hash(
                            plaintext,
                            ReqMsgTs,
                            aeskey);

                    logger.Data("Request HMAC", sha256ofData);
                }

                var header = new
                {
                    id = "uidai.registration.deceasedregister",
                    ver = "1.0",
                    msgId = msgId,
                    msgTs = ReqMsgTs,//DateTime.UtcNow.ToString("o"), // ISO-8601 format
                    senderId = "capriglobal.in",
                    lk = licensekey,
                    ac = auacode,
                    sa = auacode,
                    action = "notify",
                    isMessageEncrypted = true
                };
                logger.Step("UIDAI Request Header");

                logger.Data("ID", "uidai.registration.deceasedregister");
                logger.Data("Version", "1.0");
                logger.Data("Message ID", msgId);
                logger.Data("Message Timestamp", ReqMsgTs);
                logger.Data("Sender ID", "capriglobal.in");
                logger.Data("License Key", licensekey);
                logger.Data("AUA Code", auacode);
                logger.Data("SA", auacode);
                logger.Data("Action", "notify");
                logger.Data("Is Message Encrypted", "true");

                var msg = new
                {
                    txnId = msgId,
                    header = new
                    {
                        alg = "AES-256-GCM",
                        enc = "RSA-OAEP",
                        requestSessionKey = EncryptedSKey,
                        thumbprint = uidaithumbprint,
                        clientCert = bankthumbprint,
                        iv = IV
                    },
                    data = encryptedData,
                    requestHMAC = sha256ofData
                };
                logger.Step("UIDAI Message Header");

                logger.Data("Algorithm", "AES-256-GCM");
                logger.Data("Encryption", "RSA-OAEP");
                logger.Data("Request Session Key", EncryptedSKey);
                logger.Data("UIDAI Thumbprint", uidaithumbprint);
                logger.Data("Client Certificate Thumbprint", bankthumbprint);
                logger.Data("IV", IV);



                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };

                string headerJson = System.Text.Json.JsonSerializer.Serialize(header, options);
                string messageJson = System.Text.Json.JsonSerializer.Serialize(msg, options);

                logger.Step("Serialized UIDAI Request");

                logger.Data("Header JSON", headerJson);
                logger.Data("Message JSON", messageJson);

                string signature = Encryption.GenerateSignature(BankEncryptionCertificate, EncrPWD, headerJson, messageJson);

                logger.Data("Signature", signature);


                var jsonObject = new
                {

                    header,
                    msg,
                    signature = signature

                };

                jsonBody = System.Text.Json.JsonSerializer.Serialize(jsonObject, options);
                try
                {
                    logger.Data("Final JSON Body", jsonBody);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                     await InsertDeceasedTransaction(connstr, msgId, InputRequest, jsonBody, DateTime.Now);

                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    handler.SslProtocols =
                        System.Security.Authentication.SslProtocols.Tls12;

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromMinutes(2);

                        using (var request = new HttpRequestMessage(
                            HttpMethod.Post,
                            UIDAIURL))
                        {
                            request.Content = new StringContent(
                                jsonBody,
                                Encoding.UTF8,
                                "application/json");

                            request.Headers.Authorization =
                                new AuthenticationHeaderValue(
                                    "Bearer",
                                    token);

                            request.Headers.Add(
                                "Signature",
                                signature);

                            request.Headers.Add(
                                "X-request-id",
                                msgId);

                            request.Headers.Add(
                                "x-client-id",
                                msgId);

                            request.Headers.Add(
                                "x-encrypted",
                                "true");

                            logger.Step("Sending Request To UIDAI");

                            logger.Data("UIDAI URL", UIDAIURL);
                            logger.Data("HTTP Method", "POST");
                            logger.Data("Authorization Present",
                                string.IsNullOrEmpty(token) ? "NO" : "YES");
                            logger.Data("Signature Present",
                                string.IsNullOrEmpty(signature) ? "NO" : "YES");
                            logger.Data("X-Request-ID", msgId);
                            logger.Data("X-Client-ID", msgId);
                            logger.Data("X-Encrypted", "true");

                            using (var response = await client.SendAsync(
                                request,
                                HttpCompletionOption.ResponseHeadersRead))
                            {
                                // response.EnsureSuccessStatusCode();

                                Response = await response.Content.ReadAsStringAsync();


                                logger.Step("UIDAI Response");
                                logger.Data("Response", Response);
                            }

                            // ==========================================
                            // RESPONSE HANDLING
                            // ==========================================


                            if (string.IsNullOrWhiteSpace(Response))
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Empty Response Received From UIDAI",
                                    data = Response
                                });
                            }


                            DeceasedResponse Responsebody = null;

                            try
                            {
                                Responsebody =
                                    System.Text.Json.JsonSerializer.Deserialize<DeceasedResponse>(
                                        Response);
                            }
                            catch (Exception ex)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Unable to parse UIDAI response.",
                                    data = ex.Message
                                });
                            }
                            if (Responsebody == null)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Invalid response received from UIDAI.",
                                    data = Response
                                });
                            }


                            // ==========================================
                            // CHECK UIDAI RESPONSE
                            // ==========================================

                            if (Responsebody.response == null)
                            {
                                string errorCode = string.Empty;
                                string errorMsg = string.Empty;

                                if (Responsebody.error != null)
                                {
                                    errorCode = Responsebody.error.code;
                                    errorMsg = Responsebody.error.message;

                                    await UpdateDeceasedTransaction( connstr,msgId,Response,DateTime.Now,null,"FAILED",errorCode,errorMsg);



                                }

                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = errorMsg,
                                    errorCode = errorCode,
                                    data = Responsebody
                                });
                            }


                            // ==========================================
                            // SUCCESSFUL ENCRYPTED RESPONSE
                            // ==========================================

                            string encryptedSessionKey =
                                Responsebody.sessionKey;

                            string encrypteddatafromresponse =
                                Responsebody.response;

                            if (string.IsNullOrWhiteSpace(encryptedSessionKey))
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "SessionKey is missing in UIDAI response.",
                                    data = Responsebody
                                });
                            }

                            if (string.IsNullOrWhiteSpace(encrypteddatafromresponse))
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Encrypted response data is missing from UIDAI response.",
                                    data = Responsebody
                                });
                            }


                            // ==========================================
                            // BASE64 DECODE SESSION KEY
                            // ==========================================

                            byte[] encryptedResponseSessionKey;

                            try
                            {
                                encryptedResponseSessionKey =
                                    Convert.FromBase64String(encryptedSessionKey);
                            }
                            catch (FormatException)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Invalid Base64 SessionKey received from UIDAI.",
                                    data = Responsebody
                                });
                            }


                            // ==========================================
                            // BASE64 DECODE RESPONSE DATA
                            // ==========================================

                            byte[] encryptedResponseData;

                            try
                            {
                                encryptedResponseData =
                                    Convert.FromBase64String(encrypteddatafromresponse);
                            }
                            catch (FormatException)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Invalid Base64 response data received from UIDAI.",
                                    data = Responsebody
                                });
                            }


                            // ==========================================
                            // GET RESPONSE IV
                            // ==========================================

                            if (encryptedResponseData.Length < 16)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Encrypted response data is too short to extract IV.",
                                    data = Responsebody
                                });
                            }

                            byte[] responseIV = new byte[16];

                            Buffer.BlockCopy(
                                encryptedResponseData,
                                0,
                                responseIV,
                                0,
                                16);


                            // ==========================================
                            // DECRYPT SESSION KEY
                            // ==========================================

                            byte[] responsePlainSessionKey =
                                     Encryption.DecryptSessionKey_WithOAEP_SHA256(
                                                        BankEncryptionCertificate,
                                                                          EncrPWD,
                                                      encryptedResponseSessionKey,
                                                                      responseIV);

                            if (responsePlainSessionKey == null)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Unable to decrypt UIDAI response session key.",
                                    data = ""
                                });
                            }


                            // ==========================================
                            // DECRYPT RESPONSE DATA
                            // ==========================================

                            string plainResponse;

                            try
                            {
                                plainResponse =
                                    objEncryption.DecryptDataUsingAesGcm(
                                        responsePlainSessionKey,
                                        encryptedResponseData,
                                        responseIV);
                            }
                            catch (Exception ex)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Unable to decrypt UIDAI response.",
                                    data = ex.Message
                                });
                            }



                            if (string.IsNullOrWhiteSpace(plainResponse))
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Decrypted UIDAI response is empty.",
                                    data = ""
                                });
                            }


                            // ==========================================
                            // DESERIALIZE DECRYPTED RESPONSE
                            // ==========================================

                            List<PlainResp> plainResponseList;

                            try
                            {
                                plainResponseList =
                                    System.Text.Json.JsonSerializer
                                    .Deserialize<List<PlainResp>>(plainResponse);
                            }
                            catch (Exception ex)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Unable to parse decrypted UIDAI response.",
                                    data = new
                                    {
                                        response = plainResponse,
                                        error = ex.Message
                                    }
                                });
                            }

                           


                            if (plainResponseList == null)
                            {
                                return Ok(new
                                {
                                    status = "FAILED",
                                    message = "Invalid decrypted response received from UIDAI.",
                                    data = plainResponse
                                });
                            }


                            // ==========================================
                            // PROCESS FINAL RESPONSE
                            // ==========================================

                            foreach (var item in plainResponseList)
                            {
                                RespSid = item.sid;
                                RespStatus = item.status;
                                RespRejectCode = item.rejectCode;
                                UDRN = item.udrn;
                            }
                            string decryptedResponseJson = System.Text.Json.JsonSerializer.Serialize(plainResponseList);

                            await UpdateDeceasedTransaction(connstr, msgId, Response, DateTime.Now, decryptedResponseJson, "Success", null, null);

                            if (RespRejectCode == null)
                            {
                                RespRejectCode = string.Empty;
                            }


                            // ==========================================
                            // FINAL SUCCESS RESPONSE
                            // ==========================================

                            return Ok(new
                            {
                                status = "SUCCESS",
                                message = "",
                                data = plainResponseList
                            });


                        }
                    }


                    return Ok(new
                    {
                        status = "SUCCESS",
                        message = "Request processed successfully.",
                        response = Response
                    });
                }
                catch (Exception ex)
                {
                    exceptionDetails =
                        ex.InnerException + " | " +
                        ex.Message + " | " +
                        ex.TargetSite + " | " +
                        ex.Source + " | " +
                        ex.HResult;

                    return Ok(new
                    {
                        status = "FAILED",
                        message = errorMessage,
                        data = ex
                    });
                }




            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [Route("GetAccessToken")]
        public async Task<IHttpActionResult> GetAccessToken()
        {
            try
            {
                string clientId =
                    ConfigurationManager.AppSettings["Uidai.Deceased.ClientId"];

                string clientSecret =
                    ConfigurationManager.AppSettings["Uidai.Deceased.ClientSecret"];

                string tokenUrl =
                    ConfigurationManager.AppSettings["Uidai.Deceased.TokenUrl"];

                string url = tokenUrl
                            + "?client_id=" + clientId
                            + "&client_secret=" + clientSecret;

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        url
                    );

                    request.Content = new StringContent(string.Empty);

                    var response = await client.SendAsync(request);

                    string responseBody =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Content(
                            response.StatusCode,
                            responseBody
                        );
                    }

                    return Ok(responseBody);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        private async Task<int> InsertDeceasedTransaction(
            string connstr,
            string msgId,
            string inputRequest,
            string finalJson,
            DateTime requestTime)
        {
            using (SqlConnection con = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "InsertDeceasedRegistrationTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MSGID", msgId);
                    cmd.Parameters.AddWithValue("@InputRequest",
                        (object)inputRequest ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FinalJson",
                        (object)finalJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestTime", requestTime);

                    await con.OpenAsync();

                    object result = await cmd.ExecuteScalarAsync();

                    return Convert.ToInt32(result);
                }
            }
        }


        private async Task UpdateDeceasedTransaction(
    string connstr,
    string msgId,
    string response,
    DateTime responseTime,
    string decryptedResponse,
    string status,
    string rejectCode,
    string exception)
        {
            using (SqlConnection con = new SqlConnection(connstr))
            using (SqlCommand cmd = new SqlCommand(
                "UpdateDeceasedRegistrationTransaction", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MSGID", SqlDbType.VarChar, 100)
                    .Value = msgId;

                cmd.Parameters.Add("@Response", SqlDbType.NVarChar, -1)
                    .Value = (object)response ?? DBNull.Value;

                cmd.Parameters.Add("@ResponseTime", SqlDbType.DateTime2)
                    .Value = responseTime;

                cmd.Parameters.Add("@DecryptedResponse", SqlDbType.NVarChar, -1)
                    .Value = (object)decryptedResponse ?? DBNull.Value;

                cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50)
                    .Value = (object)status ?? DBNull.Value;

                cmd.Parameters.Add("@RejectCode", SqlDbType.VarChar, 50)
                    .Value = (object)rejectCode ?? DBNull.Value;

                cmd.Parameters.Add("@Exception", SqlDbType.NVarChar, -1)
                    .Value = (object)exception ?? DBNull.Value;

                await con.OpenAsync();

                await cmd.ExecuteNonQueryAsync();
            }
        }





    }



}

