using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class SnakeCaseJsonRpcSerializer : IJsonRpcSerializer
    {
        public JsonSerializerOptions Settings => SettingsInstance;
        // public JsonSerializer Serializer => SerializerInstance;

        private static readonly JsonSerializerOptions SettingsInstance = new()
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            Converters =
            {
                new JsonStringEnumConverter(new SnakeCaseNamingPolicy()),
            },
            WriteIndented = true
        };

        // private static readonly JsonSerializer SerializerInstance = JsonSerializer.Create(SettingsInstance);
    }
}
