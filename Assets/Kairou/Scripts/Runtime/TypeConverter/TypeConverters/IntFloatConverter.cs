namespace Kairou
{
    public class IntFloatConverter : TypeConverter<int, float>
    {
        public override float Convert(int fromValue)
        {
            return fromValue;
        }
    }
}