using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of request being single or batch
    /// </summary>
    public class RequestWrapperConverter : JsonConverter<IRequestWrapper>
    {
        public override void Write(Utf8JsonWriter writer, IRequestWrapper value, JsonSerializerOptions options)
        {
            // NOTE: used in server to parse requests, no need for serialization
            throw new InvalidOperationException();
        }

        public override IRequestWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            return tokenType switch
            {
                JsonTokenType.StartObject => new SingleRequestWrapper { Call = JsonSerializer.Deserialize<IUntypedCall>(ref reader, options)! },
                JsonTokenType.StartArray => new BatchRequestWrapper { Batch = JsonSerializer.Deserialize<List<IUntypedCall>>(ref reader, options)! },
                _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Expected {} or [] as root element")
            };
        }
    }
}
