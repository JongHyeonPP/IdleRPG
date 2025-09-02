using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Purchase
{
    public class PurchaseController
    {
        private readonly HttpClient _http;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(ILogger<PurchaseController> logger)
        {
            _logger = logger;
            _http = new HttpClient();
        }

        [CloudCodeFunction("ProcessPurchase")]
        public async Task<PurchaseResult> ProcessPurchaseAsync(
            string receipt,
            string productId,
            string playerId,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            bool success = false;
            string reason = "unknown";
            string orderId = null;
            int? purchaseState = null;

            // 로그용 프리뷰/원문
            string receiptPreview = null;   // 원문 receipt 일부(최대 220자)
            string payloadRaw = null;       // Payload 또는 inner json 원문(최대 1000자)

            try
            {
                string purchaseToken = null;
                string pidFromReceipt = null;

                if (string.IsNullOrWhiteSpace(receipt))
                    return Fail("invalid receipt empty");

                receiptPreview = Short(receipt, 220);

                using (var doc = System.Text.Json.JsonDocument.Parse(receipt))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Payload", out var payloadEl))
                    {
                        var payloadStr = payloadEl.GetString();
                        if (!string.IsNullOrEmpty(payloadStr))
                        {
                            // 기본은 payload 문자열을 보관
                            payloadRaw = Short(payloadStr, 1000);

                            using var payloadDoc = System.Text.Json.JsonDocument.Parse(payloadStr);
                            var payloadRoot = payloadDoc.RootElement;

                            // IAP v5 형태: Payload.json 안에 실제 내부 JSON 문자열이 들어있는 경우가 많음
                            if (payloadRoot.TryGetProperty("json", out var jsonEl))
                            {
                                var innerJson = jsonEl.GetString();
                                if (!string.IsNullOrEmpty(innerJson))
                                {
                                    // inner json을 최종 payload로 채택
                                    payloadRaw = Short(innerJson, 1000);

                                    using var innerDoc = System.Text.Json.JsonDocument.Parse(innerJson);
                                    var innerRoot = innerDoc.RootElement;

                                    if (innerRoot.TryGetProperty("purchaseToken", out var ptEl))
                                        purchaseToken = ptEl.GetString();
                                    if (innerRoot.TryGetProperty("productId", out var pidEl))
                                        pidFromReceipt = pidEl.GetString();
                                }
                            }

                            if (string.IsNullOrEmpty(purchaseToken))
                            {
                                if (payloadRoot.TryGetProperty("purchaseToken", out var ptEl2))
                                    purchaseToken = ptEl2.GetString();
                                if (payloadRoot.TryGetProperty("productId", out var pidEl2))
                                    pidFromReceipt = pidEl2.GetString();
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(purchaseToken))
                    return Fail("invalid receipt token missing");

                if (!string.IsNullOrEmpty(pidFromReceipt) && pidFromReceipt != productId)
                    return Fail($"productId mismatch receipt={pidFromReceipt} client={productId}");

                Dictionary<string, string> tokenInfo;
                string iapCatalogJson;
                try
                {
                    tokenInfo = purchaseSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "TOKEN_INFO");
                    iapCatalogJson = purchaseSystem.GetRemoteConfig<string>(context, gameApiClient, "IAP_CATALOG");
                }
                catch (Exception e)
                {
                    return Fail($"RC fetch error {e.Message}");
                }

                if (tokenInfo == null || string.IsNullOrEmpty(iapCatalogJson))
                    return Fail("RC missing TOKEN_INFO or IAP_CATALOG");

                tokenInfo.TryGetValue("PackageName", out var packageName);
                tokenInfo.TryGetValue("ServiceAccountEmail", out var serviceAccountEmail);

                string privateKeyPem;
                try
                {
                    var pkSecret = await gameApiClient.SecretManager.GetSecret(context, "PrivateKeyPem");
                    privateKeyPem = pkSecret?.Value?.Replace("\\n", "\n")?.Trim();
                }
                catch (Exception e)
                {
                    return Fail($"SecretManager fetch error {e.Message}");
                }

                if (string.IsNullOrWhiteSpace(packageName) ||
                    string.IsNullOrWhiteSpace(serviceAccountEmail) ||
                    string.IsNullOrWhiteSpace(privateKeyPem) ||
                    !privateKeyPem.Contains("BEGIN PRIVATE KEY"))
                    return Fail("TOKEN_INFO fields invalid");

                string accessToken;
                try
                {
                    accessToken = await GoogleAuth.GetAccessTokenAsync(_http, serviceAccountEmail, privateKeyPem);
                }
                catch (Exception e)
                {
                    return Fail($"google token error {e.Message}");
                }

                var url =
                    $"https://androidpublisher.googleapis.com/androidpublisher/v3/applications/{packageName}/purchases/products/{productId}/tokens/{purchaseToken}";
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return Fail($"verify failed http {(int)resp.StatusCode} {body}");

                using (var vdoc = System.Text.Json.JsonDocument.Parse(body))
                {
                    var vroot = vdoc.RootElement;
                    if (vroot.TryGetProperty("purchaseState", out var psEl))
                        purchaseState = psEl.GetInt32();
                    if (vroot.TryGetProperty("orderId", out var oidEl))
                        orderId = oidEl.GetString();
                }

                if (purchaseState != 0)
                    return Fail($"verify failed purchaseState={purchaseState}");

                var grants = new List<(string currency, int amount)>();
                try
                {
                    using var catDoc = System.Text.Json.JsonDocument.Parse(iapCatalogJson);
                    var catRoot = catDoc.RootElement;
                    if (catRoot.TryGetProperty("products", out var products) &&
                        products.TryGetProperty(productId, out var productNode) &&
                        productNode.TryGetProperty("grants", out var grantsArr) &&
                        grantsArr.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var g in grantsArr.EnumerateArray())
                        {
                            var currency = g.TryGetProperty("currency", out var cEl) ? cEl.GetString() : null;
                            var amount = g.TryGetProperty("amount", out var aEl) ? aEl.GetInt32() : 0;
                            if (!string.IsNullOrEmpty(currency) && amount > 0)
                                grants.Add((currency, amount));
                        }
                    }
                }
                catch (Exception e)
                {
                    return Fail($"catalog parse error {e.Message}");
                }

                if (grants.Count == 0)
                    return Fail("catalog has no grants");

                var histKey = $"IAP_ORDER_{SanitizeKey(orderId ?? "noid")}";
                if (await ExistsCloudSaveItem(context, gameApiClient, histKey))
                    return Fail("duplicate order");

                var applied = await ApplyGrantsToGameDataAsync(context, gameApiClient, grants, _logger);
                if (!applied)
                    return Fail("save error");

                var histObj = new JObject
                {
                    ["productId"] = productId,
                    ["orderId"] = orderId ?? "",
                    ["grants"] = JArray.FromObject(grants),
                    ["ts"] = DateTime.UtcNow.ToString("o")
                };
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context, context.ServiceToken, context.ProjectId, context.PlayerId,
                    new(histKey, JsonConvert.SerializeObject(histObj)));

                success = true;
                reason = $"granted {grants.Count} grants orderId={orderId}";
                return new PurchaseResult { Success = true, Message = reason };
            }
            catch (Exception e)
            {
                reason = $"fatal error {e.Message}";
                return new PurchaseResult { Success = false, Message = reason };
            }
            finally
            {
                // 단 한 줄 로그: 성공=Debug, 실패=Error. Payload/Receipt 포함.
                if (success)
                {
                    _logger.LogDebug(
                        "[IAP] productId={0} playerId={1} success=True reason=\"{2}\" orderId={3} state={4} receiptPrev={5} payload={6}",
                        productId,
                        playerId,
                        reason,
                        orderId ?? "null",
                        purchaseState?.ToString() ?? "null",
                        receiptPreview ?? "null",
                        payloadRaw ?? "null"
                    );
                }
                else
                {
                    _logger.LogError(
                        "[IAP] productId={0} playerId={1} success=False reason=\"{2}\" orderId={3} state={4} receiptPrev={5} payload={6}",
                        productId,
                        playerId,
                        reason,
                        orderId ?? "null",
                        purchaseState?.ToString() ?? "null",
                        receiptPreview ?? "null",
                        payloadRaw ?? "null"
                    );
                }
            }

            PurchaseResult Fail(string msg)
            {
                reason = msg;
                return new PurchaseResult { Success = false, Message = msg };
            }

            static string Short(string s, int max) =>
                string.IsNullOrEmpty(s) ? s : (s.Length > max ? s.Substring(0, max) : s);
        }

        private static async Task<bool> ApplyGrantsToGameDataAsync(
            IExecutionContext context,
            IGameApiClient gameApiClient,
            List<(string currency, int amount)> grants,
            ILogger logger = null)
        {
            try
            {
                var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                    context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

                GameData data;
                if (res.Data.Results.Count == 0 || res.Data.Results[0].Value == null)
                {
                    data = new GameData { level = 1, maxStageNum = 1, currentStageNum = 1 };
                }
                else
                {
                    data = JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString())
                           ?? new GameData { level = 1, maxStageNum = 1, currentStageNum = 1 };
                }

                foreach (var (currency, amount) in grants)
                {
                    var key = currency?.Trim().ToLowerInvariant();
                    switch (key)
                    {
                        case "dia": data.dia = checked(data.dia + amount); break;
                        case "gold": data.gold = checked(data.gold + amount); break;
                        case "exp": data.exp = checked(data.exp + amount); break;
                        case "clover": data.clover = checked(data.clover + amount); break;
                        default:
                            break;
                    }
                }

                await gameApiClient.CloudSaveData.SetItemAsync(
                    context, context.ServiceToken, context.ProjectId, context.PlayerId,
                    new("GameData", JsonConvert.SerializeObject(data)));

                logger?.LogDebug("[IAP] grants applied: {0}", JsonConvert.SerializeObject(grants));
                return true;
            }
            catch (Exception e)
            {
                logger?.LogError("[IAP] ApplyGrants failed: {0}", e.Message);
                return false;
            }
        }

        private static async Task<bool> ExistsCloudSaveItem(
            IExecutionContext context,
            IGameApiClient gameApiClient,
            string key)
        {
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { key });

            return res.Data.Results.Count > 0 && res.Data.Results[0].Value != null;
        }

        private static string SanitizeKey(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if ((ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') ||
                    ch == '_' || ch == '-')
                    sb.Append(ch);
                else
                    sb.Append('_');
            }
            return sb.ToString();
        }
    }

    internal static class GoogleAuth
    {
        private const string Scope = "https://www.googleapis.com/auth/androidpublisher";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

        public static async Task<string> GetAccessTokenAsync(HttpClient http, string saEmail, string privateKeyPem)
        {
            string assertion = CreateJwtAssertion(saEmail, privateKeyPem);

            var req = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new KeyValuePair<string,string>("assertion", assertion),
                })
            };

            var resp = await http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Token fetch failed {resp.StatusCode} {body}");

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        private static string CreateJwtAssertion(string saEmail, string privateKeyPem)
        {
            static string B64Url(byte[] bytes) =>
                Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            var headerJson = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
            var now = DateTimeOffset.UtcNow;
            var payloadJson =
                $"{{\"iss\":\"{saEmail}\",\"scope\":\"{Scope}\",\"aud\":\"{TokenEndpoint}\"," +
                $"\"exp\":{now.ToUnixTimeSeconds() + 3600},\"iat\":{now.ToUnixTimeSeconds()} }}";

            var header = B64Url(Encoding.UTF8.GetBytes(headerJson));
            var payload = B64Url(Encoding.UTF8.GetBytes(payloadJson));
            var signingInput = $"{header}.{payload}";

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            var signature = rsa.SignData(
                Encoding.ASCII.GetBytes(signingInput),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return $"{signingInput}.{B64Url(signature)}";
        }
    }

    public class PurchaseResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
