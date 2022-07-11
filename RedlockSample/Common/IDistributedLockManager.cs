namespace RedlockSample.Common
{
    public interface IDistributedLockManager
    {
        public Task<bool> Lock(string key, int ttlByMinute);

        public Task UnLock(string key);
    }
}
