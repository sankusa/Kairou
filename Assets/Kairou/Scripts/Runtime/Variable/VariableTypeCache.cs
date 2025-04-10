namespace Kairou
{
    public static class VariableTypeCache<T>
    {
        static readonly VariableType<T> _variableType;
        public static VariableType<T> VariableType => _variableType;

        static VariableTypeCache()
        {
            _variableType = VariableTypeDictionary.Dic[typeof(T)] as VariableType<T>;
        }
    }
}