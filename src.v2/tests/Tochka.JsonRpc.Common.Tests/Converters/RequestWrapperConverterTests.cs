using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Tests.Helpers;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class RequestWrapperConverterTests
    {
        private RequestWrapperConverter requestWrapperConverter;

        [SetUp]
        public void Setup()
        {
            requestWrapperConverter = new RequestWrapperConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            var utf8JsonWriter = new Utf8JsonWriter(new MemoryStream());
            Action action = () => requestWrapperConverter.Write(utf8JsonWriter, Mock.Of<IRequestWrapper>(), new JsonSerializerOptions());

            action.Should().Throw<InvalidOperationException>();
        }

        //TODO: server
        // [Test]
        // public void Test_ReadJson_ReturnsSingleForObject()
        // {
        //     var json = "{}";
        //     var options = new JsonSerializerOptions
        //     {
        //         Converters =
        //         {
        //             new MockCallConverter()
        //         }
        //     };
        //
        //     var result = requestWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);
        //
        //     result.Should().BeOfType<SingleRequestWrapper>()
        //         .Subject.Call.Should().NotBeNull();
        // }

        //TODO: server
        // [Test]
        // public void Test_ReadJson_ReturnsBatchForArray()
        // {
        //     var jArray = new JArray
        //     {
        //         new JObject()
        //     };
        //     var json = jArray.ToString();
        //     var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        //     {
        //         Converters = new List<JsonConverter>()
        //         {
        //             new MockCallConverter()
        //         }
        //     });
        //
        //     var result = requestWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);
        //
        //     result.Should().BeOfType<BatchRequestWrapper>();
        //     var batch = result as BatchRequestWrapper;
        //     batch.Batch.Should().HaveCount(1);
        //     batch.Batch[0].Should().Be(Mock.Get(batch.Batch[0]).Object);
        // }

        [TestCaseSource(typeof(RequestWrapperConverterTests), nameof(BadJsonCases))]
        public void Test_ReadJson_ThrowsOnBadJson(string json)
        {
            Action action = () =>
            {
                var reader = CreateJsonReader(json);
                requestWrapperConverter.Read(ref reader, typeof(IRpcId), new JsonSerializerOptions());
            };

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static IEnumerable BadJsonCases => BadJson.Select(data => new TestCaseData(data));

        private static IEnumerable<string> BadJson
        {
            get
            {
                // Other possible values from ECMA-404, Section 5:
                yield return "null";
                yield return @"""""";
                yield return @"""test""";
                yield return "0";
                yield return "0.1";
                yield return "true";
                yield return "false";
            }
        }

        private static Utf8JsonReader CreateJsonReader(string json)
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();
            return reader;
        }
    }
}
