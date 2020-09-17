using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public abstract class FieldRule : Rule
    {
        public string Field { get; set; }

        public abstract Operator Operator { get; }

        public FieldType Type { get; protected set; }

        public virtual JToken Value { get; set; }

        public bool Not { get; set; }

        public override string ToString() => $"{Type}(`{Field}`) {Operator} {Value?.ToString(Formatting.None)}";

        public override string ToFriendlyString()=> $"`{Field}` {Operator} {Value?.ToString(Formatting.None)}";

        public static FieldRule Create<T>(Operator op) where T : IComparable<T>, IEquatable<T>
        {
            return op switch
            {
                Operator.Equal => new EqualsFieldRule<T>(),
                Operator.NotEqual => new EqualsFieldRule<T> {Not = true},
                Operator.In => new InFieldRule<T>(),
                Operator.NotIn => new InFieldRule<T> {Not = true},
                Operator.Less => new LessFieldRule<T>(),
                Operator.LessOrEqual => new GreaterFieldRule<T> {Not = true},
                Operator.Greater => new GreaterFieldRule<T>(),
                Operator.GreaterOrEqual => new LessFieldRule<T> {Not = true},
                Operator.Between => new BetweenFieldRule<T>(),
                Operator.NotBetween => new BetweenFieldRule<T> {Not = true},
                Operator.IsNull => new NullFieldRule<T>(),
                Operator.IsNotNull => new NullFieldRule<T> {Not = true},
                Operator.IsEmpty => new EmptyFieldRule<T>(),
                Operator.IsNotEmpty => new EmptyFieldRule<T> {Not = true},
                Operator.Contains => new ContainsFieldRule<T>(),
                Operator.NotContains => new ContainsFieldRule<T> {Not = true},
                Operator.BeginsWith => new BeginsWithRule<T>(),
                Operator.NotBeginsWith => new BeginsWithRule<T> {Not = true},
                Operator.EndsWith => new EndsWithRule<T>(),
                Operator.NotEndsWith => new EndsWithRule<T> {Not = true},
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, "Invalid Operator")
            };
        }

        protected int CompareStrings(string dataStr, string compareStr)
            => string.Compare(dataStr, compareStr, Options.Culture, Options.CompareOptions);
    }

    public abstract class FieldRule<T> : FieldRule where T : IComparable<T>, IEquatable<T>
    {
        private readonly Func<T, bool> _compareFunc;

        protected FieldRule()
        {
            if (typeof(T) == typeof(string))
            {
                _compareFunc = CompareS;
                Type = FieldType.String;
            }
            else
            {
                _compareFunc = CompareT;
                if (typeof(T) == typeof(long))
                    Type = FieldType.Integer;
                else if (typeof(T) == typeof(double))
                    Type = FieldType.Double;
                else if (typeof(T) == typeof(bool))
                    Type = FieldType.Boolean;
                else if (typeof(T) == typeof(DateTimeOffset))
                    Type = FieldType.DateTime;
                else if (typeof(T) == typeof(DateTime))
                    Type = FieldType.Date;
                else if (typeof(T) == typeof(TimeSpan))
                    Type = FieldType.Duration;
            }
        }

        protected virtual bool Compare(DataValue<T> dataValue)
            => dataValue.HasValue && _compareFunc(dataValue.Value);

        protected virtual bool CompareT(T dataValueT) => false;

        protected virtual bool CompareS(T dataValueT) => false;

        protected virtual bool MissingToken => false;

        public override bool Evaluate(JObject data)
        {
            JToken dataToken = GetDataToken(data);
            if (dataToken == null) return MissingToken;

            DataValue<T> dataValue = FromJToken(dataToken);

            bool res = Compare(dataValue);
            return Not ? !res : res;
        }

        protected JToken GetDataToken(JObject data)
        {
            string[] fieldPath = Field.Split('.', StringSplitOptions.RemoveEmptyEntries);   // traverse
            return fieldPath.Aggregate<string, JToken>(data, (current, name) => current?[name]);
        }

        protected IList<T> CompareList { get; private set; }

        protected virtual T CompareValue { get; private set; }

        public override JToken Value
        {
            get => base.Value;
            set
            {
                base.Value = value;

                CompareList = (value is JArray valueArray)
                    ? (from token in valueArray select FromJToken(token).Value).ToList()
                    : new List<T> { FromJToken(value).Value };

                CompareValue = CompareList.FirstOrDefault();
            }
        }

        protected DataValue<T> FromJToken(JToken token)
        {
            if (typeof(T) == typeof(DateTimeOffset) && DateParser.DateAndTime(token, Options.Culture) is DataValue<T> dtoT)
                return dtoT;    // special handling for DateTimeOffset (date + time + offset)

            if (typeof(T) == typeof(DateTime) && DateParser.DateOnly(token, Options.Culture) is DataValue<T> dT)
                return dT;    // special handling for DateTime (date only)

            if (typeof(T) == typeof(TimeSpan) && DateParser.Duration(token, Options.Culture) is DataValue<T> tsT)
                return tsT;    // special handling for TimeSpan (duration)

            // default conversion
            return new DataValue<T>((T)Convert.ChangeType(token, typeof(T)));
        }

        protected int CompareStrings(T dataValueT, T compareValueT) 
            => CompareStrings(dataValueT.ToString(), compareValueT.ToString());
    }
}
