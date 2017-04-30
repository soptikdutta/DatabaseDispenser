using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    interface ILogger
    {
        void LogInformation(string message, Dictionary<string, string> properties);

        void LogRequest(string requestPath, HttpMethod httpMethod, Uri url, string responseCode, bool isSuccess,
            IDictionary<string, string> requestHeaders,
            IDictionary<string, string> requestParameters, string requestBody,
            IDictionary<string, string> responseHeaders, string responseBody);

        void LogException(Exception exception);
    }
}
