using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Jose;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System.Net.Http;
using Newtonsoft.Json;

namespace WebApplication1.Services
{
    public class SalesforceTokenManager
    {
        private readonly string _clientId;
        private readonly string _userName;
        private readonly string _privateKey;

        public SalesforceTokenManager(string clientId, string userName, string privateKey)
        {
            _clientId = clientId;
            _userName = userName;
            _privateKey = privateKey;
        }

        public string CreateToken()
        {
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();

            var payload = new Dictionary<string, object>
            {
                { "alg", "RS256" },
                { "iss", _clientId },
                { "prn", _userName },
                { "aud", "https://test.salesforce.com" },
                { "exp", now + 3600 }
            };

            return SignToken(payload);
        }

        private string SignToken(Dictionary<string, object> payload)
        {
            string jwt;
            RsaPrivateCrtKeyParameters key;

            using (var stringReader = new StringReader(_privateKey))
            {
                var pemReader = new PemReader(stringReader);
                key = (RsaPrivateCrtKeyParameters)pemReader.ReadObject();
            }

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(ToRsaParameters(key));
                jwt = JWT.Encode(payload, rsa, JwsAlgorithm.RS256);
            }

            return jwt;
        }

        /// <summary>
        /// https://github.com/neoeinstein/bouncycastle/blob/master/crypto/src/security/DotNetUtilities.cs
        /// </summary>
        /// <param name="privKey">string</param>
        /// <returns></returns>
        private static RSAParameters ToRsaParameters(RsaPrivateCrtKeyParameters privKey) => new RSAParameters
        {
            Modulus = privKey.Modulus.ToByteArrayUnsigned(),
            Exponent = privKey.PublicExponent.ToByteArrayUnsigned(),
            D = privKey.Exponent.ToByteArrayUnsigned(),
            P = privKey.P.ToByteArrayUnsigned(),
            Q = privKey.Q.ToByteArrayUnsigned(),
            DP = privKey.DP.ToByteArrayUnsigned(),
            DQ = privKey.DQ.ToByteArrayUnsigned(),
            InverseQ = privKey.QInv.ToByteArrayUnsigned()
        };

        public static async Task<AccessToken> getAccessToken(string clientId, string userName)
        {
            string privateKey = System.IO.File.ReadAllText("sfdc_private.key");

            SalesforceTokenManager jwt = new SalesforceTokenManager(
                clientId,
                userName,
                privateKey
            );

            var client = new HttpClient();
            var parameters = new Dictionary<string, string>();
            parameters["assertion"] = jwt.CreateToken();
            parameters["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer";

            var result = await client.PostAsync("https://test.salesforce.com/services/oauth2/token", new FormUrlEncodedContent(parameters));
            var contents = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AccessToken>(contents);
        }

        public class AccessToken
        {
            public string access_token { get; set; }
            public string id { get; set; }
            public string instance_url { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }
    }
}
