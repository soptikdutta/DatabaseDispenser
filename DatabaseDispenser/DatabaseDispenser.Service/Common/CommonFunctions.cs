using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DatabaseDispenser.Service.Common
{
    public static class CommonFunctions
    {
        private static readonly ConfigurationPackage ConfigurationPackage = FabricRuntime.GetActivationContext().GetConfigurationPackageObject("config");
        private static readonly ILogger Logger = null;

        static CommonFunctions()
        {
            Logger = new ApplicationInsightsLogger();
        }

        public static string GetAppKey(string section, string key, string defaultValue = "")
        {
            if (ConfigurationPackage.Settings.Sections.Contains(section) && ConfigurationPackage.Settings.Sections[section].Parameters.Contains(key))
                return ConfigurationPackage.Settings.Sections[section].Parameters[key].Value;

            return defaultValue;
        }

        public static int GenerateId()
        {
            return RandomInteger(0, int.MaxValue);
        }

        // Return a random integer between a min and max value.
        public static int RandomInteger(int min, int max)
        {
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            uint scale = uint.MaxValue;
            while (scale == uint.MaxValue)
            {
                // Get four random bytes.
                byte[] fourBytes = new byte[4];
                rand.GetBytes(fourBytes);

                // Convert that into an uint.
                scale = BitConverter.ToUInt32(fourBytes, 0);
            }

            // Add min to the scaled difference between max and min.
            return (int)(min + (max - min) *
                (scale / (double)uint.MaxValue));
        }

        public static string GetEnumDescription(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return value.ToString();
        }

        public static void LogRequest(HttpActionContext actionContext)
        {
            //Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

            //string requestPath =
            //    actionContext.ControllerContext.ControllerDescriptor.ControllerName + "_" +
            //    actionContext.ActionDescriptor.ActionName;
            
            //Logger.LogServiceCall(requestPath, actionContext.Request?.RequestUri,
            //        actionContext.Request?.Content?.ReadAsStringAsync().Result, actionContext.Request.Method.ToString(),
            //        requestHeaders);
            
            // Logger.LogMetrics("MultibannerBackendV5");

        }

        public static void LogResponse(HttpActionExecutedContext actionExecutedContext)
        {
            MultibannerContext.SetContext(actionExecutedContext.Request);

            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.Headers.Add("Returned-By", "Tjx.Multibanner");
                actionExecutedContext.Response.Headers.Add("Correlation-Id",
                    actionExecutedContext.Request.GetCorrelationId().ToString());
            }

            string requestPath =
                actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName + "_" +
                actionExecutedContext.ActionContext.ActionDescriptor.ActionName;

            Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
            if (actionExecutedContext.Request.Headers.Any())
            {
                foreach (var item in actionExecutedContext.Request.Headers)
                {
                    requestHeaders.Add(item.Key, string.Join(",", item.Value));
                }
            }

            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.Headers.Any())
            {
                foreach (var item in actionExecutedContext.Response.Headers)
                {
                    responseHeaders.Add(item.Key, string.Join(",", item.Value));
                }
            }
            
            Logger.LogRequest(requestPath, actionExecutedContext.Request.Method,
                actionExecutedContext.Request.RequestUri, 
                actionExecutedContext.Response?.StatusCode.ToString() ?? "Unknown",
                actionExecutedContext.Response?.IsSuccessStatusCode ?? false, requestHeaders, null,
                actionExecutedContext.Request.Content.ReadAsStringAsync().Result,
                responseHeaders,
                (actionExecutedContext.Response != null && actionExecutedContext.Response.Content != null)
                    ? actionExecutedContext.Response.Content.ReadAsStringAsync().Result
                    : string.Empty);
            
        }

        public static void LogMetrics(string message)
        {
            ICache cacheManager = new MemoryCache();
            var OpenList = cacheManager.Get<string>(DatabaseType.Open.GetEnumDescription()).Result;
            var UsedList = cacheManager.Get<string>(DatabaseType.Used.GetEnumDescription()).Result;
            var BeingCreatedList = cacheManager.Get<string>(DatabaseType.BeingCreated.GetEnumDescription()).Result;

            Dictionary<string, string> properties = new Dictionary<string, string>();

            if (OpenList != null && OpenList.Any())
            {
                properties.Add("OpenList", string.Join(",", OpenList));
                properties.Add("OpenListCount", string.Join(",", OpenList.Count()));
            }
            if (UsedList != null && UsedList.Any())
            {
                properties.Add("UsedList", string.Join(",", UsedList));
                properties.Add("UsedListCount", string.Join(",", UsedList.Count()));
            }
            if (BeingCreatedList != null && BeingCreatedList.Any())
            {
                properties.Add("BeingCreatedList", string.Join(",", BeingCreatedList));
                properties.Add("BeingCreatedCount", string.Join(",", BeingCreatedList.Count()));
            }
            Logger.LogInformation(message, properties);

        }

    }
}
