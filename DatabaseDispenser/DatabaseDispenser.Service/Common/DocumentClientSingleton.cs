using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Client.TransientFaultHandling.Strategies;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace DatabaseDispenser.Service.Common
{
    public sealed class DocumentClientSingleton
    {

        private DocumentClient _client;

        private static readonly Lazy<DocumentClientSingleton> Lazy =
             new Lazy<DocumentClientSingleton>(() => new DocumentClientSingleton());

        public static DocumentClientSingleton Instance { get { return Lazy.Value; } }

        private DocumentClientSingleton()
        {
        }

        /*public DocumentClient GetClient()
        {
            string endpoint = ServiceConfigSettings.DocumentDbEndpoint; 
            string accessKey = ServiceConfigSettings.DocumentDbAuthKey; 
            return GetClient(endpoint, accessKey);
        }*/

        public DocumentClient GetClient(string endpoint, string accessKey)
        {
            if (_client != null)
            {
                return _client;
            }

            _client = new DocumentClient(new Uri(endpoint), accessKey, new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,//.Gateway, - Temp for TJX network
                ConnectionProtocol = Protocol.Tcp
            });

            return _client;
        }

        public IReliableReadWriteDocumentClient CreateClient(string uri, string key)
        {
            ConnectionPolicy policy = new ConnectionPolicy()
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            var documentClient = new DocumentClient(new Uri(uri), key, policy);
            var documentRetryStrategy = new DocumentDbRetryStrategy(RetryStrategy.DefaultExponential) { FastFirstRetry = true };
            return documentClient.AsReliable(documentRetryStrategy);
        }
    }
}
