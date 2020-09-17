using System;
using System.Diagnostics.CodeAnalysis;
using Coderz.Json.Evaluation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JsonEvaluation.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EvaluatorTests_3
    {
        private readonly ITestOutputHelper _output;

        public EvaluatorTests_3(ITestOutputHelper output)
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
                        new JObject { {"field","tags"},{"type","string"},{"operator","contains"},{"value", "tag2"} },
                        new JObject { {"field","call.time"},{"type","datetime"},{"operator","between"},{"value",JArray.Parse("['1/1/2019','12/31/2019']")} },
                        new JObject { {"field","call.length"},{"type","duration"},{"operator","greater"},{"value","PT1M"} }
                    }
                }
            };
        }

        [Theory]
        [InlineData("00:10:00")]
        [InlineData("1.09:15:30")]
        [InlineData("PT25M30S")]
        [InlineData("P3DT2H30M5S")]
        public void Test_Duration_Parsing(string duration)
        {
            DataValue<TimeSpan> ts = DateParser.Duration( new JValue(duration) );
            Assert.True(ts.HasValue);

            _output.WriteLine(ts.Value.ToString());
        }

        [Fact]
        public void Test_Rule_3()
        {
            JObject json = RuleJson();
            var eval = new JsonEvaluator(json);
            _output.WriteLine(eval.ToString());

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"call", new JObject
                        {
                            {"time", "2019-06-01T08:00:00-1:00"},
                            {"length", "PT45M"}
                        }
                    },
                    { "tags", new JArray {"tag1","tag2"} }
                })
            );

            Assert.True(
                eval.Evaluate(new JObject
                {
                    {"call", new JObject
                        {
                            {"time", "2019-03-15T19:00:00Z"},
                            {"length", "PT2H"}
                        }
                    },
                    { "tags", new JArray {"tag2"} }
                })
            );


            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"call", new JObject
                        {
                            {"time", "2019-06-01T08:00:00-1:00"},
                            {"length", "PT25s"}
                        }
                    },
                    { "tags", new JArray {"tag1","tag2"} }
                })
            );

            Assert.False(
                eval.Evaluate(new JObject
                {
                    {"call", new JObject
                        {
                            {"time", "2019-03-15T19:00:00Z"},
                            {"length", "PT4H"}
                        }
                    }
                })
            );
        }
    }
}
