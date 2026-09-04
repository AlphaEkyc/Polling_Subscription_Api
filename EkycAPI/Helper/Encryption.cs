using Jose;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;


namespace EkycAPI.Helper
{
    public class Encryption
    {
        public static byte[] GenerateSKey()
        {
            byte[] key = new byte[32];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            return key;
        }


        public static string EncryptSessionKey_WithOAEP_SHA256(
      byte[] sessionKey,
      string CertificatePath,
      byte[] iv)
        {
            X509Certificate2 cert =
                new X509Certificate2(CertificatePath);

            RSA rsaPublicKey =
                cert.GetRSAPublicKey();

            AsymmetricKeyParameter bcPublicKey =
                DotNetUtilities.GetRsaPublicKey(rsaPublicKey);

            IAsymmetricBlockCipher cipher =
                new OaepEncoding(
                    new RsaEngine(),
                    new Sha256Digest(),   // OAEP digest
                    new Sha256Digest(),   // MGF1 digest
                    iv                    // OAEP label
                );

            cipher.Init(true, bcPublicKey);

            byte[] encrypted =
                cipher.ProcessBlock(
                    sessionKey,
                    0,
                    sessionKey.Length
                );

            return Convert.ToBase64String(encrypted);
        }
        public Dictionary<string, string> EncryptDataUsingAesGcm(
    byte[] sessionKey,
    string plainText,
    byte[] iv)
        {
            Dictionary<string, string> objDictionary =
                new Dictionary<string, string>();

            byte[] plaintext =
                Encoding.UTF8.GetBytes(plainText);

            // Nonce = last 12 bytes of IV
            byte[] nonce = new byte[12];

            Array.Copy(
                iv,
                4,
                nonce,
                0,
                12);

            // AES-GCM
            GcmBlockCipher gcmCipher =
                new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());

            AeadParameters parameters =
                new AeadParameters(
                    new KeyParameter(sessionKey),
                    128,
                    nonce,
                    iv);

            gcmCipher.Init(true, parameters);

            // Output contains CipherText + 16-byte Authentication Tag
            byte[] output =
                new byte[gcmCipher.GetOutputSize(plaintext.Length)];

            int length =
                gcmCipher.ProcessBytes(
                    plaintext,
                    0,
                    plaintext.Length,
                    output,
                    0);

            length +=
                gcmCipher.DoFinal(
                    output,
                    length);

            // Original TestController format:
            // IV + CipherText + Tag

            byte[] combined =
                new byte[iv.Length + output.Length];

            Array.Copy(
                iv,
                0,
                combined,
                0,
                iv.Length);

            Array.Copy(
                output,
                0,
                combined,
                iv.Length,
                output.Length);

            string ivString =
                Convert.ToBase64String(iv);

            objDictionary.Add(
                "EncPID",
                Convert.ToBase64String(combined));

            objDictionary.Add(
                "IV",
                ivString);

            objDictionary.Add(
                "Status",
                "SUCCESS");

            return objDictionary;
        }


        public string Sha256_hash(
    byte[] plaintext,
    string ReqMsgTs,
    byte[] aeskey)
        {
            // plaintext is already the SHA-256 hash
            // generated from InputRequest.

            // Convert SHA-256 hash to uppercase hexadecimal.
            string hashHex = BitConverter
                .ToString(plaintext)
                .Replace("-", "")
                .ToUpperInvariant();

            byte[] hashBytes =
                Encoding.UTF8.GetBytes(hashHex);

            /*
             * The original method signature does not provide an IV.
             * Therefore ReqMsgTs is retained as a parameter for
             * compatibility with the existing controller.
             *
             * Do not use it as an arbitrary cryptographic key.
             */

            // Generate a separate 12-byte nonce for AES-GCM.
            byte[] nonce = new byte[12];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }

            GcmBlockCipher gcmCipher =
                new GcmBlockCipher(
                    new Org.BouncyCastle.Crypto.Engines.AesEngine());

            AeadParameters parameters =
                new AeadParameters(
                    new KeyParameter(aeskey),
                    128,
                    nonce,
                    null);

            gcmCipher.Init(true, parameters);

            byte[] encrypted =
                new byte[gcmCipher.GetOutputSize(hashBytes.Length)];

            int length =
                gcmCipher.ProcessBytes(
                    hashBytes,
                    0,
                    hashBytes.Length,
                    encrypted,
                    0);

            length +=
                gcmCipher.DoFinal(
                    encrypted,
                    length);

            // AES-GCM output = ciphertext + authentication tag.
            byte[] result =
                new byte[nonce.Length + length];

            Array.Copy(
                nonce,
                0,
                result,
                0,
                nonce.Length);

            Array.Copy(
                encrypted,
                0,
                result,
                nonce.Length,
                length);

            // Base64URL
            return Convert.ToBase64String(result)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }




        public static string GenerateSignature(
    string pfxPath,
    string pfxPassword,
    string header,
    string message)
        {
            var cert = new X509Certificate2(
                pfxPath,
                pfxPassword,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.EphemeralKeySet);

            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                string dataToSign = header + "||" + message;

                byte[] dataBytes =
                    Encoding.UTF8.GetBytes(dataToSign);

                byte[] hash;

                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(dataBytes);
                }

                byte[] signature =
                    rsa.SignHash(
                        hash,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                return Convert.ToBase64String(signature);
            }
        }



        public string DecryptDataUsingAesGcm(
       byte[] sessionKey,
       byte[] respencryteddata,
       byte[] ivresp)
        {
            try
            {
                if (sessionKey == null || sessionKey.Length == 0)
                    throw new ArgumentException("Session key is empty.");

                if (respencryteddata == null || respencryteddata.Length < 32)
                    throw new ArgumentException("Encrypted response data is invalid.");

                if (ivresp == null || ivresp.Length != 16)
                    throw new ArgumentException("IV must be 16 bytes.");

                // -----------------------------------------
                // Create 12-byte nonce
                // Same logic as original implementation
                // -----------------------------------------

                byte[] nonce = new byte[12];

                Array.Copy(
                    ivresp,
                    4,
                    nonce,
                    0,
                    12);


                // -----------------------------------------
                // Remove first 16 bytes
                // -----------------------------------------

                byte[] ciphertextWithTag =
                    new byte[respencryteddata.Length - 16];

                Array.Copy(
                    respencryteddata,
                    16,
                    ciphertextWithTag,
                    0,
                    ciphertextWithTag.Length);


                // -----------------------------------------
                // BouncyCastle AES-GCM
                // -----------------------------------------

                var cipher =
                    new Org.BouncyCastle.Crypto.Modes.GcmBlockCipher(
                        new Org.BouncyCastle.Crypto.Engines.AesEngine());


                var keyParameter =
                    new Org.BouncyCastle.Crypto.Parameters.KeyParameter(
                        sessionKey);


                var parameters =
                    new Org.BouncyCastle.Crypto.Parameters.AeadParameters(
                        keyParameter,
                        128,               // 128-bit authentication tag
                        nonce,
                        ivresp);           // AAD


                cipher.Init(false, parameters);


                // -----------------------------------------
                // Decrypt
                // ciphertextWithTag already contains:
                //
                // ciphertext + 16 byte GCM tag
                // -----------------------------------------

                byte[] plaintext =
                    new byte[cipher.GetOutputSize(ciphertextWithTag.Length)];

                int length =
                    cipher.ProcessBytes(
                        ciphertextWithTag,
                        0,
                        ciphertextWithTag.Length,
                        plaintext,
                        0);

                length +=
                    cipher.DoFinal(
                        plaintext,
                        length);


                // -----------------------------------------
                // Convert plaintext to string
                // -----------------------------------------

                return Encoding.UTF8.GetString(
                    plaintext,
                    0,
                    length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "DecryptDataUsingAesGcm: " +
                    ex.Message +
                    " | " +
                    ex.InnerException +
                    " | " +
                    ex.TargetSite +
                    " | " +
                    ex.HelpLink);

                return ex.Message;
            }
        }

        public static byte[] DecryptSessionKey_WithOAEP_SHA256(
            string pfxPath,
            string pfxPassword,
            byte[] encryptedSessionKey,
            byte[] iv)
        {
            try
            {
                var cert = new X509Certificate2(
                    pfxPath,
                    pfxPassword,
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable
                );

                var rsa = cert.GetRSAPrivateKey();

                var bcPrivateKey =
                    DotNetUtilities.GetKeyPair(rsa).Private;

                OaepEncoding rsaOaep =
                    new OaepEncoding(
                        new RsaEngine(),
                        new Sha256Digest(),
                        iv
                    );

                rsaOaep.Init(
                    false,
                    bcPrivateKey
                );

                byte[] decrypted =
                    rsaOaep.ProcessBlock(
                        encryptedSessionKey,
                        0,
                        encryptedSessionKey.Length
                    );

                return decrypted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "DecryptSessionKey_WithOAEP_SHA256: " +
                    ex.Message);

                return null;
            }
        }

    }
}