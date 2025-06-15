using Kairou.DataStore;

namespace Kairou
{
    public class BoolVariableType : VariableType<bool>, IDataStorageAccessor<bool>
    {
        public override bool CanNegate => true;
        public override bool Negate(bool value) => !value;
        public override bool CanAdd => true;
        public override bool Add(bool value1, bool value2) => value1 || value2;
        public override bool CanMultiply => true;
        public override bool Multiply(bool value1, bool value2) => value1 && value2;

        public bool TryGet(IDataStorage storage, string key, out bool value)
        {
            return storage.TryGet(key, out value);
        }

        public void Set(IDataStorage storage, string key, bool value)
        {
            storage.Set(key, value);
        }
    }
}