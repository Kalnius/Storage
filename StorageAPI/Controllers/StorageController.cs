using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageAPI.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace StorageAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private readonly TimeSpan defaultExpiresAfter;
        private readonly TimeSpan maxExpiration;

        public StorageController(IConfiguration configuration, ILogger<StorageController> logger)
        {
            _logger = logger;
            defaultExpiresAfter = configuration.GetValue<TimeSpan>("DefaultExpiration");
            maxExpiration = configuration.GetValue<TimeSpan>("MaxExpiration");
        }

        [HttpPost, Route("create"), SwaggerOperation("Create a new record in storage")]
        public void Create(string key, TimeSpan? expiresAfter, [FromBody] List<object> recordItems)
        {
            if (expiresAfter > maxExpiration)
            {
                throw new ArgumentException($"Expiration exceeds maximum allowed length. " +
                    $"Maximum expiration length is {maxExpiration}");
            }

            var value = new StorageDataModel
            (
                recordItems, 
                expiresAfter ?? defaultExpiresAfter, 
                DateTime.Now.Add(expiresAfter ?? defaultExpiresAfter)
            );
            
            if (Storage.Data.ContainsKey(key))
            {
                Storage.Data[key] = value;
            }
            else Storage.Data.Add(key, value);
            _logger.Log(LogLevel.Information, $"Record \"{key}\" created. " +
                $"Record item(s): \"{JsonSerializer.Serialize(recordItems)}\"");
        }

        [HttpPut, Route("append"), SwaggerOperation("Add a record item to existing record")]
        public void Append(string key, [FromBody] object recordItem)
        {
            if (!Storage.Data.ContainsKey(key))
            {
                Storage.Data.Add(key, 
                    new StorageDataModel
                    (
                        new List<object> { recordItem },
                        defaultExpiresAfter,
                        DateTime.Now.Add(defaultExpiresAfter)
                    )
                );
            }
            else Storage.Data[key].Records.Add(recordItem);
            _logger.Log(LogLevel.Information, 
                $"Record item \"{JsonSerializer.Serialize(recordItem)}\" added to record \"{key}\"");
        }

        [HttpPut, Route("delete"), 
            SwaggerResponse(204, Type = typeof(NoContentResult), Description = "No records")]
        public void Delete(string key)
        {
            try
            {
                Storage.Data.Remove(key);
            }
            catch
            {
                _logger.Log(LogLevel.Error, $"Key \"{key}\" was not found in the storage.");
            }
        }


        [HttpGet, Route("get"), 
            SwaggerResponse(204, Type = typeof(NoContentResult), Description = "No records")]
        public List<object> Get(string key)
        {
            if (Storage.Data.ContainsKey(key))
            {
                var storageItem = Storage.Data[key];
                storageItem.ExpiresAt = DateTime.Now.Add(storageItem.ExpirationTime);
                return Storage.Data[key].Records;
            }
            _logger.Log(LogLevel.Error, $"Key \"{key}\" was not found in the storage.");
            return null;
        }
    }
}
