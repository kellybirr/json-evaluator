using Coderz.Json.Evaluation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JsonEvaluation.Tests
{
    public class ParserTests
    {
        private readonly ITestOutputHelper _output;

        public ParserTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        static JObject RuleJson()
        {
            return new JObject
            {
                {"condition", "AND"},
                {"rules", new JArray
                    {   
                        new JObject { {"field","name"},{"type","string"},{"operator","contains"},{"value","joe"} },
                        new JObject { {"field","price"},{"type","double"},{"operator","between"},{"value",JArray.Parse("[8.00,11.00]")} },
                        new JObject
                        {
                            {"condition", "OR"},
                            {"rules", new JArray
                                {
                                    new JObject { {"field","category"},{"type","integer"},{"operator","equal"},{"value", 1} },
                                    new JObject { {"field","category"},{"type","integer"},{"operator","in"},{"value", JArray.Parse("[2,3,4]")} },
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void Test_FriendlyString()
        {
            JObject json = RuleJson();
            Rule rule = Rule.Parse(json);
            _output.WriteLine(rule.ToFriendlyString());
        }
    }
}
