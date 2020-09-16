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

        protected override bool Compare(T dataValue)
        {
            return dataValue.Equals(CompareValue);
        }
    }

    class InFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotIn : Operator.In;

        protected override bool Compare(T dataValue)
        {
            return CompareList.Contains(dataValue);
        }
    }

    class LessFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.GreaterOrEqual : Operator.Less;

        protected override bool Compare(T dataValue)
        {
            return (dataValue.CompareTo(CompareValue) < 0);
        }
    }

    class GreaterFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.LessOrEqual : Operator.Greater;

        protected override bool Compare(T dataValue)
        {
            return (dataValue.CompareTo(CompareValue) > 0);
        }
    }

    class BetweenFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.NotBetween : Operator.Between;

        protected override bool Compare(T dataValue)
        {
            T lowValue = CompareList[0], highValue = CompareList[1];
            return (dataValue.CompareTo(lowValue) >= 0 && dataValue.CompareTo(highValue) <= 0);
        }
    }

    class NullFieldRule<T> : FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.IsNotNull : Operator.IsNull;

        protected override bool MissingToken => !Not;

        protected override bool Compare(T dataValue)
        {
            return dataValue.Equals(null);
        }
    }

    class EmptyFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.IsNotEmpty : Operator.IsEmpty;

        protected override bool MissingToken => !Not;

        protected override bool Compare(T dataValue)
        {
            return (typeof(T) == typeof(string))
                ? string.IsNullOrEmpty(dataValue?.ToString())
                : dataValue.Equals(default);
        }

    }

    class ContainsFieldRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.DoesNotContain : Operator.Contains;

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
            if (dataToken is JArray dataArray)  // array contains item
                return dataArray.Any(v => FromJToken(v).Equals(CompareValue));

            T dataValue = FromJToken(dataToken);    // try string contains
            if (typeof(T) == typeof(string) && dataValue != null && CompareValue != null)
            {
                string dataStr = dataValue.ToString(), compareStr = CompareValue.ToString();
                if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr)) return false;

                return dataStr.Contains(compareStr, StringComparison.InvariantCultureIgnoreCase);
            }

            // last resort (equals)
            return dataValue?.Equals(CompareValue) ?? false;
        }
    }

    class BeginsWithRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.DoesNotBeginWith : Operator.BeginsWith;

        protected override bool MissingToken => Not;

        protected override bool Compare(T dataValue)
        {
            string dataStr = dataValue?.ToString();
            string compareStr = CompareValue?.ToString();

            if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr))
                return false;

            return dataStr.StartsWith(compareStr, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    class EndsWithRule<T>: FieldRule<T> where T: IComparable<T>, IEquatable<T>
    {
        public override Operator Operator => (Not) ? Operator.DoesNotEndWith : Operator.EndsWith;

        protected override bool MissingToken => Not;

        protected override bool Compare(T dataValue)
        {
            string dataStr = dataValue?.ToString();
            string compareStr = CompareValue?.ToString();

            if (string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(compareStr))
                return false;

            return dataStr.StartsWith(compareStr, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
