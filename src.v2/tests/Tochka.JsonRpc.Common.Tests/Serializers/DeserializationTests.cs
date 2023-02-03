using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class DeserializationTests
    {
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer = new HeaderJsonRpcSerializer();

        [TestCaseSource(typeof(JsonResponses), nameof(JsonResponses.Cases))]
        [TestCaseSource(typeof(JsonErrorResponses), nameof(JsonErrorResponses.Cases))]
        public void Test_Deserialize_Response(string responseString, object expected)
        {
            var result = JsonSerializer.Deserialize(responseString, expected.GetType(), headerJsonRpcSerializer.Settings);
            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected, opt => opt.ComparingByMembers<JsonElement>());
        }
    }
}
