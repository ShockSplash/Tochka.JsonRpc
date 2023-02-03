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
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Tests.Helpers;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class ResponseWrapperConverterTests
    {
        private ResponseWrapperConverter responseWrapperConverter;

        [SetUp]
        public void Setup()
        {
            responseWrapperConverter = new ResponseWrapperConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            var utf8JsonWriter = new Utf8JsonWriter(new MemoryStream());
            Action action = () => responseWrapperConverter.Write(utf8JsonWriter, Mock.Of<IResponseWrapper>(), new JsonSerializerOptions());

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_ReadJson_ReturnsSingleForObject()
        {
            var json = "{}";
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new MockResponseConverter()
                }
            };

            var reader = CreateJsonReader(json);
            var result = responseWrapperConverter.Read(ref reader, Mock.Of<Type>(), options);

            result.Should().BeOfType<SingleResponseWrapper>()
                .Subject.Single.Should().NotBeNull();
        }

        [Test]
        public void Test_ReadJson_ReturnsBatchForArray()
        {
            var json = "[{}]";
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new MockResponseConverter()
                }
            };

            var reader = CreateJsonReader(json);
            var result = responseWrapperConverter.Read(ref reader, Mock.Of<Type>(), options);

            result.Should().BeOfType<BatchResponseWrapper>();
            var batch = result as BatchResponseWrapper;
            batch.Batch.Should().HaveCount(1);
            batch.Batch[0].Should().Be(Mock.Get(batch.Batch[0]).Object);
        }

        [TestCaseSource(typeof(ResponseWrapperConverterTests), nameof(BadJsonCases))]
        public void Test_ReadJson_ThrowsOnBadJson(string json)
        {
            Action action = () =>
            {
                var reader = CreateJsonReader(json);
                responseWrapperConverter.Read(ref reader, typeof(IRpcId), new JsonSerializerOptions());
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
