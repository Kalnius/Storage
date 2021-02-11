using StorageAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorageAPI
{
    public static class Storage
    {
        public static readonly Dictionary<string, StorageDataModel> Data = new Dictionary<string, StorageDataModel>();

        public static void InitCleanup(TimeSpan cleanupTimespan)
        {
            var timer = new System.Threading.Timer((e) =>
            {
                Data.ToList().ForEach(a => {
                    if (a.Value.ExpiresAt < DateTime.Now)
                    {
                        Data.Remove(a.Key);
                    }
                });
            }, null, TimeSpan.Zero, cleanupTimespan);
        }
    }
}
