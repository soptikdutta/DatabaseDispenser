using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    public class MultibannerContext
    {
        public static class Current
        {
            public static string CorrelationId
            {
                get
                {
                    if (!Equals(CallContext.LogicalGetData(ContextKeys.CorrelationId.ToString()), null))
                        return CallContext.LogicalGetData(ContextKeys.CorrelationId.ToString()).ToString();
                    return string.Empty;
                }
            }
        }

        public MultibannerContext()
        {
            
            if (CallContext.LogicalGetData(ContextKeys.CorrelationId.ToString()) != null)
                CorrelationId = CallContext.LogicalGetData(ContextKeys.CorrelationId.ToString()).ToString();
        }

        public static void SetContext(HttpRequestMessage request)
        {
            GenericPrincipal p = Thread.CurrentPrincipal as GenericPrincipal;
            
            CallContext.LogicalSetData(ContextKeys.CorrelationId.ToString(), request.GetCorrelationId().ToString());
            
        }

        public static void SetContextByCorrelationId(string correlationId)
        {
            CallContext.LogicalSetData(ContextKeys.CorrelationId.ToString(), correlationId);
        }
        public void SetContext([CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "")
        {
            CallContext.LogicalSetData(ContextKeys.CorrelationId.ToString(), CorrelationId);
            
        }
        
        public string CorrelationId { get; set; }
    }
}
