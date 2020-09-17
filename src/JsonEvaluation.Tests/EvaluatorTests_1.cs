using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Coderz.Json.Evaluation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JsonEvaluation.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]

    public class EvaluatorTests_4
    {
        private readonly ITestOutputHelper _output;

        public EvaluatorTests_4(ITestOutputHelper output)
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
                        new JObject { {"field","product"},{"type","string"},{"operator","in"},{"value", JArray.Parse("['hat','shirt']")} },
                    }
                }
            };
        }

        [Fact]
        public void Test_Rule_4_IgnoreCase()
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
                    {"amount", 10},
                    {"product", "Hat"}
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"amount", 30},
                    {"product", "Pants"}
                })
            );
        }

        [Fact]
        public void Test_Rule_4_Exact()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json);
            _output.WriteLine(eval.ToString());

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"amount", 10},
                    {"product", "Hat"}
                })
            );

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"amount", 30},
                    {"product", "shirt"}
                })
            );
        }
    }
}
