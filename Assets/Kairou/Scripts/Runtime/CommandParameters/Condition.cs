using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    public static class CompareOperatorExtensions {
        public static string GetOperatorString(this Condition.CompareOperator compareOperator)
        {
            if(compareOperator == Condition.CompareOperator.EqualTo) return "==";
            if(compareOperator == Condition.CompareOperator.NotEqualTo) return "!=";
            if(compareOperator == Condition.CompareOperator.LessThan) return "<";
            if(compareOperator == Condition.CompareOperator.GreaterThan) return ">";
            if(compareOperator == Condition.CompareOperator.LessThanOrEqualTo) return "<=";
            if(compareOperator == Condition.CompareOperator.GreaterThanOrEqualTo) return ">=";
            return "";
        }
    }

    [Serializable]
    public abstract class Condition : IValidatableAsCommandField
    {
        public enum CompareOperator
        {
            EqualTo,
            NotEqualTo,
            LessThan,
            GreaterThan,
            LessThanOrEqualTo,
            GreaterThanOrEqualTo,
        }

        public static CompareOperator[] OperatorsForCompareable = new CompareOperator[]
        {
            CompareOperator.EqualTo,
            CompareOperator.NotEqualTo,
            CompareOperator.LessThan,
            CompareOperator.GreaterThan,
            CompareOperator.LessThanOrEqualTo,
            CompareOperator.GreaterThanOrEqualTo,
        };

        public static CompareOperator[] OperatorsForNotCompareable = new CompareOperator[]
        {
            CompareOperator.EqualTo,
            CompareOperator.NotEqualTo,
        };

        public abstract Type TargetType { get; }
        public abstract CompareOperator Operator { get; }

        public abstract bool Evaluate(IProcessInterface process);
        public abstract string GetSummary();
        public abstract IEnumerable<string> Validate(Command command, string fieldName);
    }

    [Serializable]
    public class Condition<T> : Condition
    {
        public override Type TargetType => typeof(T);

        [SerializeField] FlexibleParameter<T> _value1;
        [SerializeField] FlexibleParameter<T> _value2;
        [SerializeField] CompareOperator _operator;
        public override CompareOperator Operator => _operator;

        public override bool Evaluate(IProcessInterface process)
        {
            T value1 = _value1.ResolveValue(process);
            T value2 = _value2.ResolveValue(process);

            return _operator switch
            {
                CompareOperator.EqualTo => EqualityComparer<T>.Default.Equals(value1, value2),
                CompareOperator.NotEqualTo => !EqualityComparer<T>.Default.Equals(value1, value2),
                CompareOperator.LessThan => Compare(value1, value2) < 0,
                CompareOperator.GreaterThan => Compare(value1, value2) > 0,
                CompareOperator.LessThanOrEqualTo => Compare(value1, value2) <= 0,
                CompareOperator.GreaterThanOrEqualTo => Compare(value1, value2) >= 0,
                _ => throw new InvalidOperationException($"Unexpected value {_operator} for CompareOperator."),
            };
        }

        static int Compare(T value1, T value2)
        {
            if (value1 is IComparable<T> genericComparable)
                genericComparable.CompareTo(value2);
            if (value1 is IComparable comparable)
                return comparable.CompareTo(value2);
            throw new InvalidOperationException($"Cannot compare {value1} and {value2}");
        }

        public override string GetSummary()
        {
            return $"{_value1.GetSummary()} {_operator.GetOperatorString()} {_value2.GetSummary()}";
        }

        public override IEnumerable<string> Validate(Command command, string fieldName)
        {
            foreach (string errorMessage in _value1.Validate(command, $"{fieldName}.{nameof(_value1)}"))
            {
                yield return errorMessage;
            }
            foreach (string errorMessage in _value2.Validate(command, $"{fieldName}.{nameof(_value2)}"))
            {
                yield return errorMessage;
            }
        }
    }
}