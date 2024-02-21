using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Simplifier.Core.Cache;

public class RedisHelper
{
    private readonly ILogger<RedisHelper> _logger;
    private readonly IDatabase _db;
    private readonly ConnectionMultiplexer _connection; // Add this line

    public RedisHelper(
        ILogger<RedisHelper> logger,
        ConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connection = connectionMultiplexer; // Add this line

        var redisDatabase = 0;

        try
        {
            var redisDatabaseEnvVar = Environment.GetEnvironmentVariable("REDIS_DATABASE") ?? "0";
            redisDatabase = Convert.ToInt32(redisDatabaseEnvVar);
        }
        catch (Exception)
        {
            // ignored
        }

        _db = connectionMultiplexer.GetDatabase(redisDatabase);
    }

    public T GetObject<T>(string key)
    {
        try
        {
            string json = _db.StringGet(key);
            return string.IsNullOrWhiteSpace(json) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }
        catch (RedisTimeoutException ex)
        {
            var errorMessage = $"GetObject timed out for key {key}";
            _logger.LogWarning(errorMessage);
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"GetObject for key {key} failed with exception: {ex}");
            return default(T);
        }
    }

    public void SetObject<T>(string key, T @object, TimeSpan? expiration = null)
    {
        if (@object is null)
            return;

        try
        {
            string json = JsonConvert.SerializeObject(@object);
            _db.StringSet(key, json, expiration);
        }
        catch (RedisTimeoutException ex)
        {
            var errorMessage = $"SetObject timed out for key {key}";
            _logger.LogWarning(errorMessage);
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"SetObject for key {key} failed with exception: {ex}");
        }
    }

    public void DeleteKey(string key)
    {
        try
        {
            _db.KeyDelete(key);
            _logger.LogInformation($"DELETE finished for key {key}");
        }
        catch (RedisTimeoutException ex)
        {
            var errorMessage = $"DELETE timed out for key {key} with exception: {ex}";
            _logger.LogWarning(errorMessage);
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"DELETE for key {key} failed with exception: {ex}");
        }
    }

    public void DeleteKeysByPattern(string pattern)
    {
        try
        {
            _logger.LogInformation($"DeleteKeysByPattern called for key {pattern}");
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var foundKeys = server.Keys(pattern: pattern);

            foreach (var key in foundKeys)
            {
                _db.KeyDelete(key);
            }

            _logger.LogInformation($"DeleteKeysByPattern finished for key {pattern}");
        }
        catch (RedisTimeoutException ex)
        {
            var errorMessage = $"DeleteKeysByPattern timed out for key {pattern}";
            _logger.LogInformation(errorMessage);
            _logger.LogWarning(errorMessage);
            throw ex;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"DeleteKeysByPattern for pattern {pattern} failed with exception: {ex}");
        }
    }
}
