using Kairou.DataStore;

namespace Kairou
{
    public class IntVariableType : VariableType<int>, IDataStorageAccessor<int>
    {
        public override bool CanNegate => true;
        public override int Negate(int value) => -value;
        public override bool CanAdd => true;
        public override int Add(int value1, int value2) => value1 + value2;
        public override bool CanSubtract => true;
        public override int Subtract(int value1, int value2) => value1 - value2;
        public override bool CanMultiply => true;
        public override int Multiply(int value1, int value2) => value1 * value2;
        public override bool CanDivide => true;
        public override int Divide(int value1, int value2) => value1 / value2;
        public override bool CanModulo => true;
        public override int Modulo(int value1, int value2) => ((value1 % value2) + value2) % value2;

        public bool TryGet(IDataStorage storage, string key, out int value)
        {
            return storage.TryGet(key, out value);
        }

        public void Set(IDataStorage storage, string key, int value)
        {
            storage.Set(key, value);
        }
    }
}