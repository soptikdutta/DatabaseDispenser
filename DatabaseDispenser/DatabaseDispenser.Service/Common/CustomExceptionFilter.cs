using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace DatabaseDispenser.Service.Common
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        #region Properties
        /// <summary>
        /// If true,it will display the stacktrace along with response
        /// </summary>
        private static bool IsDebug { get; set; }
        #endregion

        #region UnhandledExceptionFilterAttribute()

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExceptionFilter"/> class.
        /// </summary>
        public CustomExceptionFilter()
        {
        }

        public CustomExceptionFilter(bool isDebug)
        {
            IsDebug = isDebug;
        }

        #endregion UnhandledExceptionFilterAttribute()

        #region DefaultHandler

        /// <summary>
        /// Gets a delegate method that returns an <see cref="HttpResponseMessage"/>
        /// that describes the supplied exception.
        /// </summary>
        /// <value>
        /// A <see cref="Func{T1,T2,TResult}"/> delegate method that returns
        /// an <see cref="HttpResponseMessage"/> that describes the supplied exception.
        /// </value>
        private static readonly Func<Exception, HttpRequestMessage, HttpResponseMessage> DefaultHandler = (exception, request) =>
        {
            if (exception == null)
            {
                return null;
            }

            var response = request.CreateResponse(
                HttpStatusCode.InternalServerError, GetContentOf(exception)
            );

            return response;
        };

        #endregion DefaultHandler

        #region GetContentOf

        /// <summary>
        /// Gets a delegate method that extracts information from the specified exception.
        /// </summary>
        /// <value>
        /// A <see cref="Func{Exception, String}"/> delegate method that extracts information
        /// from the specified exception.
        /// </value>
        private static readonly Func<Exception, string> GetContentOf = exception =>
        {
            if (exception == null)
            {
                return String.Empty;
            }

            var result = new StringBuilder();

            result.AppendLine(exception.Message);
            result.AppendLine();

            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                result.AppendLine(innerException.Message);
                result.AppendLine();
                innerException = innerException.InnerException;
            }

            if (IsDebug) result.AppendLine(exception.StackTrace);

            return result.ToString().TrimEnd(Environment.NewLine.ToCharArray()); ;
        };

        #endregion GetContentOf

        #region Handlers

        /// <summary>
        /// Gets the exception handlers registered with this filter.
        /// </summary>
        /// <value>
        /// A <see cref="ConcurrentDictionary{TKey,TValue}"/> collection that contains
        /// the exception handlers registered with this filter.
        /// </value>
        protected ConcurrentDictionary<Type, Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>>> Handlers
        {
            get
            {
                return _filterHandlers;
            }
        }

        
        private readonly ConcurrentDictionary<Type, Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>>> _filterHandlers = new ConcurrentDictionary<Type, Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>>>();
        private ILogger logger = null;
        #endregion Handlers

        #region OnException(HttpActionExecutedContext actionExecutedContext)

        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null || actionExecutedContext.Exception == null)
            {
                return;
            }

            MultibannerContext.SetContext(actionExecutedContext.Request);

            logger = new ApplicationInsightsLogger();
            logger.LogException(actionExecutedContext.Exception);

            var type = LookUpException(actionExecutedContext.Exception).GetType();

            Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>> registration;

            if (Handlers.TryGetValue(type, out registration))
            {
                var statusCode = registration.Item1;
                var handler = registration.Item2;

                var response = handler(
                    actionExecutedContext.Exception.GetBaseException(),
                    actionExecutedContext.Request
                );

                // Use registered status code if available
                if (statusCode.HasValue)
                {
                    response.StatusCode = statusCode.Value;
                    //Comment this line if we want to show the error message in reason phrase also.
                    response.ReasonPhrase = statusCode.ToString().Replace(Environment.NewLine, String.Empty);
                }

                actionExecutedContext.Response = response;
            }
            else
            {
                // If no exception handler registered for the exception type, fallback to default handler
                actionExecutedContext.Response = DefaultHandler(
                    actionExecutedContext.Exception.GetBaseException(), actionExecutedContext.Request
                );
            }

            if (actionExecutedContext.Exception != null)
            {
                CommonFunctions.LogResponse(actionExecutedContext);
            }
            base.OnException(actionExecutedContext);
        }

        #endregion OnException(HttpActionExecutedContext actionExecutedContext)

        #region Register<TException>(HttpStatusCode statusCode)

        /// <summary>
        /// Registers an exception handler that returns the specified status code for exceptions of type <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of exception to register a handler for.</typeparam>
        /// <param name="statusCode">The HTTP status code to return for exceptions of type <typeparamref name="TException"/>.</param>
        /// <returns>
        /// This <see cref="CustomExceptionFilter"/> after the exception handler has been added.
        /// </returns>
        public CustomExceptionFilter Register<TException>(HttpStatusCode statusCode)
            where TException : Exception
        {
            var type = typeof(TException);
            var item = new Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>>(
                statusCode, DefaultHandler
            );

            if (!Handlers.TryAdd(type, item))
            {
                Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>> oldItem;

                if (Handlers.TryRemove(type, out oldItem))
                {
                    Handlers.TryAdd(type, item);
                }
            }

            return this;
        }

        #endregion Register<TException>(HttpStatusCode statusCode)

        #region Register<TException>(Func<Exception, HttpRequestMessage, HttpResponseMessage> handler)

        /// <summary>
        /// Registers the specified exception <paramref name="handler"/> for exceptions of type <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of exception to register the <paramref name="handler"/> for.</typeparam>
        /// <param name="handler">The exception handler responsible for exceptions of type <typeparamref name="TException"/>.</param>
        /// <returns>
        /// This <see cref="CustomExceptionFilter"/> after the exception <paramref name="handler"/>
        /// has been added.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is <see langword="null"/>.</exception>
        public CustomExceptionFilter Register<TException>(Func<Exception, HttpRequestMessage, HttpResponseMessage> handler)
            where TException : Exception
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            var type = typeof(TException);
            var item = new Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>>(
                null, handler
            );

            if (!Handlers.TryAdd(type, item))
            {
                Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>> oldItem;

                if (Handlers.TryRemove(type, out oldItem))
                {
                    Handlers.TryAdd(type, item);
                }
            }

            return this;
        }

        #endregion Register<TException>(Func<Exception, HttpRequestMessage, HttpResponseMessage> handler)

        #region Unregister<TException>()

        /// <summary>
        /// Unregisters the exception handler for exceptions of type <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of exception to unregister handlers for.</typeparam>
        /// <returns>
        /// This <see cref="CustomExceptionFilter"/> after the exception handler
        /// for exceptions of type <typeparamref name="TException"/> has been removed.
        /// </returns>
        public CustomExceptionFilter Unregister<TException>()
            where TException : Exception
        {
            Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>> item;

            Handlers.TryRemove(typeof(TException), out item);

            return this;
        }

        #endregion Unregister<TException>()

        #region Check for registered exceptions
        /// <summary>
        /// Check for registered exceptions
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>Exception</returns>
        private Exception LookUpException(Exception exception)
        {
            Exception e = exception;
            var type = exception.GetType();
            Tuple<HttpStatusCode?, Func<Exception, HttpRequestMessage, HttpResponseMessage>> registration;
            if (Handlers.TryGetValue(type, out registration))
            {
                return e;
            }
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                type = exception.GetType();
                if (Handlers.TryGetValue(type, out registration))
                {
                    e = exception;
                    break;
                }
            }
            return e;
        }
        #endregion
    }
}
