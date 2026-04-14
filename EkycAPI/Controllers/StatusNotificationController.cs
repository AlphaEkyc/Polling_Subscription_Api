using AadhaarStatusService.Api.Models.Dto;
using EkycAPI.Helper;
using EkycAPI.Models;
using EkycAPI.Services.Interface;
using Jose;
using Jose.keys;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.IsisMtt.Ocsp;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Util;
//using System.Web.Mvc;

namespace EkycAPI.Controllers
{
    [RoutePrefix("api/status")]
    public class StatusNotificationController : ApiController
    {


        private static readonly SemaphoreSlim SubscribeLock = new SemaphoreSlim(1, 1);

        [HttpPost]
        [Route("Subscribe")]

        public async Task<IHttpActionResult> Subscribe([FromBody] MsgBlock request)
        {
            var correlationId = Guid.NewGuid().ToString();
            await SubscribeLock.WaitAsync();
            try
            {
                


                var asabase = ConfigurationManager.AppSettings["Uidai.AsaBaseUrl"];
                var endpoint = ConfigurationManager.AppSettings["Uidai.SubscriptionEndpoint"];
                var aua = ConfigurationManager.AppSettings["Uidai.AuaCode"];
                var subAua = ConfigurationManager.AppSettings["Uidai.SubAuaCode"];
                var license = ConfigurationManager.AppSettings["Uidai.LicenseKey"];
                var signingCertPath = ConfigurationManager.AppSettings["Uidai.SigningCertPath"];
                var signingCertPassword = ConfigurationManager.AppSettings["Uidai.SigningCertPassword"];
                // var secret = ConfigurationManager.AppSettings["Uidai.HmacSecret"];

                var connstr = ConfigurationManager.ConnectionStrings["CapriEkycDb"].ConnectionString;
                //if (string.IsNullOrEmpty(secret))
                //    return InternalServerError(new Exception("HMAC secret missing in Web.config"));
                var msgId = Guid.NewGuid().ToString();
                var msgTs = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                var debugDir = @"C:\UIDAI_Subscription_Debug";
                Directory.CreateDirectory(debugDir);

                // ---------- ONE LOG FILE ----------
                string logFile = Path.Combine(debugDir, $"Subscription_Debug_{msgId}.txt");
                void Log(string s) => File.AppendAllText(logFile, s + Environment.NewLine);


                if (request == null)
                {
                    Log("ERROR: Request is NULL");
                    return BadRequest("Invalid request");
                }

                Log("===== REQUEST PARAMETERS =====");

                Log($"txnId           : {request.TxnId}");
                Log($"userId          : {request.UserId}");
                Log($"notifyEndpoint  : {request.NotifyEndpoint}");
                Log($"startDate       : {request.StartDate}");
                Log($"schedule        : {request.Schedule}");

                Log("==============================");


                Log("===== METADATA =====");
                Log($"AUA Code       : {aua}");
                Log($"SubAUA Code    : {subAua}");
                Log($"MsgId          : {msgId}");
                Log($"MsgTs          : {msgTs}");
                Log($"Endpoint       : {endpoint}");
                //Log($"LicenseKey     : {license} ");
                Log("");

                // ---------------- FIXED HEADER JSON (ORDER MATTERS) ----------------
                HeaderBlock header = new HeaderBlock
                {
                    Ver = "1.0.0",
                    MsgId = msgId,
                    MsgTs = msgTs,
                    Ac = aua,
                    Sa = subAua,
                    Action = "subscribe",
                    IsMessageEncrypted = false,
                    Lk = license
                };

                // ---------------- FIXED MSG JSON (ORDER MATTERS) ----------------
                MsgBlock msg = new MsgBlock
                {
                    NotifyEndpoint = request.NotifyEndpoint,
                    StartDate = request.StartDate,
                    Schedule = request.Schedule
                };

                string UserID = request.UserId;
                string txxnID = request.TxnId;


                
                string headerJsonString = JsonSerializer.Serialize(header);
                string msgJsonString = JsonSerializer.Serialize(msg);

                string payloadToSign = headerJsonString + msgJsonString;


                // ---------------- data.signature = HMAC(header + msg) ----------------
                string dataSignature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(license)))
                {
                    dataSignature = Convert.ToBase64String(
                        hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadToSign))
                    );
                }


                var dataObject = new
                {

                    signature = dataSignature,
                    header = header,
                    msg = msg,

                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = false
                };

                var requestToSign = new
                {
                    data = dataObject
                };

                // SERIALIZE ONCE
                string jsonWithPlaceholder = JsonSerializer.Serialize(requestToSign, options);

                // PREPARE JSON FOR SIGNING (empty outer signature)



                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonWithPlaceholder);

                // ---------------- ROOT SIGNATURE USING DSC -------------------
                string rootSignature = SignWithDSC(
                    dataBytes,
                    signingCertPath,
                    signingCertPassword
                );

                var finalRequestObject = new
                {
                    data = dataObject,
                    signature = rootSignature
                };


                string finalJson = JsonSerializer.Serialize(finalRequestObject, options);

                //string finalJson = JsonSerializer.Serialize(finalRequestObject, options);

                var logId = await InsertRequestLog(
                    connstr,
                    msgId,
                    request.NotifyEndpoint,
                    request.StartDate,
                    request.Schedule,
                   //Convert.ToBase64String(finalBytes),
                   finalJson,
                    "{}",
                    "Pending",
                    "Test",
                     UserID,
                     txxnID);


                ServicePointManager.ServerCertificateValidationCallback =
                     (sender, certificate, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient())
                {

                    client.Timeout = TimeSpan.FromSeconds(30);


                    var httpResponse = await client.PostAsync(endpoint, new StringContent(finalJson, Encoding.UTF8, "application/json"));
                    var responseJson = await httpResponse.Content.ReadAsStringAsync();
                    var statusText = httpResponse.IsSuccessStatusCode ? "SUCCESS" : "FAILED";


                    //var content = new ByteArrayContent(finalRequestBytes); content.Headers.ContentType =
                    //    new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    //client.DefaultRequestHeaders.ExpectContinue = false;
                    //ServicePointManager.UseNagleAlgorithm = false;
                    //content.Headers.ContentLength = finalRequestBytes.Length;
                    //var Httpresponse = await client.PostAsync(endpoint, content);
                    //var responseJson = await Httpresponse.Content.ReadAsStringAsync();

                    await UpdateResponseLog(
                        connstr,
                        logId,
                        statusText,
                       httpResponse.StatusCode.ToString(),
                      "Protean response received",
                       responseJson,
                       httpResponse.IsSuccessStatusCode ? null : "ASA_ERROR",
                       null,
                       null
                    );

                    bool isSubscribed = false;
                    string asaStatus = "FAILED";
                    string asaMessage = null;
                    string asaTxnId = null;

                    using (var doc = JsonDocument.Parse(responseJson))
                    {
                        var root = doc.RootElement;

                        // response exists?
                        if (root.TryGetProperty("response", out var responseElement))
                        {
                            // Case 1: response = "S12", "S27", "A10"
                            if (responseElement.ValueKind == JsonValueKind.String)
                            {
                                asaMessage = responseElement.GetString();
                            }
                            // Case 2: response is an object
                            else if (responseElement.ValueKind == JsonValueKind.Object)
                            {
                                if (responseElement.TryGetProperty("msg", out var msgElement) &&
                                    msgElement.ValueKind == JsonValueKind.Object)
                                {
                                    if (msgElement.TryGetProperty("status", out var statusElement) &&
                                        statusElement.ValueKind == JsonValueKind.String)
                                    {
                                        asaStatus = statusElement.GetString();
                                    }

                                    if (msgElement.TryGetProperty("message", out var messageElement) &&
                                        messageElement.ValueKind == JsonValueKind.String)
                                    {
                                        asaMessage = messageElement.GetString();
                                    }

                                    if (string.Equals(asaStatus, "Success", StringComparison.OrdinalIgnoreCase) &&
                                        root.TryGetProperty("txnId", out var txnElement) &&
                                        txnElement.ValueKind == JsonValueKind.String)
                                    {
                                        isSubscribed = true;
                                        asaTxnId = txnElement.GetString();
                                    }
                                }
                                else
                                {
                                    asaMessage = "Invalid ASA response: msg block missing";
                                }
                            }
                            else
                            {
                                asaMessage = "Invalid ASA response: unexpected response type";
                            }
                        }
                        else
                        {
                            asaMessage = "Invalid ASA response: response field missing";
                        }
                    }
                    Log("===== ASA RESPONSE =====");
                    Log(responseJson);
                    Log("");
                    return Ok(new
                    {
                        success = isSubscribed,
                        asaStatus = asaStatus,
                        asaMessage = asaMessage,
                        subscriptionTxnId = asaTxnId,
                        rawResponse = responseJson,
                        correlationId = correlationId
                    });

                }

            }

            catch (Exception ex)
            {
                return InternalServerError(new Exception(
              $"CorrelationId={correlationId} | {ex.Message}", ex));
            }
            finally
            {
                SubscribeLock.Release();   // ALWAYS RELEASE
            }
        }

        [HttpPost]
        [Route("uidai/notify/V8")]
        public IHttpActionResult Notify(object payload)
        {
            // You can log or ignore
            return Ok();
        }

        

        [HttpPost]
        [Route("Polling")]
        public async Task<IHttpActionResult> Polling([FromBody] PollingInput request)
        {
            try
            {
                var log = new ApiTraceLogger("PollingAPI");
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.TxnId) ||
                    string.IsNullOrWhiteSpace(request.MsgTs) ||
                    string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest("TxnId, MsgTs, UserId are required");
                }


                //var correlationId = Guid.NewGuid().ToString();
                var Constr = ConfigurationManager.ConnectionStrings["CapriEkycDb"].ConnectionString;
                var PollingEndpoint = ConfigurationManager.AppSettings["Uidai.PollingEndpoint"];
                var aua = ConfigurationManager.AppSettings["Uidai.AuaCode"];
                var subAua = ConfigurationManager.AppSettings["Uidai.SubAuaCode"];
                var license = ConfigurationManager.AppSettings["Uidai.LicenseKey"];
                var secret = ConfigurationManager.AppSettings["Uidai.LicenseKey"];
                var uidaiEncryptionCertificate = ConfigurationManager.AppSettings["Uidai.EncryptionCertificate"];
                var signingCertPath = ConfigurationManager.AppSettings["Uidai.SigningCertPath"];
                var signingCertPassword = ConfigurationManager.AppSettings["Uidai.SigningCertPassword"];


                if (string.IsNullOrWhiteSpace(secret))
                    throw new Exception("HMAC secret missing");

                if (!File.Exists(uidaiEncryptionCertificate))
                    throw new Exception("Protean public key not found");

                // 1. Plain business message
                //var log = new ApiTraceLogger("PollingAPI");
                log.Data("TxnId", request.TxnId);
                log.Data("UserId", request.UserId);
                log.Data("MsgTs", request.MsgTs);

                log.Step("STEP 1 Generate Transaction ID");
               // string txnId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                string txnId = request.TxnId;
                log.Data("Generated txnId", txnId);


                var plainMsgObject = new
                {
                    txnId = txnId
                };
                string plainMsg = JsonSerializer.Serialize(plainMsgObject);

                byte[] msgBytes = Encoding.UTF8.GetBytes(plainMsg);


                log.Step("STEP 2 AES Key Generation");
                // 2. Generate AES key and IV
                byte[] aesKey = new byte[32]; // 256-bit
                byte[] iv = new byte[16];     // 128-bit GCM nonce


                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(aesKey);
                    rng.GetBytes(iv);
                }   // 96-bit GCM nonce
                log.Data("AES Key Length", aesKey.Length.ToString());
                log.Data("Aes Key", BitConverter.ToString(aesKey).Replace("-", ""));
                log.Data("IV ", BitConverter.ToString(iv).Replace("-", ""));
                log.Data("IV Length", iv.Length.ToString());
                // 3. Encrypt Msg using AES-GCM


                var (cipherText, tag) = CryptoGcmHelper.AesgcmEncrypt(aesKey, iv, msgBytes);



                byte[] encryptedDataBytes = new byte[cipherText.Length + tag.Length];

                Buffer.BlockCopy(cipherText, 0, encryptedDataBytes, 0, cipherText.Length);
                Buffer.BlockCopy(tag, 0, encryptedDataBytes, cipherText.Length, tag.Length);

                byte[] finalPayload = new byte[iv.Length + encryptedDataBytes.Length];

                Buffer.BlockCopy(iv, 0, finalPayload, 0, iv.Length);
                Buffer.BlockCopy(encryptedDataBytes, 0, finalPayload, iv.Length, encryptedDataBytes.Length);
                log.Step("STEP 3  Encryption of Data AesKey,IV,TXNID through AesGCMEncrypt Method");
                string encryptedData = Convert.ToBase64String(finalPayload);
                log.Data("Encrypted Payload", encryptedData);

                // 4. Encrypt AES key using Protean RSA public key
                var Cert = new X509Certificate2(uidaiEncryptionCertificate);
                log.Step("CERTIFICATE INFO Protean Encryption Certificate");
                log.Data("Subject", Cert.Subject);
                log.Data("Issuer", Cert.Issuer);
                log.Data("Thumbprint", Cert.Thumbprint);
                log.Data("Serial Number", Cert.SerialNumber);
                log.Data("Public Key Algorithm", Cert.PublicKey.Oid.FriendlyName);

                byte[] encryptedSessionKey;

                using (var rsa = Cert.GetRSAPublicKey())
                {
                    var rsaParams = rsa.ExportParameters(false);
                    var bcKey = DotNetUtilities.GetRsaPublicKey(rsaParams);

                    byte[] label = iv; // IMPORTANT: label = IV

                    IAsymmetricBlockCipher cipher = new OaepEncoding(
                        new RsaEngine(),
                        new Sha256Digest(),
                        new Sha256Digest(),
                        label
                    );

                    cipher.Init(true, bcKey);

                    encryptedSessionKey = cipher.ProcessBlock(aesKey, 0, aesKey.Length);
                }
                string requestSessionKey = Convert.ToBase64String(encryptedSessionKey);
                log.Data("Encoded requestSession Key with Base64String ", requestSessionKey);
                log.Data("requestSessionKey Length", requestSessionKey.Length.ToString());
                 var msgId = Guid.NewGuid().ToString();
                var msgts = request.MsgTs;
                var UserId = request.UserId;
                // 5. Build Header (single, flat, correct)
                //thumbprint = proteanCert.Thumbprint.Replace(" ", "").ToUpperInvariant(),
                var msgHeader = new
                {
                    alg = "AES-256-GCM",
                    enc = "RSA-OAEP",
                    requestSessionKey = requestSessionKey,
                    thumbprint = Cert.Thumbprint.Replace(" ", "").ToUpperInvariant(),
                    iv = Convert.ToBase64String(iv)
                };
                var msg = new
                {
                    txnId = txnId,
                    header = msgHeader,
                    data = encryptedData
                };
                log.Step("STEP 5 Generate requestHMAC");
                string msgJsonWithoutHmac = JsonSerializer.Serialize(msg);
                log.Data("msgWithoutHmac JSON", msgJsonWithoutHmac);

                string requestHmac;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(license)))
                {
                    requestHmac = Convert.ToBase64String(
                        hmac.ComputeHash(Encoding.UTF8.GetBytes(msgJsonWithoutHmac))
                    );
                }
                log.Data("requestHMAC", requestHmac);

                var msgWithHmac = new
                {
                    txnId = txnId,
                    header = msgHeader,
                    data = encryptedData,
                    requestHMAC = requestHmac
                };


                var header = new
                {
                    ver = "1.0",
                    msgId = msgId,
                    msgTs = msgts,
                    lk = license,
                    ac = aua,
                    sa = subAua,
                    action = "notify",
                    isMessageEncrypted = true,

                };

                log.Step("STEP 6 Generate DATA Signature");
                string dataheaderJson = JsonSerializer.Serialize(header);
                string msgJson = JsonSerializer.Serialize(msgWithHmac);

                log.Data("Header JSON", dataheaderJson);
                log.Data("Message JSON", msgJson);
              

                string dataSignature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(license)))
                {
                    dataSignature = Convert.ToBase64String(
                        hmac.ComputeHash(
                            Encoding.UTF8.GetBytes(dataheaderJson + msgJson)
                        )
                    );
                }
                log.Data("DATA Signature", dataSignature);

                var dataObject = new
                {
                    signature = dataSignature,
                    header = header,
                    msg = msgWithHmac
                };

                string dataJson = JsonSerializer.Serialize(dataObject);

                // 6. Build unsigned request
                var Options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var RequestToSign = new
                {
                    data = dataObject
                };

                // SERIALIZE ONCE
                string jsonWithRequest = JsonSerializer.Serialize(RequestToSign, Options);

                log.Step("STEP 7 DSC Signing");
                log.Data("Exact JSON used for DSC signing", jsonWithRequest);

                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonWithRequest);
                log.Data("Byte Length", dataBytes.Length.ToString());
                string rootSignature = SignWithDSC(
                         dataBytes,
                             signingCertPath,
                          signingCertPassword
                    );


                var finalRequest = new
                {
                    data = dataObject,
                    signature = rootSignature
                };



                string finalJson = JsonSerializer.Serialize(finalRequest, Options);

               
                var key = Convert.ToBase64String(iv);
                await InsertPollingRequest(txnId, msgId, finalJson, "Pending", "PreProd", Constr, jsonWithRequest, key, requestHmac);
                log.Step("STEP 8 Final Request");
                log.Data("Final JSON sent to ASA", finalJson);

                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                // var finalJson = JsonSerializer.Serialize(finalrequest);
                using (var client = new HttpClient())
                {

                    client.Timeout = TimeSpan.FromSeconds(30);

                    var httpResponse = await client.PostAsync(PollingEndpoint, new StringContent(finalJson, Encoding.UTF8, "application/json"));
                    var responsejson = await httpResponse.Content.ReadAsStringAsync();

                    log.Step("STEP 9 ASA Response");

                    log.Data("HTTP Status", httpResponse.StatusCode.ToString());
                    log.Data("Response JSON", responsejson);
                    // 1. Parse root JSON safely
                    var root = JsonSerializer.Deserialize<JsonElement>(responsejson);


                    if (!root.TryGetProperty("response", out var response))
                        throw new Exception("Invalid ASA response");


                    var respMsg = response.GetProperty("msg");
                    var Header = respMsg.GetProperty("header");

                    // ----------------------------
                    // 1. Extract fields
                    // ----------------------------
                    string EncSessionKeyB64 = Header.GetProperty("requestSessionKey").GetString();
                    string DataB64 = respMsg.GetProperty("data").GetString();
                    string recordPending = respMsg.GetProperty("recordPending").GetString();
                    //string TxnId = respMsg.GetProperty("txnId").GetString();
                    string TxnId = respMsg.GetProperty("txnId").GetString();
                    // ----------------------------


                    // ----------------------------
                    // STEP 2: Decode Base64
                    // ----------------------------
                    byte[] FullPayload = Convert.FromBase64String(DataB64);
                    byte[] EncSessionKey = Convert.FromBase64String(EncSessionKeyB64);

                    // ----------------------------
                    // STEP 3: Extract IV (first 16 bytes)
                    // ----------------------------
                    byte[] IV = new byte[16];
                    Buffer.BlockCopy(FullPayload, 0, IV, 0, 16);

                    // ----------------------------
                    // STEP 4: Extract encrypted_data (NO SPLITTING)
                    // ----------------------------
                    int encLen = FullPayload.Length - 16;
                    byte[] EncryptedData = new byte[encLen];

                    Buffer.BlockCopy(FullPayload, 16, EncryptedData, 0, encLen);

                    // ----------------------------
                    // STEP 5: RSA decrypt AES key (LABEL = IV)
                    // ----------------------------
                    var cert = new X509Certificate2(
                        ConfigurationManager.AppSettings["Uidai.DecryptionCertificate"],
                        ConfigurationManager.AppSettings["Uidai.DecryptionPassword"],
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable
                    );

                    byte[] AesKey;

                    using (var rsa = cert.GetRSAPrivateKey())
                    {
                        var keyPair = DotNetUtilities.GetKeyPair(rsa);

                        var rsaCipher = new OaepEncoding(
                            new RsaEngine(),
                            new Sha256Digest(),
                            new Sha256Digest(),
                            IV // IMPORTANT LABEL
                        );

                        rsaCipher.Init(false, keyPair.Private);

                        AesKey = rsaCipher.ProcessBlock(EncSessionKey, 0, EncSessionKey.Length);
                    }

                    // ----------------------------
                    // STEP 6: Prepare NONCE (last 12 bytes of IV)
                    // ----------------------------
                    byte[] Nonce = new byte[12];
                    Buffer.BlockCopy(IV, IV.Length - 12, Nonce, 0, 12);


                    // STEP 7: AES-GCM DECRYPT (NO MANUAL TAG SPLIT)

                    var cipher = new GcmBlockCipher(new AesEngine());

                    var parameters = new AeadParameters(
                        new KeyParameter(AesKey),
                        128,
                        Nonce,
                        IV // AAD
                    );

                    cipher.Init(false, parameters);

                    byte[] output = new byte[cipher.GetOutputSize(EncryptedData.Length)];

                    int len = cipher.ProcessBytes(EncryptedData, 0, EncryptedData.Length, output, 0);
                    cipher.DoFinal(output, len);

                    string decryptedJson = Encoding.UTF8.GetString(output);
                    await UpdatePollingRequest(
     Constr,
     txnId,     // ✅ your generated ID
     decryptedJson,
     null,
     null,
     recordPending,
     null,
     null,
     null,
     responsejson
 );
                    // STEP 8: RETURN

                    return Ok(new
                    {
                        success = true,
                        TxnId = TxnId,
                        recordPending = recordPending,
                        data = decryptedJson
                    });
                    //string connStr,string Response,string Message,string RespTimeStamp,string RecordPending,string Error, string ErrorCode, string Exception,string ResponseJson
                   
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);

            }


        }






        private static string BuildRequestHmac(string secret, string plainMsgJson)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var msgBytes = Encoding.UTF8.GetBytes(plainMsgJson);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                return Convert.ToBase64String(hmac.ComputeHash(msgBytes));
            }
        }





        private static string SignWithDSC(
     byte[] dataBytes,
     string certPath,
     string password)
        {
            var cert = new X509Certificate2(
                certPath,
                password,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.PersistKeySet
            );

            if (!cert.HasPrivateKey)
                throw new Exception("DSC certificate does not contain a private key");

            using (RSA rsa = cert.GetRSAPrivateKey())
            {


                File.WriteAllText(@"C:\UIDAI_DEBUG\CERT_RUNTIME.txt",
                  $"Subject: {cert.Subject}\n" +
                  $"Issuer: {cert.Issuer}\n" +
                  $"Serial: {cert.SerialNumber}\n" +
                  $"Thumbprint: {cert.Thumbprint}\n" +
                  $"HasPrivateKey: {cert.HasPrivateKey}\n" +
                  $"KeyType: {rsa?.KeySize}\n");
                if (rsa == null)
                    throw new Exception("Unable to access RSA private key from DSC");

                byte[] signature = rsa.SignData(
                    dataBytes,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                string signatureHex = BitConverter
           .ToString(signature)
           .Replace("-", "");

                File.WriteAllText(
                    @"C:\UIDAI_DEBUG\SIGNATURE_RAW_HEX.txt",
                    signatureHex
                );


                return Convert.ToBase64String(signature);
            }
        }


        private static string HmacBase64(string secret, string text)
        {

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hash);
            }


        }
        private static async Task<long> InsertRequestLog(
               string connStr,
               string msgId,
               string notifyEndpoint,
               string startDate,
               string schedule,
               string finalJson,
               string dataBlockJson,
               string status,
             string testOrProd,
             string txxnId,     // NEW
    string userId)

        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("usp_Insert_Subscription_Status_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var now = DateTime.UtcNow;
                cmd.Parameters.AddWithValue("@Txnid", txxnId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@TransactionNo", Guid.NewGuid().ToString("N"));
                cmd.Parameters.AddWithValue("@TransactionDate", now);
                cmd.Parameters.AddWithValue("@Version", "1.0.0");
                cmd.Parameters.AddWithValue("@MsgId", msgId);
                cmd.Parameters.AddWithValue("@AUACode", ConfigurationManager.AppSettings["Uidai.AuaCode"]);
                cmd.Parameters.AddWithValue("@SubAUACode", ConfigurationManager.AppSettings["Uidai.SubAuaCode"]);
                cmd.Parameters.AddWithValue("@APIName", "SUBSCRIPTION");
                cmd.Parameters.AddWithValue("@Server", Environment.MachineName);
                cmd.Parameters.AddWithValue("@TestOrProduction", testOrProd);
                cmd.Parameters.AddWithValue("@Action", "subscribe");
                cmd.Parameters.AddWithValue("@isMessageEncrypted", false);

                cmd.Parameters.AddWithValue("@NotifyEndpoint", notifyEndpoint);
                cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(startDate));
                cmd.Parameters.AddWithValue("@Schedule", schedule);

                // These two matter for audit
                cmd.Parameters.AddWithValue("@Data", dataBlockJson);   // Only data block
                cmd.Parameters.AddWithValue("@Request", finalJson);   // Full request JSON

                // Pre-call values
                cmd.Parameters.AddWithValue("@Status", status); // "REQUEST_SENT"
                cmd.Parameters.AddWithValue("@Response", DBNull.Value);
                cmd.Parameters.AddWithValue("@RespTimestamp", DBNull.Value);
                cmd.Parameters.AddWithValue("@Message", "Request logged");
                cmd.Parameters.AddWithValue("@Error", DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorCode", DBNull.Value);
                cmd.Parameters.AddWithValue("@Exception", DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDate", now);
                cmd.Parameters.AddWithValue("@ResponseJson", DBNull.Value);

                await conn.OpenAsync();

                var result = await cmd.ExecuteScalarAsync();

                return Convert.ToInt64(result);
            }
        }

        private static async Task UpdateResponseLog(
    string connStr,
    long transactionLogId,
    string status,
    string responseText,
    string message,
    string responseJson,
    string error,
    string errorCode,
    string exception)
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("usp_Update_Subscription_Status_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TransactionLogid", transactionLogId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Response", responseText);
                cmd.Parameters.AddWithValue("@RespTimestamp", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Message", message);

                cmd.Parameters.AddWithValue("@Error",
                    string.IsNullOrWhiteSpace(error) ? (object)DBNull.Value : error);

                cmd.Parameters.AddWithValue("@ErrorCode",
                    string.IsNullOrWhiteSpace(errorCode) ? (object)DBNull.Value : errorCode);

                cmd.Parameters.AddWithValue("@Exception",
                    string.IsNullOrWhiteSpace(exception) ? (object)DBNull.Value : exception);

                cmd.Parameters.AddWithValue("@ResponseJson",
                    string.IsNullOrWhiteSpace(responseJson) ? (object)DBNull.Value : responseJson);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }



        private static async Task InsertPollingRequest(
            string TransactionNo,
            string MsgId,
            string finalJson,
            string Status,
            string testOrProd,
            string connStr,
            string DataJson,
            string Iv,
            string RequestHmac
            )
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("usp_Insert_Status_Polling_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TransactionNo", TransactionNo);
                cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ApiName", "Polling");
                cmd.Parameters.AddWithValue("@Server", Environment.MachineName);
                cmd.Parameters.AddWithValue("@TestOrProduction", "PreProd");
                cmd.Parameters.AddWithValue("@Data", DataJson);
                cmd.Parameters.AddWithValue("@Version", "1.0.0");
                cmd.Parameters.AddWithValue("@MsgId", MsgId);
                cmd.Parameters.AddWithValue("@AUACode", ConfigurationManager.AppSettings["Uidai.AuaCode"]);
                cmd.Parameters.AddWithValue("@SubAUACode", ConfigurationManager.AppSettings["Uidai.SubAuaCode"]);
                cmd.Parameters.AddWithValue("@Action", "Polling");
                cmd.Parameters.AddWithValue("@isMessageEncrypted", true);
                cmd.Parameters.AddWithValue("@Response", DBNull.Value);
                cmd.Parameters.AddWithValue("@Message", "Request Logged");
                cmd.Parameters.AddWithValue("@Iv", Iv);
                cmd.Parameters.AddWithValue("@RequestHMAC", RequestHmac);
                cmd.Parameters.AddWithValue("@RespTimestamp", DBNull.Value);
                cmd.Parameters.AddWithValue("@RecordPending", DBNull.Value);
                cmd.Parameters.AddWithValue("@Error", DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorCode", DBNull.Value);
                cmd.Parameters.AddWithValue("@Exception", DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@ResponseJson", DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task UpdatePollingRequest(
       string connStr,
       string transactionNo,   // ✅ YOUR internal TransactionNo (NOT UIDAI txnId)
       string response,
       string message,
       string respTimeStamp,
       string recordPending,
       string error,
       string errorCode,
       string exception,
       string responseJson
   )
        {
            if (string.IsNullOrEmpty(transactionNo))
                throw new Exception("TransactionNo is required for update");

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("usp_Update_Status_Polling_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;

                // 🔴 MUST MATCH SP PARAM NAMES EXACTLY
                cmd.Parameters.Add("@TransactionNo", SqlDbType.VarChar, 50).Value = transactionNo;

                cmd.Parameters.Add("@ResponseJson", SqlDbType.NVarChar).Value =
                    (object)responseJson ?? DBNull.Value;

                cmd.Parameters.Add("@Exception", SqlDbType.NVarChar).Value =
                    (object)exception ?? DBNull.Value;

                cmd.Parameters.Add("@ErrorCode", SqlDbType.NVarChar).Value =
                    (object)errorCode ?? DBNull.Value;

                cmd.Parameters.Add("@Error", SqlDbType.NVarChar).Value =
                    (object)error ?? DBNull.Value;

                cmd.Parameters.Add("@RecordPending", SqlDbType.VarChar, 10).Value =
                    (object)recordPending ?? DBNull.Value;

                cmd.Parameters.Add("@RespTimestamp", SqlDbType.VarChar, 50).Value =
                    (object)respTimeStamp ?? DBNull.Value;

                cmd.Parameters.Add("@Message", SqlDbType.NVarChar).Value =
                    (object)message ?? DBNull.Value;

                cmd.Parameters.Add("@Response", SqlDbType.NVarChar).Value =
                    (object)response ?? DBNull.Value;

                try
                {
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("UpdatePollingRequest failed: " + ex.Message, ex);
                }
            }
        }

    }
}

