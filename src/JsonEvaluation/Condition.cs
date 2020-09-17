using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public abstract class Condition : Rule
    {
        public abstract ConditionType Type { get; }

        public bool Not { get; set; }

        public IList<Rule> Rules { get; } = new List<Rule>();

        public static Condition Create(string typeStr)
        {
            var type = (ConditionType)Enum.Parse(typeof(ConditionType), typeStr, true);
            return Create(type);
        }

        public static Condition Create(ConditionType type)
        {
            return type switch
            {
                ConditionType.And => new AndCondition(),
                ConditionType.Or => new OrCondition(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid Condition")
            };
        }

        public override string ToString() => BuildRuleString(false);

        public override string ToFriendlyString() => BuildRuleString(true);

        private string BuildRuleString(bool friendly)
        {
            var sb = new StringBuilder("(");
            for (int i = 0; i < Rules.Count; i++)
            {
                string ruleStr = (friendly) ? Rules[i].ToFriendlyString() : Rules[i].ToString();
                sb.Append($"({ruleStr})");

                if (i < (Rules.Count - 1))
                    sb.Append($" {Type} ");
            }

            sb.Append(")");
            return sb.ToString();
        }

        public override RuleOptions Options
        {
            get => base.Options;
            set
            {   // cascade update
                base.Options = value;
                foreach (Rule rule in Rules)
                    rule.Options = value;
            }
        }

        sealed class AndCondition : Condition
        {
            public override ConditionType Type => ConditionType.And;

            public override bool Evaluate(JObject data)
            {
                bool res = Rules.All(r => r.Evaluate(data));
                return Not ? !res : res;
            }
        }

        sealed class OrCondition : Condition
        {
            public override ConditionType Type => ConditionType.Or;

            public override bool Evaluate(JObject data)
            {
                bool res = Rules.Any(r => r.Evaluate(data));
                return Not ? !res : res;
            }
        }
    }

}
