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

        protected override bool CompareT(T dataValueT)
        {
            return (typeof(T) == typeof(string))
                ? (CompareStrings(dataValueT, CompareValue) == 0)
                : dataValueT.Equals(CompareValue);
        }
    }

    class InFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotIn : Operator.In;

        protected override bool CompareT(T dataValueT)
        {
            return CompareList.Contains(dataValueT);
        }
    }

    class LessFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.GreaterOrEqual : Operator.Less;

        protected override bool CompareT(T dataValueT)
        {
            return (typeof(T) == typeof(string))
                ? (CompareStrings(dataValueT, CompareValue) < 0)
                : (dataValueT.CompareTo(CompareValue) < 0);
        }
    }

    class GreaterFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.LessOrEqual : Operator.Greater;

        protected override bool CompareT(T dataValueT)
        {
            return (typeof(T) == typeof(string))
                ? (CompareStrings(dataValueT, CompareValue) > 0)
                : (dataValueT.CompareTo(CompareValue) > 0);
        }
    }

    class BetweenFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBetween : Operator.Between;

        protected override bool CompareT(T dataValueT)
        {   
            T lowValue = CompareList[0], highValue = CompareList[1];
            return (typeof(T) == typeof(string))
                ? (CompareStrings(dataValueT, lowValue) >= 0 && CompareStrings(dataValueT, highValue) <= 0)
                : (dataValueT.CompareTo(lowValue) >= 0 && dataValueT.CompareTo(highValue) <= 0);
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
            // data is array of items
            if (dataToken is JArray dataArray) 
                return dataArray.Any(TestArrayItem);

            DataValue<T> dataValue = FromJToken(dataToken);
            if (!dataValue.HasValue) return false;

            // data is string - look for partial match
            if (typeof(T) == typeof(string) && dataValue.Value != null && CompareValue != null)
            {
                string dataStr = dataValue.Value.ToString(), compareStr = CompareValue.ToString();
                if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr)) return false;

                return (Options.Culture.CompareInfo.IndexOf(dataStr, compareStr, Options.CompareOptions) >= 0);
            }

            // last resort (equals)
            return dataValue.Value?.Equals(CompareValue) ?? false;
        }

        private bool TestArrayItem(JToken itemToken)
        {
            DataValue<T> item = FromJToken(itemToken);
            if (!item.HasValue) return false;

            return (typeof(T) == typeof(string))
                ? (CompareStrings(item.Value, CompareValue) == 0)
                : item.Value.Equals(CompareValue);
        }
    }

    abstract class StringPartRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        protected override bool MissingToken => Not;

        protected abstract bool CheckStringPart(string dataStr, string compareStr);

        protected override bool CompareT(T dataValueT)
        {
            string dataStr = dataValueT?.ToString();
            string compareStr = CompareValue?.ToString();

            if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr))
                return false;

            return CheckStringPart(dataStr, compareStr);
        }
    }

    class BeginsWithRule<T>: StringPartRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBeginsWith : Operator.BeginsWith;

        protected override bool CheckStringPart(string dataStr, string compareStr)
            => Options.Culture.CompareInfo.IsPrefix(dataStr, compareStr, Options.CompareOptions);
    }

    class EndsWithRule<T>: StringPartRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotEndsWith : Operator.EndsWith;

        protected override bool CheckStringPart(string dataStr, string compareStr)
            => Options.Culture.CompareInfo.IsSuffix(dataStr, compareStr, Options.CompareOptions);
    }
}
