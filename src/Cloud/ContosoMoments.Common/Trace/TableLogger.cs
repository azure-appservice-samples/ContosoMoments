using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContosoMoments.Common
{
    /// <summary>
    /// call Azure Function (HTTP endpoint) to log messages into table store
    /// </summary>
    public static class TableLogger
    {
        static Uri baseAddress;

        /// <summary>
        /// TableLogger Initalizer. Call once in app lifetime to init
        /// </summary>
        /// <param name="baseUri"></param>
        public static void Init(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri) ||
                !Uri.IsWellFormedUriString(baseUri, UriKind.RelativeOrAbsolute))
            {
                return; // Avoiding Errors if improperly configured
            }
            baseAddress = new Uri(baseUri);
        }

        /// <summary>
        /// post msg to TableLogger URI
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <param name="msg"></param>
        /// <param name="functionUriWithToken"></param>
        /// <returns></returns>
        public static HttpResponseMessage LogMessage(String partitionKey, string rowKey, Object msg, string functionUriWithToken)
        {
            if (default(Uri) == baseAddress
                || string.IsNullOrWhiteSpace(partitionKey) 
                || string.IsNullOrWhiteSpace(rowKey) 
                || !Uri.IsWellFormedUriString(functionUriWithToken, UriKind.Relative) 
                || msg == null)
            {
                return new HttpResponseMessage(); // Avoiding Errors if improperly configured
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = baseAddress;

                JObject o = (JObject)JToken.FromObject(msg);
                var request = new StringContent(o.ToString());
                request.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var result = client.PostAsync(functionUriWithToken, request).Result;
                return result;
            }
        }

        public static Task<HttpListenerResponse> LogMessageAsyc(String partitionKey, string rowKey, Object msg, string relativApiUri)
        {
            throw new NotImplementedException();
        }
    }
}
