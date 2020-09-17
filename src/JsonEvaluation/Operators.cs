using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    class EqualsFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotEqual : Operator.Equal;

        protected override bool MissingToken
        {
            get
            {
                bool isDefault = CompareValue.Equals(default);
                return (Not) ? !isDefault : isDefault;
            }
        }

        protected override bool Compare(DataValue<T> dataValue)
        {
            return dataValue.HasValue && dataValue.Value.Equals(CompareValue);
        }
    }

    class InFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotIn : Operator.In;

        protected override bool Compare(DataValue<T> dataValue)
        {
            return dataValue.HasValue && CompareList.Contains(dataValue.Value);
        }
    }

    class LessFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.GreaterOrEqual : Operator.Less;

        protected override bool Compare(DataValue<T> dataValue)
        {
            return dataValue.HasValue && (dataValue.Value.CompareTo(CompareValue) < 0);
        }
    }

    class GreaterFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.LessOrEqual : Operator.Greater;

        protected override bool Compare(DataValue<T> dataValue)
        {
            return dataValue.HasValue && (dataValue.Value.CompareTo(CompareValue) > 0);
        }
    }

    class BetweenFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBetween : Operator.Between;

        protected override bool Compare(DataValue<T> dataValue)
        {
            T lowValue = CompareList[0], highValue = CompareList[1];
            return dataValue.HasValue && (dataValue.Value.CompareTo(lowValue) >= 0 && dataValue.Value.CompareTo(highValue) <= 0);
        }
    }

    class NullFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.IsNotNull : Operator.IsNull;

        protected override bool MissingToken => !Not;

        protected override bool Compare(DataValue<T> dataValue)
        {
            return !dataValue.HasValue || dataValue.Value.Equals(null);
        }
    }

    class EmptyFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.IsNotEmpty : Operator.IsEmpty;

        protected override bool MissingToken => !Not;

        protected override bool Compare(DataValue<T> dataValue)
        {
            if (!dataValue.HasValue) return true;
            return (typeof(T) == typeof(string))
                ? string.IsNullOrEmpty(dataValue.Value?.ToString())
                : dataValue.Value.Equals(default);
        }

    }

    class ContainsFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotContains : Operator.Contains;

        protected override bool MissingToken => Not;

        public override bool Evaluate(JObject data)
        {
            JToken dataToken = GetDataToken(data);
            if (dataToken == null) return MissingToken;

            bool res = CompareDataToken(dataToken);
            return Not ? !res : res;
        }

        private bool CompareDataToken(JToken dataToken)
        {
            if (dataToken is JArray dataArray) 
            {   // array contains item
                return dataArray.Any(v =>
                {
                    DataValue<T> item = FromJToken(v);
                    return item.HasValue && item.Value.Equals(CompareValue);
                });
            }

            DataValue<T> dataValue = FromJToken(dataToken);
            if (!dataValue.HasValue) return false;

            // try string contains
            if (typeof(T) == typeof(string) && dataValue.Value != null && CompareValue != null)
            {
                string dataStr = dataValue.Value.ToString(), compareStr = CompareValue.ToString();
                if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr)) return false;

                return dataStr.Contains(compareStr, StringComparison.InvariantCultureIgnoreCase);
            }

            // last resort (equals)
            return dataValue.Value?.Equals(CompareValue) ?? false;
        }
    }

    abstract class StringPartRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        protected override bool MissingToken => Not;

        protected abstract bool CheckString(string dataStr, string compareStr);

        protected override bool Compare(DataValue<T> dataValue)
        {
            if (!dataValue.HasValue) return false;

            string dataStr = dataValue.Value?.ToString();
            string compareStr = CompareValue?.ToString();

            if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr))
                return false;

            return CheckString(dataStr, compareStr);
        }
    }

    class BeginsWithRule<T>: StringPartRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBeginsWith : Operator.BeginsWith;

        protected override bool CheckString(string dataStr, string compareStr)
        {
            return dataStr.StartsWith(compareStr, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    class EndsWithRule<T>: StringPartRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotEndsWith : Operator.EndsWith;

        protected override bool CheckString(string dataStr, string compareStr)
        {
            return dataStr.EndsWith(compareStr, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
