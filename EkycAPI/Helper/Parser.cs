using EkycAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace EkycAPI.Helper
{
    public class Parser
    {
        public static PollingResult ParsePollingPayload(
    string decryptedJson,
    string recordPendingFromEnvelope)
        {
            var result = new PollingResult
            {
                RecordPending = recordPendingFromEnvelope
            };

            var root = JsonSerializer.Deserialize<JsonElement>(decryptedJson);

            // txnId
            if (root.TryGetProperty("txnId", out var txnProp))
            {
                result.TxnId = txnProp.GetString();
            }

            // uids array
            if (root.TryGetProperty("uids", out var uidsProp) &&
                uidsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var uid in uidsProp.EnumerateArray())
                {
                    var item = new UidStatus();

                    if (uid.TryGetProperty("token", out var tokenProp))
                        item.Token = tokenProp.GetString();

                    if (uid.TryGetProperty("status", out var statusProp))
                        item.Status = statusProp.GetString();

                    result.Uids.Add(item);
                }
            }

            return result;
        }

    }
}