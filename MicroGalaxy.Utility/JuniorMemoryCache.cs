using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MicroGalaxy.Utility
{
    /// <summary>
    /// 基于MemoryCache的缓存辅助类
    /// </summary>
    public static class JuniorMemoryCache
    {

        private static System.Collections.Concurrent.ConcurrentDictionary<string, bool> processFlag = new System.Collections.Concurrent.ConcurrentDictionary<string, bool>();

        private static readonly Object _locker = new object();

        public static T GetCacheItem<T>(String key)
        {
            if (!MemoryCache.Default.Contains(key) && processFlag.ContainsKey(key))
            {
                var outbool = false;
                processFlag.TryRemove(key, out outbool);
            }
            var start = DateTime.Now;
            while (!MemoryCache.Default.Contains(key))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"key等待...:{key}");
                System.Threading.Thread.Sleep(100);
#else
             System.Threading.Thread.Sleep(10);
#endif
                if ((DateTime.Now - start).TotalSeconds > 100)
                {
                    System.Diagnostics.Debug.WriteLine($"key等待超时:{key}");
                    return default(T);
                }
            }
            if (MemoryCache.Default.Contains(key))
            {
                return (T)MemoryCache.Default[key];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 添加\获取缓存内容
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="key">对象主键</param>
        /// <param name="populate">对象</param>
        /// <param name="slidingExpiration">过期时间差</param>
        /// <param name="absoluteExpiration">过期时间</param>
        /// <returns></returns>
        public static T GetCacheItem<T>(String key, T populate, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            return GetCacheItem(key, () => { return populate; }, slidingExpiration, absoluteExpiration);
        }
        /// <summary>
        /// 添加\获取\代理缓存内容
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="key">对象主键</param>
        /// <param name="proxyPopulate">对象代理</param>
        /// <param name="slidingExpiration">过期时间差</param>
        /// <param name="absoluteExpiration">过期时间</param>
        /// <returns></returns>
        public static T GetCacheItem<T>(String key, Func<T> proxyPopulate, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid cache key");
            if (proxyPopulate == null) throw new ArgumentNullException("cachePopulate");
            if (slidingExpiration == null && absoluteExpiration == null) slidingExpiration = new TimeSpan(2400, 0, 0);
            if (!MemoryCache.Default.Contains(key) && processFlag.ContainsKey(key))
            {
                var outbool = false;
                processFlag.TryRemove(key, out outbool);
            }
            if (processFlag.TryAdd(key, false))
            {
                //System.Diagnostics.Debug.WriteLine($"没有key:{key}");
                if (MemoryCache.Default[key] == null)
                {
                    lock (_locker)
                    {
                        if (MemoryCache.Default[key] == null)
                        {
                            var item = new CacheItem(key, proxyPopulate.Invoke());
#if DEBUG
                            if (key.Contains("timeline_"))
                            {
                                System.Diagnostics.Debug.WriteLine($"key需要执行:{key}");
                            }
#endif
                            var policy = CreatePolicy(slidingExpiration, absoluteExpiration);
                            if (item.Value == null)//null 值不缓存
                            {
                                return (T)item.Value;
                            }
                            if (typeof(T).IsGenericType || typeof(T).IsArray)//长度为0的 值不缓存
                            {
                                IEnumerable<dynamic> titem = item.Value as IEnumerable<dynamic>;
                                if (titem!=null && titem.Count()==0)
                                {
                                    return (T)item.Value;
                                }
                            }
                            MemoryCache.Default.Add(item, policy);
                        }
                        else
                        {
                            MemoryCache.Default.AddOrGetExisting(new CacheItem(key, MemoryCache.Default[key]), CreatePolicy(slidingExpiration, absoluteExpiration));
                        }
                        processFlag.TryUpdate(key, true, false);
                    }
                }
                processFlag[key] = true;
            }
            else
            {
                if (!processFlag.ContainsKey(key))
                {

                }
                else if (!processFlag[key])
                {
                    var start = DateTime.Now;
                    while (!processFlag[key])
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"key等待...:{key}");
                        System.Threading.Thread.Sleep(100);
#else 
                        System.Threading.Thread.Sleep(10);
#endif
                        if ((DateTime.Now - start).TotalSeconds > 10)
                        {
                            System.Diagnostics.Debug.WriteLine($"key等待超时:{key}");
                            return default(T);
                        }
                    }
                }
            }



            return (T)MemoryCache.Default[key];
        }

        private static CacheItemPolicy CreatePolicy(TimeSpan? slidingExpiration, DateTime? absoluteExpiration)
        {
            var policy = new CacheItemPolicy();

            if (absoluteExpiration.HasValue)
            {
                policy.AbsoluteExpiration = absoluteExpiration.Value;
            }
            else if (slidingExpiration.HasValue)
            {
                policy.SlidingExpiration = slidingExpiration.Value;
            }

            policy.Priority = CacheItemPriority.Default;

            return policy;
        }

        public static bool Contains(string key)
        {
            return MemoryCache.Default.Contains(key);
        }
    }
}