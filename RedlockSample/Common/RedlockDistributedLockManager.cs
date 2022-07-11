using StackExchange.Redis;

namespace RedlockSample.Common
{
    public class RedlockDistributedLockManager : IDistributedLockManager
    {
        private readonly Redlock.CSharp.Redlock _dlm;
        private readonly IConfiguration _config;
        public RedlockDistributedLockManager(Redlock.CSharp.Redlock dlm, IConfiguration config)
        {
            this._dlm = dlm;
            this._config = config;
        }

        public async Task<bool> Lock(string key, int ttlByMinute)
        {
            Redlock.CSharp.Lock lockObject = null;
            return await Task.Run(() => { return this._dlm.Lock(key, TimeSpan.FromMinutes(ttlByMinute), out lockObject); });
        }

        public async Task UnLock(string key)
        {
            var configOption = new ConfigurationOptions
            {
                EndPoints =
                {
                     { this._config["redis:host"],Convert.ToInt32(this._config["redis:port"]) }
                },
                Password = this._config["redis:password"],
                DefaultDatabase = Convert.ToInt32(this._config["redis:defaultDb"])
            };

            RedisValue redLockValue;
            using (var redis = ConnectionMultiplexer.Connect(configOption.ToString()))
            {
                redLockValue = await redis.GetDatabase().StringGetAsync(key);
            }

            this._dlm.Unlock(new Redlock.CSharp.Lock(key, redLockValue, default));
        }
    }
}
