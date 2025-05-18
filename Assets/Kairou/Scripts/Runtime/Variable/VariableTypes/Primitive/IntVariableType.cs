namespace Kairou
{
    public class IntVariableType : VariableType<int>
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
    }
}