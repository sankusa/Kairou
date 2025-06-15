using Kairou.DataStore;
using UnityEngine;

namespace Kairou
{
    public class Vector3VariableType : VariableType<Vector3>, IDataStorageAccessor<Vector3>
    {
        public override bool CanNegate => true;
        public override Vector3 Negate(Vector3 value) => -value;
        public override bool CanAdd => true;
        public override Vector3 Add(Vector3 value1, Vector3 value2) => value1 + value2;
        public override bool CanSubtract => true;
        public override Vector3 Subtract(Vector3 value1, Vector3 value2) => value1 - value2;
        public override bool CanMultiply => true;
        public override Vector3 Multiply(Vector3 value1, Vector3 value2) => new Vector3(value1.x * value2.x, value1.y * value2.y, value1.z * value2.z);
        public override bool CanDivide => true;
        public override Vector3 Divide(Vector3 value1, Vector3 value2) => new Vector3(value1.x / value2.x, value1.y / value2.y, value1.z / value2.z);

        public bool TryGet(IDataStorage storage, string key, out Vector3 value)
        {
            return storage.TryGet(key, out value);
        }

        public void Set(IDataStorage storage, string key, Vector3 value)
        {
            storage.Set(key, value);
        }
    }
}