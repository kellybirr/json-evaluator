using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public abstract class Rule
    {
        public abstract bool Evaluate(JObject data);

        public static Rule Parse(JObject json)
        {
            // condition (rule set)
            if (json.TryGetValue(TokenName.Condition, out JToken conditionToken))
                return ParseCondition(json, conditionToken);

            return ParseFieldRule(json);
        }

        public virtual string ToFriendlyString() => ToString();

        private static Rule ParseCondition(JObject json, JToken conditionToken)
        {
            // create condition
            var conditionObj = Condition.Create((string) conditionToken);
            if (json.TryGetValue(TokenName.Rules, out JToken rulesToken) && rulesToken is JArray rulesArray)
            {
                // process rules
                foreach (JToken subRule in rulesArray)
                {
                    if (subRule is JObject subRuleObj)
                        conditionObj.Rules.Add(Rule.Parse(subRuleObj));
                }
            }

            // handle NOT
            if (json.TryGetValue(TokenName.Not, out JToken notToken) && notToken.Type == JTokenType.Boolean)
                conditionObj.Not = notToken.Value<bool>();

            return conditionObj;
        }

        private static Rule ParseFieldRule(JObject json)
        {
            // parse operator
            string operatorStr = json[TokenName.Operator]?.ToString() ?? throw new ArgumentException("Missing 'operator'");
            operatorStr = Regex.Replace(operatorStr.ToLowerInvariant(), @"[^a-z]", "");
            Operator op = Enum.Parse<Operator>(operatorStr, true);
            
            // parse field type
            string fieldTypeStr = json[TokenName.Type]?.ToString() ?? throw new ArgumentException("Missing 'type'");
            FieldType fieldType = Enum.Parse<FieldType>(fieldTypeStr, true);

            FieldRule ruleObj = fieldType switch
            {
                FieldType.String => FieldRule.Create<string>(op),
                FieldType.Integer => FieldRule.Create<long>(op),
                FieldType.Double => FieldRule.Create<double>(op),
                FieldType.Boolean => FieldRule.Create<bool>(op),
                FieldType.DateTime => FieldRule.Create<DateTimeOffset>(op),
                FieldType.Date => FieldRule.Create<DateTime>(op),
                FieldType.Duration => FieldRule.Create<TimeSpan>(op),
                _ => throw new ArgumentOutOfRangeException()
            };

            ruleObj.Field = json[TokenName.Field]?.ToString() ?? throw new ArgumentException("Missing 'field'");
            ruleObj.Value = json[TokenName.Value];

            return ruleObj;
        }
    }
}
