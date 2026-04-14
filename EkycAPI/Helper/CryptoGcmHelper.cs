using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EkycAPI.Helper
{
    public class CryptoGcmHelper
    {

        //public static (byte[] Cipher, byte[] Tag) AesgcmEncrypt(
        //  byte[] key,
        //  byte[] iv,
        // byte[] plaintext)
        //{
        //    var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        //    var parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

        //    cipher.Init(true, parameters);

        //    var output = new byte[cipher.GetOutputSize(plaintext.Length)];
        //    var len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, output, 0);
        //    cipher.DoFinal(output, len);

        //    // BouncyCastle returns cipher+tag together
        //    var tag = new byte[16];
        //    var encrypted = new byte[output.Length - 16];

        //    Buffer.BlockCopy(output, output.Length - 16, tag, 0, 16);
        //    Buffer.BlockCopy(output, 0, encrypted, 0, encrypted.Length);

        //    return (encrypted, tag);
        //}

        public static (byte[] Cipher, byte[] Tag) AesgcmEncrypt(
                 byte[] key,
                 byte[] iv,
                 byte[] plaintext)
                {
            byte[] nonce = new byte[12];
            Buffer.BlockCopy(iv, 4, nonce, 0, 12);

            var cipher = new GcmBlockCipher(new AesEngine());

            var parameters = new AeadParameters(
                new KeyParameter(key),
                128,
                nonce,
                iv
            );

            cipher.Init(true, parameters);

            byte[] output = new byte[cipher.GetOutputSize(plaintext.Length)];

            int len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, output, 0);
            cipher.DoFinal(output, len);

            byte[] tag = new byte[16];
            byte[] encrypted = new byte[output.Length - 16];

            Buffer.BlockCopy(output, output.Length - 16, tag, 0, 16);
            Buffer.BlockCopy(output, 0, encrypted, 0, encrypted.Length);

            return (encrypted, tag);
        }

        public static byte[] AesGcmDecrypt(
    byte[] key,
    byte[] iv,
    byte[] cipher,
    byte[] tag)
        {
            var cipherTextWithTag = new byte[cipher.Length + tag.Length];
            Buffer.BlockCopy(cipher, 0, cipherTextWithTag, 0, cipher.Length);
            Buffer.BlockCopy(tag, 0, cipherTextWithTag, cipher.Length, tag.Length);

            var gcm = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

            gcm.Init(false, parameters);

            var output = new byte[gcm.GetOutputSize(cipherTextWithTag.Length)];
            var len = gcm.ProcessBytes(cipherTextWithTag, 0, cipherTextWithTag.Length, output, 0);
            gcm.DoFinal(output, len);

            return output;
        }

    }
}