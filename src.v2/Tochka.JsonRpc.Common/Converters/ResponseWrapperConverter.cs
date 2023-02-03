using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of response being single on some batch errors
    /// </summary>
    public class ResponseWrapperConverter : JsonConverter<IResponseWrapper>
    {
        public override void Write(Utf8JsonWriter writer, IResponseWrapper value, JsonSerializerOptions options) => throw
            // NOTE: used in client to parse responses, no need for serialization
            new InvalidOperationException();

        public override IResponseWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            return tokenType switch
            {
                JsonTokenType.StartObject => new SingleResponseWrapper { Single = JsonSerializer.Deserialize<IResponse>(ref reader, options)! },
                JsonTokenType.StartArray => new BatchResponseWrapper { Batch = JsonSerializer.Deserialize<List<IResponse>>(ref reader, options)! },
                _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Expected {} or [] as root element")
            };
        }
    }
}
