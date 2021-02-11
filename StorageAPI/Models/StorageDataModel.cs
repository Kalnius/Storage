using System;
using System.Collections.Generic;

namespace StorageAPI.Models
{
    public class StorageDataModel
    {
        public List<object> Records { get; set; }
        public TimeSpan ExpirationTime { get; set; }
        public DateTime ExpiresAt { get; set; }

        public StorageDataModel(List<object> records, TimeSpan expirationTime, DateTime expiresAt)
        {
            Records = records;
            ExpirationTime = expirationTime;
            ExpiresAt = expiresAt;
        }
    }
}
