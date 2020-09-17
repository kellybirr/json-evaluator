namespace Coderz.Json.Evaluation
{
    public enum ConditionType
    {
        And,
        Or
    }

    public enum Operator
    {
        Equal,
        NotEqual,
        In,
        NotIn,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        Between,
        NotBetween,
        IsNull,
        IsNotNull,
        IsEmpty,
        IsNotEmpty,
        Contains,
        NotContains,
        BeginsWith,
        NotBeginsWith,
        EndsWith,
        NotEndsWith
    }

    public enum FieldType
    {
        Unknown,
        String,
        Integer,
        Double,
        Boolean,
        DateTime,
        Date,
        Duration
    }

    public static class TokenName
    {
        public const string Condition = "condition";
        public const string Rules = "rules";
        public const string Id = "id";
        public const string Field = "field";
        public const string Type = "type";
        public const string Input = "input";
        public const string Operator = "operator";
        public const string Value = "value";
        public const string Not = "not";
    }
}
