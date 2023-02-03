using System;
using System.Text.Json;
using Moq;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Common.Tests.Helpers
{
    public class MockResponseConverter : ResponseConverter
    {
        public override IResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDocument.ParseValue(ref reader);
            return Mock.Of<IResponse?>();
        }

        // public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions serializer)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public override object Read(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        // {
        //     JToken.Load(reader);
        //     return Mock.Of<IResponse>();
        // }
    }
}
