using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Coderz.Json.Evaluation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JsonEvaluation.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]

    public class EvaluatorTests_1
    {
        private readonly ITestOutputHelper _output;

        public EvaluatorTests_1(ITestOutputHelper output)
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
        public void Test_Rule_1_IgnoreCase()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json)
            {
                Options = { CompareOptions = CompareOptions.IgnoreCase }
            };
            _output.WriteLine(eval.ToString());

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"name", "Top plays by Joe Montana"},
                    {"price", 10.00},
                    {"category", 2}
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"amount", 30},
                    {"date", "12/31/2019"}
                })
            );

        }

        [Fact]
        public void Test_Rule_1_IgnoreAccent()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json)
            {
                Options = { CompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace }
            };
            _output.WriteLine(eval.ToString());

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"name", "Top plays by Joë Montana"},
                    {"price", 10.00},
                    {"category", 2}
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"amount", 30},
                    {"date", "12/31/2019"}
                })
            );
        }

        [Fact]
        public void Test_Rule_1_Exact()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json);
            _output.WriteLine(eval.ToString());

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"name", "Top plays by Joe Montana"},
                    {"price", 10.00},
                    {"category", 2}
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"amount", 30},
                    {"date", "12/31/2019"}
                })
            );
        }
    }
}
