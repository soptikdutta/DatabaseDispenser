using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DatabaseDispenser.Service.Common
{
    public class ApplicationInsightsLogger : ILogger
    {
        //private static readonly ConfigurationPackage ConfigurationPackage = FabricRuntime.GetActivationContext().GetConfigurationPackageObject("config");
        private readonly TelemetryClient _telemetry;

        public ApplicationInsightsLogger()
        {
            _telemetry = new TelemetryClient();

            //string instrumentationKey = ConfigurationPackage.Settings.Sections["Telemetry"].Parameters["InstrumentationKey"].Value;
            _telemetry.InstrumentationKey = "e8fe33b7-4c45-4c96-9c62-a0feecc60591";
        }

        public async void LogInformation(string message, Dictionary<string, string> properties)
        {
            await Task.Run(() =>
            {
                _telemetry.Context.Operation.Id = MultibannerContext.Current.CorrelationId;
                _telemetry.TrackTrace(message, SeverityLevel.Information, properties);
            });
        }

        public async void LogRequest(string requestPath, HttpMethod httpMethod, Uri url, string responseCode, bool isSuccess, IDictionary<string, string> requestHeaders,
            IDictionary<string, string> requestParameters, string requestBody,
            IDictionary<string, string> responseHeaders, string responseBody)
        {


            RequestTelemetry requestTelemetry = new RequestTelemetry();
            requestTelemetry.Id = MultibannerContext.Current.CorrelationId;
            //requestTelemetry.Duration = duration;
            //do not delete this http method. On removing logging is not working.
            requestTelemetry.HttpMethod = httpMethod.ToString();
            requestTelemetry.Name = requestPath;
            requestTelemetry.ResponseCode = responseCode;
            requestTelemetry.Timestamp = DateTime.UtcNow;
            requestTelemetry.Success = isSuccess;
            requestTelemetry.Url = url;
            requestTelemetry.Context.Operation.Name = requestPath;

            if (requestHeaders != null)
            {
                foreach (var item in requestHeaders)
                {
                    requestTelemetry.Properties.Add($"RequestHeader_{item.Key}", item.Value);
                }
            }

            if (requestParameters != null)
            {
                foreach (var item in requestParameters)
                {
                    requestTelemetry.Properties.Add($"RequestParameter_{item.Key}", item.Value);
                }
            }
            requestTelemetry.Properties.Add("RequestBody", requestBody);
            if (responseHeaders != null)
            {
                foreach (var item in responseHeaders)
                {
                    requestTelemetry.Properties.Add($"ResponseHeader_{item.Key}", item.Value);
                }
            }
            requestTelemetry.Properties.Add("ResponseBody", responseBody);
            requestTelemetry.Properties.Add("CorrelationId", MultibannerContext.Current.CorrelationId);

            _telemetry.Context.Operation.Id = MultibannerContext.Current.CorrelationId;
            _telemetry.TrackRequest(requestTelemetry);
        }

        public async void LogException(Exception exception)
        {
            _telemetry.Context.Operation.Id = MultibannerContext.Current.CorrelationId;
            _telemetry.TrackException(exception);
            
        }
    }
}
