using UnityEngine;

namespace Kairou
{
    public class Vector4VariableType : VariableType<Vector4>
    {
        public override bool CanNegate => true;
        public override Vector4 Negate(Vector4 value) => -value;
        public override bool CanAdd => true;
        public override Vector4 Add(Vector4 value1, Vector4 value2) => value1 + value2;
        public override bool CanSubtract => true;
        public override Vector4 Subtract(Vector4 value1, Vector4 value2) => value1 - value2;
        public override bool CanMultiply => true;
        public override Vector4 Multiply(Vector4 value1, Vector4 value2) => new Vector4(value1.x * value2.x, value1.y * value2.y, value1.z * value2.z, value1.w * value2.w);
        public override bool CanDivide => true;
        public override Vector4 Divide(Vector4 value1, Vector4 value2) => new Vector4(value1.x / value2.x, value1.y / value2.y, value1.z / value2.z, value1.w / value2.w);
    }
}