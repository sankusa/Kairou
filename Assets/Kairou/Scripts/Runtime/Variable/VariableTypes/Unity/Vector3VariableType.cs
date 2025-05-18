using UnityEngine;

namespace Kairou
{
    public class Vector3VariableType : VariableType<Vector3>
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
    }
}