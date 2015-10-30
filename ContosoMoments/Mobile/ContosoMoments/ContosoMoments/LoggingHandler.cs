using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments
{
    public class LoggingHandler : DelegatingHandler
    {
        private bool logRequestResponseBody;

        public LoggingHandler(bool logRequestResponseBody = false)
        {
            this.logRequestResponseBody = logRequestResponseBody;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("Request: {0} {1}", request.Method, request.RequestUri.ToString());

            if (logRequestResponseBody && request.Content != null)
            {
                var requestContent = await request.Content.ReadAsStringAsync();
                Console.WriteLine(requestContent);
            }

            Console.WriteLine("HEADERS");

            foreach (var header in request.Headers)
            {
                Console.WriteLine(string.Format("{0}:{1}", header.Key, string.Join(",", header.Value)));
            }

            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response: {0}", response.StatusCode);

            if (logRequestResponseBody)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }

            return response;
        }
    }
}
