namespace Kairou
{
    public class FloatVariableType : VariableType<float>
    {
        public override bool CanNegate => true;
        public override float Negate(float value) => -value;
        public override bool CanAdd => true;
        public override float Add(float value1, float value2) => value1 + value2;
        public override bool CanSubtract => true;
        public override float Subtract(float value1, float value2) => value1 - value2;
        public override bool CanMultiply => true;
        public override float Multiply(float value1, float value2) => value1 * value2;
        public override bool CanDivide => true;
        public override float Divide(float value1, float value2) => value1 / value2;
        public override bool CanModulo => base.CanModulo;
        public override float Modulo(float value1, float value2) => ((value1 % value2) + value2) % value2;
    }
}