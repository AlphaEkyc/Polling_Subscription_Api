//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EkycAPI.Services.Interface
//{
//    public interface IAuditRepository
//    {

//        Task<long> InsertSubscriptionRequestAsync(
//           SubscriptionAuditRequest auditRequest,
//           byte[] requestHash,
//           string correlationId);

//        Task UpdateSubscriptionResponseAsync(
//            long transactionLogId,
//            SubscriptionAuditResponse auditResponse,
//            byte[] responseHash,
//            string correlationId);

//        Task LogCryptoEventAsync(
//            string correlationId,
//            string txnId,
//            string algorithm,
//            string keyThumbprint,
//            byte[] sessionKeyHash,
//            bool success);









//    }
//}
