using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Client.TransientFaultHandling.Strategies;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace DatabaseDispenser.Service.Common
{
    public class AzureDbOperations : IDbOperations
    {
        //private DocumentClient client { get; set; }
        private IReliableReadWriteDocumentClient documentClient { get; set; }
        private ILogger logger { get; set; }

        public AzureDbOperations()
        {
            //client = DocumentClientSingleton.Instance.GetClient(ServiceConfigSettings.DocumentDbEndpointUrl,
              //  ServiceConfigSettings.AuthKey);
            documentClient = DocumentClientSingleton.Instance.CreateClient(ServiceConfigSettings.DocumentDbEndpointUrl,
                ServiceConfigSettings.AuthKey);
            logger = new ApplicationInsightsLogger();
        }


        public async Task<string> CreateDatabase(string databaseId)
        {
            
            var result = documentClient.CreateDatabaseAsync(new Database {Id = databaseId.ToString()}).Result;
            if (result.StatusCode == HttpStatusCode.Created)
            {
                //CreateCollection(databaseId);
            }

            logger.LogInformation($"Created Database: {databaseId} on {DateTime.UtcNow}", null);

            return databaseId;

        }

        private void CreateCollection(string dbID)
        {
            List<Task> TaskList = new List<Task>();

            try
            {
                var whatever = documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "post"}).Result;

                whatever = documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "user"}).Result;
                whatever = documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "store"}).Result;
                whatever = documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "survey"}).Result;
                whatever =documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "surveyanswer"}).Result;
                whatever = documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "department"}).Result;
                whatever= documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "contact"}).Result;
                whatever=documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "content"}).Result;
                whatever=  documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "notification"}).Result;
                whatever= documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "storedvaluecards"}).Result;
                whatever=documentClient.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(dbID),
                    new DocumentCollection {Id = "settings"}).Result;

                //Task.WhenAll(TaskList.ToArray()).Wait();
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                Dictionary<string,string> properties = new Dictionary<string, string>();
                properties.Add("Exception", exception.Length >= 5000 ? exception.Substring(0, 4000) : exception);
                logger.LogInformation($"Failed collection on: {dbID} on {DateTime.UtcNow}", properties);
                throw ex;
            }

        }

        public string DeleteAllDatabases()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteDatabase(string dbName)
        {
            await documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(dbName));
            logger.LogInformation($"Deleted Database: { dbName } on { DateTime.UtcNow}", null);
        }

        public async Task<IEnumerable<string>> GetAllDatabases()
        {
            var database = documentClient.CreateDatabaseQuery();

            var databases = database.ToList();
            if (databases != null && databases.Any(x => x.Id != "the-goods"))
                return databases.ToList().Where(x => x.Id != "the-goods").Select(x => x.Id);

            return null;

        }
    
    }
}
