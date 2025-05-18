namespace Kairou
{
    public class StringVariableType : VariableType<string>
    {
        public override bool CanAdd => true;
        public override string Add(string value1, string value2) => value1 + value2;
    }
}