using System;
using System.Collections.Generic;

namespace Coderz.Json.Evaluation
{
    public readonly struct DataValue<T> : IEquatable<DataValue<T>>
    {
        public DataValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public DataValue(bool hasValue)
        {
            Value = default;
            HasValue = hasValue;
        }

        public T Value { get; }

        public bool HasValue { get; }

        public override string ToString() => Value?.ToString();

        public override int GetHashCode() => HashCode.Combine(Value);

        public static explicit operator T(DataValue<T> obj) => obj.Value;

        public override bool Equals(object obj)
        {
            return obj is DataValue<T> other && Equals(other);
        }

        public bool Equals(DataValue<T> other)
        {
            return HasValue && other.HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public static bool operator ==(DataValue<T> left, DataValue<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DataValue<T> left, DataValue<T> right)
        {
            return !(left == right);
        }
    }
}
