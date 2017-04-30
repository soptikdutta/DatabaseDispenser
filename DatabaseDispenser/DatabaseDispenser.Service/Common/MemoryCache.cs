using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    public class MemoryCache : ICache
    {
        private readonly object _lockObject = new object();

        public string KeyPrefix
        {
            get { return "Tjx-MultibannerBackend"; }
        }

        public bool TurnOffCaching
        {
            get
            {
                return false;
            }
        }

        public int CacheExpirationInHours
        {
            get { return 2400; }
        }

        public string FullKey(string key)
        {
            if (!string.IsNullOrEmpty(KeyPrefix))
                return KeyPrefix + "." + key;
            return key;
        }

        public async Task AddOrUpdate(object cacheObject, string key)
        {
            if (System.Runtime.Caching.MemoryCache.Default == null)
                throw new Exception("Memory cache is not set up");
            lock (_lockObject)
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(CacheExpirationInHours)
                };

                if (System.Runtime.Caching.MemoryCache.Default[FullKey(key)] == null)
                {
                    System.Runtime.Caching.MemoryCache.Default.Add(new CacheItem(FullKey(key), cacheObject),
                        cacheItemPolicy);
                }
                else
                {
                    System.Runtime.Caching.MemoryCache.Default.Set((new CacheItem(FullKey(key), cacheObject)), cacheItemPolicy);
                }
            }

        }

        public async Task Remove(string key)
        {
            System.Runtime.Caching.MemoryCache.Default.Remove(FullKey(key));
        }

        public async Task<bool> Exist(string key)
        {
            return System.Runtime.Caching.MemoryCache.Default.Contains(FullKey(key));
        }

        public async Task<IEnumerable<T>> Get<T>(string key) where T : class
        {
            if (System.Runtime.Caching.MemoryCache.Default == null)
                return null;

            lock (_lockObject)
            {
                return System.Runtime.Caching.MemoryCache.Default[FullKey(key)] == null
                    ? null
                    : System.Runtime.Caching.MemoryCache.Default[FullKey(key)] as IEnumerable<T>;
            }
        }

        public async Task<IEnumerable<T>> Get<T>(string key, Func<Task<IEnumerable<T>>> function) where T : class
        {
            if (function == null)
                return null;
            var obj = Get<T>(key);
            if (obj.Result == null)
            {
                lock (_lockObject)
                {
                    obj = Get<T>(key);
                    if (obj.Result == null)
                    {
                        obj = function();
                        AddOrUpdate(obj, key);
                    }
                }
            }

            return await obj;
        }

        public async Task Clear()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAllKeys()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetByKey(string key)
        {
            throw new NotImplementedException();
        }
    }
}
