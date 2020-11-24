namespace StressTest.App.Data
{
    public interface IRedisCacheProvider
    {
        void GetItem(string key);
        void SetItem(string key, string item);
    }
}