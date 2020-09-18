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

        protected override bool DoCompareAsT(T dataValueT) => dataValueT.Equals(CompareValue);

        protected override bool DoCompareAsString(T dataValueT) => (CompareStrings(dataValueT, CompareValue) == 0);
    }

    class InFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotIn : Operator.In;

        protected override bool DoCompareAsT(T dataValueT) => CompareList.Contains(dataValueT);

        protected override bool DoCompareAsString(T dataValueT)
            => CompareList.Any(v => CompareStrings(dataValueT, v) == 0);
    }

    class LessFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.GreaterOrEqual : Operator.Less;

        protected override bool DoCompareAsT(T dataValueT) => (dataValueT.CompareTo(CompareValue) < 0);

        protected override bool DoCompareAsString(T dataValueT) => (CompareStrings(dataValueT, CompareValue) < 0);
    }

    class GreaterFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.LessOrEqual : Operator.Greater;

        protected override bool DoCompareAsT(T dataValueT) => (dataValueT.CompareTo(CompareValue) > 0);

        protected override bool DoCompareAsString(T dataValueT) => (CompareStrings(dataValueT, CompareValue) > 0);
    }

    class BetweenFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBetween : Operator.Between;

        protected override bool DoCompareAsT(T dataValueT)
        {   
            T lowValue = CompareList[0], highValue = CompareList[1];
            return (dataValueT.CompareTo(lowValue) >= 0 && dataValueT.CompareTo(highValue) <= 0);
        }

        protected override bool DoCompareAsString(T dataValueT)
        {
            T lowValue = CompareList[0], highValue = CompareList[1];
            return (CompareStrings(dataValueT, lowValue) >= 0 && CompareStrings(dataValueT, highValue) <= 0);
        }
    }

    class NullFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.IsNotNull : Operator.IsNull;

        protected override bool MissingToken => !Not;

        protected override bool DoCompare(DataValue<T> dataValue)
        {
            return !dataValue.HasValue || dataValue.Value.Equals(null);
        }
    }

    class EmptyFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        private readonly Func<T, bool> _isEmptyFunc;

        public EmptyFieldRule()
        {
            if (typeof(T) == typeof(string))
                _isEmptyFunc = d => string.IsNullOrEmpty(d?.ToString());
            else
                _isEmptyFunc = d => d.Equals(default);
        }

        public override Operator Operator => (Not) ? Operator.IsNotEmpty : Operator.IsEmpty;

        protected override bool MissingToken => !Not;

        protected override bool DoCompare(DataValue<T> dataValue)
        {
            return !dataValue.HasValue || _isEmptyFunc(dataValue.Value);
        }
    }

    class ContainsFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        private readonly Func<T, bool> _singleTestFunc, _arrayTestFunc;

        public ContainsFieldRule()
        {
            if (typeof(T) == typeof(string))
            {
                _singleTestFunc = TestStringContains;
                _arrayTestFunc = v => CompareStrings(v, CompareValue) == 0;
            }
            else
            {
                _singleTestFunc = d => d?.Equals(CompareValue) ?? false;
                _arrayTestFunc = v => v.Equals(CompareValue);
            }
        }

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

            // single item, like string.contains
            DataValue<T> dataValue = FromJToken(dataToken);
            return dataValue.HasValue && _singleTestFunc(dataValue.Value);
        }

        private bool TestArrayItem(JToken itemToken)
        {
            DataValue<T> item = FromJToken(itemToken);
            return item.HasValue && _arrayTestFunc(item.Value);
        }

        private bool TestStringContains(T dataValueT)
        {
            string dataStr = dataValueT?.ToString(), compareStr = CompareValue?.ToString();
            if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr)) return false;

            return Options.Culture.CompareInfo.IndexOf(dataStr, compareStr, Options.CompareOptions) >= 0;
        }
    }

    abstract class StringPartRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        protected override bool MissingToken => Not;

        protected abstract bool CheckStringPart(string dataStr, string compareStr);

        protected override bool DoCompare(DataValue<T> dataValue)
        {
            string dataStr = dataValue.Value?.ToString();
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
