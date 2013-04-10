using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace gov.va.medora.utils
{
    [TestFixture]
    public class JsonUtilsTest
    {
        [Test]
        public void testSerializeStringDictionary()
        {
            Dictionary<String, String> keysAndVals = new Dictionary<string, string>();
            keysAndVals.Add("Key01", "Value01");
            keysAndVals.Add("Key02", "Value02");
            keysAndVals.Add("Key03", "Value03");

            String result = JsonUtils.Serialize<Dictionary<String, String>>(keysAndVals);

            Assert.IsTrue(String.Equals(result, "[{\"Key\":\"Key01\",\"Value\":\"Value01\"},{\"Key\":\"Key02\",\"Value\":\"Value02\"},{\"Key\":\"Key03\",\"Value\":\"Value03\"}]"));
        }

        [Test]
        public void testDeserializeStringDictionary()
        {
            String arg = "[{\"Key\":\"Key01\",\"Value\":\"Value01\"},{\"Key\":\"Key02\",\"Value\":\"Value02\"},{\"Key\":\"Key03\",\"Value\":\"Value03\"}]";
            Dictionary<String, String> result = JsonUtils.Deserialize<Dictionary<String, String>>(arg);

            Assert.IsTrue(String.Equals(result["Key01"], "Value01"));
            Assert.IsTrue(String.Equals(result["Key02"], "Value02"));
            Assert.IsTrue(String.Equals(result["Key03"], "Value03"));
        }
    }
}
