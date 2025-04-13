namespace Kairou
{
    public class IntStringConverter : TypeConverter<int, string>
    {
        public override string Convert(int input)
        {
            return input.ToString();
        }
    }
}