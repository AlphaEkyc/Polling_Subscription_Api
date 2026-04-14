using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkycAPI.Services.Interface
{
    public interface ICryptoService
    {

        string GenerateHmacBase64(byte[] data);
        bool VerifyHmacBase64(byte[] data, string expectedBase64Hmac);
    }
}
