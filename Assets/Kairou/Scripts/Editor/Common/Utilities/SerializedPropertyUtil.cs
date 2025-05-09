namespace Kairou
{
    public static class SerializedPropertyUtil
    {
        public static string ConvertToReflectionPath(string propertyPath)
        {
            return propertyPath.Replace(".Array.data[", "[");
        }
    }
}