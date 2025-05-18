using UnityEngine;

namespace Kairou
{
    public class Vector2VariableType : VariableType<Vector2>
    {
        public override bool CanNegate => true;
        public override Vector2 Negate(Vector2 value) => -value;
        public override bool CanAdd => true;
        public override Vector2 Add(Vector2 value1, Vector2 value2) => value1 + value2;
        public override bool CanSubtract => true;
        public override Vector2 Subtract(Vector2 value1, Vector2 value2) => value1 - value2;
        public override bool CanMultiply => true;
        public override Vector2 Multiply(Vector2 value1, Vector2 value2) => value1 * value2;
        public override bool CanDivide => true;
        public override Vector2 Divide(Vector2 value1, Vector2 value2) => value1 / value2;
    }
}