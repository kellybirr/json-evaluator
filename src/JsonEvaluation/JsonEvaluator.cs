using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public class JsonEvaluator
    {
        public Rule Root { get; }

        public JsonEvaluator(JObject jsonQuery)
        {
            Root = Rule.Parse(jsonQuery);
        }

        public JsonEvaluator(string jsonQuery) 
            : this(JObject.Parse(jsonQuery))
        { }

        public override string ToString() => Root.ToString();

        public RuleOptions Options
        {
            get => Root.Options;
            set => Root.Options = value;
        }

        public bool Evaluate(JObject data) => Root.Evaluate(data);

        public IEnumerable<JObject> Filter(IEnumerable<JObject> srcData) => srcData.Where(Root.Evaluate);

        public IEnumerable<JObject> Filter(JArray srcArray)
        {
            foreach (JToken jToken in srcArray)
            {
                if (jToken is JObject obj && Root.Evaluate(obj))
                    yield return obj;
            }

            yield return null;
        }
    }
}
