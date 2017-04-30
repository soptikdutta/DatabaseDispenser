using System;
using System.Net;
using System.Web.Http;
using DatabaseDispenser.Service.Common;
using Owin;

namespace DatabaseDispenser.Service
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Filters.Add(new LogRequest());
            RegisterExceptionFilter(config);

            config.MapHttpAttributeRoutes();
            appBuilder.UseWebApi(config);
        }

        public static void RegisterExceptionFilter(HttpConfiguration config)
        {
            //Registering exception filters
            config.Filters.Add(new CustomExceptionFilter(true)
                    .Register<UnauthorizedAccessException>(HttpStatusCode.Unauthorized)
                    .Register<Exception>(HttpStatusCode.ServiceUnavailable)

            );
        }
    }
}
