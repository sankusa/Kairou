using System;
using UnityEditor;
using UnityEngine;

namespace Kairou.Editor
{
    public class VariableSelectGenericMenu
    {
        readonly GenericMenu _menu = new();

        public VariableSelectGenericMenu(Command command, TargetVariableScope variableScope, Func<VariableDefinition, bool> filterFunc, Action<VariableDefinition> onSelect)
        {
            if (command == null) return;
            
            var page = command.ParentPage;
            var book = page?.ParentBook;

            if (page != null && (variableScope == TargetVariableScope.None || variableScope == TargetVariableScope.Page))
            {
                foreach (var variable in page.Variables)
                {
                    if (filterFunc.Invoke(variable) == false) continue;
                    _menu.AddItem(new GUIContent("Page/" + variable.Name), false, () =>
                    {
                        onSelect.Invoke(variable);
                    });
                }
            }
            if (book != null && (variableScope == TargetVariableScope.None || variableScope == TargetVariableScope.Book))
            {
                foreach (var variable in book.Variables)
                {
                    if (filterFunc.Invoke(variable) == false) continue;
                    _menu.AddItem(new GUIContent("Book/" + variable.Name), false, () =>
                    {
                        onSelect.Invoke(variable);
                    });
                }
            }
        }

        public void ShowAsContext()
        {
            _menu.ShowAsContext();
        }
    }
}