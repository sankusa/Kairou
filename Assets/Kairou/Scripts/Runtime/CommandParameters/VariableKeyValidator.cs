using System;
using System.Collections.Generic;

namespace Kairou
{
    public static class VariableKeySharedLogic
    {
        public static string GetSummary(string variableName) => $"[{variableName}]";

        public static IEnumerable<string> Validate(Command command, string fieldName, string variableName, TargetVariableScope targetScope, Func<VariableDefinition, string, IEnumerable<string>> validateFunc)
        {
            var page = command.ParentPage;
            var book = page?.ParentBook;
            if (string.IsNullOrEmpty(variableName))
            {
                yield return $"{fieldName} : VariableName is empty";
                yield break;
            }
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