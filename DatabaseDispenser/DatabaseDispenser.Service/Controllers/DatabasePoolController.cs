using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DatabaseDispenser.Service.Common;

namespace DatabaseDispenser.Service.Controllers
{
    [ServiceRequestActionFilter]
    public class DatabasePoolController : ApiController
    {
        private readonly object _lockObject = new object();

        private ILogger logger { get; set; }


        private ICache cacheManager { get; set; }
        private IDbOperations dbOperations { get; set; }
        

        private static IEnumerable<string> OpenList { get; set; }
        private static IEnumerable<string> UsedList { get; set; }
        private static IEnumerable<string> BeingCreatedList { get; set; }

        public DatabasePoolController()
        {
            cacheManager = new MemoryCache();
            logger = new ApplicationInsightsLogger();

            if (!cacheManager.Exist(DatabaseType.Used.GetEnumDescription()).Result)
                cacheManager.AddOrUpdate(new List<string>(), DatabaseType.Used.GetEnumDescription());
            if (!cacheManager.Exist(DatabaseType.Open.GetEnumDescription()).Result)
                cacheManager.AddOrUpdate(new List<string>(), DatabaseType.Open.GetEnumDescription());
            if (!cacheManager.Exist(DatabaseType.BeingCreated.GetEnumDescription()).Result)
                cacheManager.AddOrUpdate(new List<string>(), DatabaseType.BeingCreated.GetEnumDescription());

            dbOperations = new AzureDbOperations();
        }

        #region Cache Update Methods

        private void Add(string databaseId, DatabaseType databaseType)
        {
            logger.LogInformation($"Adding {databaseId} to {databaseType.GetEnumDescription()}", null);
            var databases = cacheManager.Get<string>(databaseType.GetEnumDescription()).Result;
            List<string> databasesList = null;
            if (databases != null && databases.Any())
            {
                databasesList = databases.ToList();
                databasesList.Add(databaseId.ToLowerInvariant());
            }
            else
            {
                databasesList = new List<string>();
                databasesList.Add(databaseId.ToLowerInvariant());
            }
            cacheManager.AddOrUpdate(databasesList, databaseType.GetEnumDescription());
        }

        private void Remove(string databaseId, DatabaseType databaseType)
        {
            logger.LogInformation($"Removing {databaseId} from {databaseType.GetEnumDescription()}", null);
            var databases = cacheManager.Get<string>(databaseType.GetEnumDescription()).Result;
            List<string> databasesList = null;
            if (databases != null && databases.Any() && databases.Contains(databaseId.ToLowerInvariant()))
            {
                databasesList = databases.ToList();
                databasesList.Remove(databaseId.ToLowerInvariant());
                cacheManager.AddOrUpdate(databasesList, databaseType.GetEnumDescription());
            }
        }

        private bool Exists(string key, DatabaseType type)
        {
            var databases = cacheManager.Get<string>(type.GetEnumDescription()).Result;
            List<string> databasesList = null;
            if (databases != null && databases.Any() && databases.Contains(key.ToLowerInvariant()))
            {
                return true;
            }
            return false;
        }
        

        private int TotalUsedDatabase
        {
            get
            {
                var usedDatabases = cacheManager.Get<string>(DatabaseType.Used.GetEnumDescription()).Result;
                int count = 0;
                if (usedDatabases != null && usedDatabases.Any())
                    count = count + usedDatabases.Count();
                return count;
            }
        }

        private int TotalAvailableDatabase
        {
            get
            {
                var openDatabases = cacheManager.Get<string>(DatabaseType.Open.GetEnumDescription()).Result;
                int count = 0;
                if (openDatabases != null && openDatabases.Any())
                    count = count + openDatabases.Count();
                return count;
            }
        }

        #endregion


        // GET api/values 
        [HttpPost]
        [Route("create", Name = "Create")]
        public async Task<string> CreateDatabase()
        {
            string databaseId = string.Empty;
            Dictionary<string, string> properties = new Dictionary<string, string>();

            OpenList = cacheManager.Get<string>(DatabaseType.Open.GetEnumDescription()).Result;
            UsedList = cacheManager.Get<string>(DatabaseType.Used.GetEnumDescription()).Result;
            BeingCreatedList = cacheManager.Get<string>(DatabaseType.BeingCreated.GetEnumDescription()).Result;

            if (TotalAvailableDatabase > 0)
            {
                CommonFunctions.LogMetrics($"Returning from cache");
                databaseId = OpenList.FirstOrDefault();
                Remove(databaseId, DatabaseType.Open);
            }
            else if (TotalAvailableDatabase + TotalUsedDatabase >= ServiceConfigSettings.MaximumDatabases)
            {
                CommonFunctions.LogMetrics($"Max count of databases reached");
                throw new Exception("Please wait as the system has reached maximum number of databases");
            }
            else
            {
                CommonFunctions.LogMetrics($"Creating physical database");
                string dbID = "the-goods-mobile-" + Guid.NewGuid().ToString();
                Add(databaseId, DatabaseType.BeingCreated);
                databaseId = dbOperations.CreateDatabase(dbID).Result;
                Remove(databaseId, DatabaseType.BeingCreated);
            }

            Add(databaseId, DatabaseType.Used);
            return databaseId;
        }

        [HttpDelete]
        [Route("{id}", Name = "Delete")]
        public async Task ReleaseDatabase(string id)
        {
            Remove(id, DatabaseType.Used);
            Add(id, DatabaseType.Open);
        }

        [HttpPut]
        [Route("maintain", Name = "Maintain")]
        public async Task Maintain()
        {

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                List<Task> unnecessaryDataDeletionTasks = new List<Task>();
                var databases = dbOperations.GetAllDatabases().Result;
                if (databases != null && databases.Any())
                {
                    foreach (string database in databases)
                    {
                        if (!Exists(database, DatabaseType.Open) && !Exists(database, DatabaseType.Used) &&
                            !Exists(database, DatabaseType.BeingCreated))
                        {
                            unnecessaryDataDeletionTasks.Add(dbOperations.DeleteDatabase(database));
                        }
                    }
                }
                Task.WhenAll(unnecessaryDataDeletionTasks).Wait();

                var validDatabases = dbOperations.GetAllDatabases().Result;
                List<Task> validDatabasesDeletionTasks = new List<Task>();
                int validDatabasesCount = validDatabases == null ? 0 : validDatabases.Count();
                if (validDatabases != null && validDatabases.Any() &&
                    validDatabasesCount > ServiceConfigSettings.MinimumDatabases
                    && TotalAvailableDatabase > ServiceConfigSettings.MinimumDatabases)
                {
                    foreach (string validDatabaseId in validDatabases)
                    {
                        while (validDatabasesCount >= ServiceConfigSettings.MinimumDatabases
                               && TotalAvailableDatabase >= ServiceConfigSettings.MinimumDatabases)
                        {
                            if (Exists(validDatabaseId,DatabaseType.Open))
                            {
                                Remove(validDatabaseId, DatabaseType.Open);
                                validDatabasesDeletionTasks.Add(dbOperations.DeleteDatabase(validDatabaseId));
                                validDatabasesCount--;

                            }
                        }
                    }
                    Task.WhenAll(validDatabasesDeletionTasks).Wait();

                }

                else if (validDatabases == null || !validDatabases.Any() || TotalAvailableDatabase <= ServiceConfigSettings.MinimumDatabases)
                {
                    while (TotalAvailableDatabase <= ServiceConfigSettings.MinimumDatabases)
                    {
                        string dbID = "the-goods-mobile-" + Guid.NewGuid().ToString();
                        Add(dbID, DatabaseType.BeingCreated);
                        var databaseIdNew = dbOperations.CreateDatabase(dbID);
                        Remove(dbID, DatabaseType.BeingCreated);
                        Add(databaseIdNew.Result, DatabaseType.Open);
                    }
                }

            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        }
        
    }
}
