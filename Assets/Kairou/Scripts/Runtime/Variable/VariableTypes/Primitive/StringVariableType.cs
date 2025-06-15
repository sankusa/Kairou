using Kairou.DataStore;

namespace Kairou
{
    public class StringVariableType : VariableType<string>, IDataStorageAccessor<string>
    {
        public override bool CanAdd => true;
        public override string Add(string value1, string value2) => value1 + value2;

        public string Convert(string value)
        {
            return value;
        }

        public bool TryGet(IDataStorage storage, string key, out string value)
        {
            return storage.TryGet(key, out value);
        }

        public void Set(IDataStorage storage, string key, string value)
        {
            storage.Set(key, value);
        }
    }
}