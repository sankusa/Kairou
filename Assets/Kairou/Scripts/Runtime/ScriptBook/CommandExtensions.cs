using System;
using System.Collections.Generic;
using System.Linq;

namespace Kairou
{
    public static class CommandExtensions
    {
        public static IEnumerable<string> ValidateVariableKey(this Command command, string fieldName, string variableName, TargetVariableScope targetScope, Func<VariableDefinition, string, IEnumerable<string>> validateFunc)
        {
            var page = command.ParentPage;
            var book = page?.ParentBook;
            if (page != null && (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Page))
            {
                var variable = page.Variables.Find(x => x.Name == variableName);
                if (variable != null)
                {
                    foreach(string errorMessage in validateFunc(variable, fieldName))
                    {
                        yield return errorMessage;
                    }
                    yield break;
                }
            }
            if (book != null && (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Book))
            {
                var variable = book.Variables.Find(x => x.Name == variableName);
                if (variable != null)
                {
                    foreach(string errorMessage in validateFunc(variable, fieldName))
                    {
                        yield return errorMessage;
                    }
                    yield break;
                }
            }
            yield return $"{fieldName} : Target Variable not found. VariableName: {variableName}";
        }
    }
}