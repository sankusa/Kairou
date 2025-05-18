using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    public static class AssignOperatorOperatorExtension
    {
        public static string GetOperatorString(this VariableSetter.AssignOperator assignOperator)
        {
            return assignOperator switch
            {
                VariableSetter.AssignOperator.Assign => "=",
                VariableSetter.AssignOperator.Negate => "=!",
                VariableSetter.AssignOperator.Add => "+=",
                VariableSetter.AssignOperator.Subtract => "-=",
                VariableSetter.AssignOperator.Multiply => "*=",
                VariableSetter.AssignOperator.Divide => '\u2215' + "=",
                VariableSetter.AssignOperator.Modulo => "%=",
                _ => throw new ArgumentOutOfRangeException(nameof(assignOperator)),
            };
        }
    }

    [Serializable]
    public abstract class VariableSetter : IValidatableAsCommandField
    {
        public enum AssignOperator
        {
            Assign,
            Negate,
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo,
        }

        public abstract Type TargetType { get; }

        public abstract AssignOperator[] GenerateAllowedOperators();

        public abstract AssignOperator Operator { get; }

        public abstract void Set(IProcessInterface process);
        public abstract string GetSummary();
        public abstract IEnumerable<string> Validate(Command command, string fieldName);
    }

    [Serializable]
    public class VariableSetter<T> : VariableSetter
    {
        public override Type TargetType => typeof(T);

        public override AssignOperator[] GenerateAllowedOperators() => Enum
            .GetValues(typeof(AssignOperator))
            .Cast<AssignOperator>()
            .Where(x =>
            {
                return x switch
                {
                    AssignOperator.Assign => true,
                    AssignOperator.Negate => VariableTypeDictionary.Dic[TargetType].CanNegate,
                    AssignOperator.Add => VariableTypeDictionary.Dic[TargetType].CanAdd,
                    AssignOperator.Subtract => VariableTypeDictionary.Dic[TargetType].CanSubtract,
                    AssignOperator.Multiply => VariableTypeDictionary.Dic[TargetType].CanMultiply,
                    AssignOperator.Divide => VariableTypeDictionary.Dic[TargetType].CanDivide,
                    AssignOperator.Modulo => VariableTypeDictionary.Dic[TargetType].CanModulo,
                    _ => false,
                };
            })
            .ToArray();

        [SerializeField] VariableValueAccessorKey<T> _variable;
        [SerializeField] FlexibleParameter<T> _value;
        [SerializeField] AssignOperator _operator;
        public override AssignOperator Operator => _operator;

        public override void Set(IProcessInterface process)
        {
            var accessor = _variable.Find(process);
            var value = _value.ResolveValue(process);

            switch(_operator)
            {
                case AssignOperator.Assign:
                    accessor.SetValue(_value.ResolveValue(process));
                    return;
                case AssignOperator.Negate:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Negate(value));
                    return;
                case AssignOperator.Add:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Add(accessor.GetValue(), value));
                    return;
                case AssignOperator.Subtract:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Subtract(accessor.GetValue(), value));
                    return;
                case AssignOperator.Multiply:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Multiply(accessor.GetValue(), value));
                    return;
                case AssignOperator.Divide:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Divide(accessor.GetValue(), value));
                    return;
                case AssignOperator.Modulo:
                    accessor.SetValue(VariableTypeCache<T>.VariableType.Modulo(accessor.GetValue(), value));
                    return;
                default:
                    throw new InvalidOperationException(nameof(_operator));
            }
        }

        public override string GetSummary()
        {
            return $"{_variable.GetSummary()} {_operator.GetOperatorString()} {_value.GetSummary()}";
        }

        public override IEnumerable<string> Validate(Command command, string fieldName)
        {
            foreach (string errorMessage in _variable.Validate(command, $"{fieldName}.{nameof(_variable)}"))
            {
                yield return errorMessage;
            }
            foreach (string errorMessage in _value.Validate(command, $"{fieldName}.{nameof(_value)}"))
            {
                yield return errorMessage;
            }

            switch (_operator)
            {
                case AssignOperator.Assign:
                    break;
                case AssignOperator.Negate:
                    if (VariableTypeCache<T>.VariableType.CanNegate == false) yield return $"{fieldName} : Can't Negate";
                    break;
                case AssignOperator.Add:
                    if (VariableTypeCache<T>.VariableType.CanAdd == false) yield return $"{fieldName} : Can't Add";
                    break;
                case AssignOperator.Subtract:
                    if (VariableTypeCache<T>.VariableType.CanSubtract == false) yield return $"{fieldName} : Can't Subtract";
                    break;
                case AssignOperator.Multiply:
                    if (VariableTypeCache<T>.VariableType.CanMultiply == false) yield return $"{fieldName} : Can't Multiply";
                    break;
                case AssignOperator.Divide:
                    if (VariableTypeCache<T>.VariableType.CanDivide == false) yield return $"{fieldName} : Can't Divide";
                    break;
                case AssignOperator.Modulo:
                    if (VariableTypeCache<T>.VariableType.CanModulo == false) yield return $"{fieldName} : Can't Modulo";
                    break;
                default:
                    throw new InvalidOperationException(nameof(_operator));
            }
        }
    }
}