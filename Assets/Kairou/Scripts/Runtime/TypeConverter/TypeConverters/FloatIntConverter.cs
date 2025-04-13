namespace Kairou
{
    public class FloatIntConverter : TypeConverter<float, int>
    {
        public override int Convert(float input)
        {
            return (int)input;
        }
    }
}