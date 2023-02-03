using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class JsonRpcIdConverterTests
    {
        private JsonRpcIdConverter jsonRpcIdConverter;
        private MemoryStream writerOutput;
        private Utf8JsonWriter writer;

        [SetUp]
        public void Setup()
        {
            jsonRpcIdConverter = new JsonRpcIdConverter();
            writerOutput = new MemoryStream();
            writer = new Utf8JsonWriter(writerOutput);
        }

        [Test]
        public void Test_WriteJson_WorksForNumber()
        {
            var id = new NumberRpcId(42);

            jsonRpcIdConverter.Write(writer, id, new JsonSerializerOptions());

            GetWriteResult().Should().Be("42");
        }

        private string GetWriteResult()
        {
            writer.Flush();
            return Encoding.UTF8.GetString(writerOutput.ToArray());
        }

        [Test]
        public void Test_WriteJson_WorksForString()
        {
            var id = new StringRpcId("test");

            jsonRpcIdConverter.Write(writer, id, new JsonSerializerOptions());

            GetWriteResult().Should().Be("\"test\"");
        }

        [Test]
        public void Test_WriteJson_WorksForNull()
        {
            IRpcId id = null;

            jsonRpcIdConverter.Write(writer, id, new JsonSerializerOptions());

            GetWriteResult().Should().Be("null");
        }

        [TestCase("")]
        [TestCase("test")]
        public void Test_ReadJson_ReturnsStringIdForString(string value)
        {
            var json = $@"""{value}""";

            var reader = CreateJsonReader(json);
            var result = jsonRpcIdConverter.Read(ref reader, Mock.Of<Type>(), new JsonSerializerOptions());

            result.Should().BeOfType<StringRpcId>()
                .Subject.String.Should().Be(value);
        }

        [TestCase(0)]
        [TestCase(42)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void Test_ReadJson_ReturnsNumberIdForNumber(int value)
        {
            var json = JsonSerializer.Serialize(value);

            var reader = CreateJsonReader(json);
            var result = jsonRpcIdConverter.Read(ref reader, Mock.Of<Type>(), new JsonSerializerOptions());

            result.Should().BeOfType<NumberRpcId>()
                .Subject.Number.Should().Be(value);
        }

        [TestCase(0L)]
        [TestCase(42L)]
        [TestCase(-1L)]
        [TestCase(long.MaxValue)]
        public void Test_ReadJson_ReturnsNumberIdForNumber(long value)
        {
            var json = JsonSerializer.Serialize(value);

            var reader = CreateJsonReader(json);
            var result = jsonRpcIdConverter.Read(ref reader, Mock.Of<Type>(), new JsonSerializerOptions());

            result.Should().BeOfType<NumberRpcId>()
                .Subject.Number.Should().Be(value);
        }

        [Test]
        public void Test_ReadJson_ReturnsNullForNull()
        {
            var json = "null";

            var reader = CreateJsonReader(json);
            var result = jsonRpcIdConverter.Read(ref reader, Mock.Of<Type>(), new JsonSerializerOptions());

            result.Should().BeNull();
        }

        [TestCaseSource(typeof(JsonRpcIdConverterTests), nameof(BadJsonIdCases))]
        public void Test_ReadJson_ThrowsOnBadIdProperty(string json)
        {
            Action action = () =>
            {
                var reader = CreateJsonReader(json);
                jsonRpcIdConverter.Read(ref reader, Mock.Of<Type>(), new JsonSerializerOptions());
            };

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static IEnumerable BadJsonIdCases => BadJsonIds.Select(data => new TestCaseData(data));

        private static IEnumerable<string> BadJsonIds
        {
            get
            {
                // Other possible values from ECMA-404, Section 5:
                yield return "{}";
                yield return "[]";
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
