using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    public interface ICache
    {
        string KeyPrefix { get; }

        int CacheExpirationInHours { get; }

        bool TurnOffCaching { get; }

        Task AddOrUpdate(object cacheObject, string key);

        Task Remove(string key);

        Task<bool> Exist(string key);

        Task<IEnumerable<T>> Get<T>(string key) where T : class;

        Task<IEnumerable<T>> Get<T>(string key, Func<Task<IEnumerable<T>>> function) where T : class;

        Task Clear();

        Task<IEnumerable<string>> GetAllKeys();

        Task<string> GetByKey(string key);
    }
}
