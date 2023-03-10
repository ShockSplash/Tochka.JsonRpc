using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Converters;

namespace Tochka.JsonRpc.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class HeaderJsonRpcSerializer : IJsonRpcSerializer
    {
        public JsonSerializerOptions Settings => SettingsInstance;

        private static readonly JsonSerializerOptions SettingsInstance = new()
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            Converters =
            {
                // TODO: server
                // new RequestWrapperConverter(),
                // new CallConverter(),

                new JsonRpcIdConverter(),

                // responses
                new ResponseWrapperConverter(),
                new ResponseConverter()
            },
            WriteIndented = true
        };
    }
}
