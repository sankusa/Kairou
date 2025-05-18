namespace Kairou
{
    public class BoolVariableType : VariableType<bool>
    {
        public override bool CanNegate => true;
        public override bool Negate(bool value) => !value;
        public override bool CanAdd => true;
        public override bool Add(bool value1, bool value2) => value1 || value2;
        public override bool CanMultiply => true;
        public override bool Multiply(bool value1, bool value2) => value1 && value2;
    }
}