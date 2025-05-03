using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul
{
    /// <summary>
    /// IMemoryCache için extension metotlar
    /// </summary>
    public static class MemoryCacheExtensions
    {
        /// <summary>
        /// IMemoryCache içerisindeki tüm anahtarları döndürür
        /// </summary>
        /// <param name="memoryCache">IMemoryCache nesnesi</param>
        /// <returns>Anahtarlar listesi</returns>
        public static List<object> GetKeys(this IMemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(memoryCache) as dynamic;
            var keys = new List<object>();
            
            foreach (var item in collection)
            {
                var methodInfo = item.GetType().GetProperty("Key");
                var key = methodInfo.GetValue(item);
                keys.Add(key);
            }
            
            return keys;
        }
    }
} 