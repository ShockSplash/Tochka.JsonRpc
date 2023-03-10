using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class CamelCaseJsonRpcSerializer : IJsonRpcSerializer
    {
        public JsonSerializerOptions Settings => SettingsInstance;

        private static readonly JsonSerializerOptions SettingsInstance = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            WriteIndented = true
        };
    }
}
