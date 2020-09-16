using System.Diagnostics.CodeAnalysis;
using Coderz.Json.Evaluation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JsonEvaluation.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EvaluatorTests_2
    {
        private readonly ITestOutputHelper _output;

        public EvaluatorTests_2(ITestOutputHelper output)
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
                        new JObject { {"field","sale.amount"},{"type","double"},{"operator","greater"},{"value", 20.00d} },
                        new JObject { {"field","sale.date"},{"type","date"},{"operator","between"},{"value",JArray.Parse("['1/1/2019','12/31/2019']")} }
                    }
                }
            };
        }

        [Fact]
        public void Test_Rule_2()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json);
            _output.WriteLine(eval.ToString());

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"sale", new JObject
                        {
                            {"amount", 22.50},
                            {"date", "6/1/2019"}
                        }
                    }
                })
            );

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"sale", new JObject
                        {
                            {"amount", 30},
                            {"date", "12/31/2019"}
                        }
                    }
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"sale", new JObject
                        {
                            {"amount", 40},
                            {"date", "8/25/2020"}
                        }
                    }
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"sale", new JObject
                        {
                            {"amount", 19},
                            {"date", "2/2/2019"}
                        }
                    }
                })
            );
        }
    }
}
