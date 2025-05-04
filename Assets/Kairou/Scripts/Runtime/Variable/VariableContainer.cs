using System.Collections.Generic;

namespace Kairou
{
    public class VariableContainer
    {
        readonly Dictionary<string, Variable> _variables = new();

        public bool TryGetValue(string name, out Variable variable)
        {
            return _variables.TryGetValue(name, out variable);
        }

        public void GenerateVariables(List<VariableDefinition> variables)
        {
            foreach (var variable in variables)
            {
                _variables[variable.Name] = variable.CreateVariable();
            }
        }

        public void Clear()
        {
            foreach (var variable in _variables.Values)
            {
                variable.ReturnToPool();
            }
            _variables.Clear();
        }
    }
}