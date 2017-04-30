using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    interface IDbOperations
    {
        Task<string> CreateDatabase(string databaseId);
        Task DeleteDatabase(string dbName);

        string DeleteAllDatabases();

        Task<IEnumerable<string>> GetAllDatabases();
        //IEnumerable<Database> GetDatabases();
    }
}
