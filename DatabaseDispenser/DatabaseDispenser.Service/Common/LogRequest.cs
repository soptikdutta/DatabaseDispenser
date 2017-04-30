using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace DatabaseDispenser.Service.Common
{
    public class LogRequest : ActionFilterAttribute
    {
        private Stopwatch _stopwatch;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _stopwatch = Stopwatch.StartNew();

            MultibannerContext.SetContext(actionContext.Request);
            var context = new MultibannerContext
            {
                CorrelationId = MultibannerContext.Current.CorrelationId,
            };
            //log request in to application insights
            //CommonFunctions.LogRequest(actionContext);
            string requestPath =
                actionContext.ControllerContext.ControllerDescriptor.ControllerName + "_" +
                actionContext.ActionDescriptor.ActionName;
            CommonFunctions.LogMetrics(requestPath + " started");
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //actionExecutedContext.Response.Headers.Add("CorrelationId",
                //actionExecutedContext.Request.GetCorrelationId().ToString());
            if (actionExecutedContext.Exception == null)
            {
                CommonFunctions.LogResponse(actionExecutedContext);
            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
