using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
internal class DeserializationTests
{
    private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer = new();
    private readonly CamelCaseJsonRpcSerializer camelCaseJsonRpcSerializer = new();
    private readonly SnakeCaseJsonRpcSerializer snakeCaseJsonRpcSerializer = new();

    [Test]
    public void BatchResponseFor1Request()
    {
        var json = GetFileContent("BatchResponseFor1Request.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<BatchResponseWrapper>();
        var batchResponse = (BatchResponseWrapper) deserialized;
        batchResponse.Batch.Should().HaveCount(1);
        var response = batchResponse.Batch.Single();
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
    }

    [Test]
    public void BatchResponseForRequests()
    {
        var json = GetFileContent("BatchResponseForRequests.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<BatchResponseWrapper>();
        var batchResponse = (BatchResponseWrapper) deserialized;
        batchResponse.Batch.Should().HaveCount(3);
        foreach (var response in batchResponse.Batch)
        {
            response.Should().BeOfType<UntypedResponse>();
            response.Jsonrpc.Should().Be("2.0");
        }

        batchResponse.Batch[0].Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) batchResponse.Batch[0].Id;
        intId.NumberValue.Should().Be(123);

        batchResponse.Batch[1].Id.Should().BeOfType<StringRpcId>();
        var stringId = (StringRpcId) batchResponse.Batch[1].Id;
        stringId.StringValue.Should().Be("123");

        batchResponse.Batch[2].Id.Should().BeNull();
    }

    [Test]
    public void BatchResponseWith1Error()
    {
        var json = GetFileContent("BatchResponseWith1Error.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<BatchResponseWrapper>();
        var batchResponse = (BatchResponseWrapper) deserialized;
        batchResponse.Batch.Should().HaveCount(1);
        var response = batchResponse.Batch.Single();
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
        var error = (UntypedErrorResponse) response;
        error.Error.Code.Should().Be(456);
        error.Error.Message.Should().Be("errorMessage");
    }

    [Test]
    public void BatchResponseWithErrors()
    {
        var json = GetFileContent("BatchResponseWithErrors.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<BatchResponseWrapper>();
        var batchResponse = (BatchResponseWrapper) deserialized;
        batchResponse.Batch.Should().HaveCount(3);
        foreach (var response in batchResponse.Batch)
        {
            response.Should().BeOfType<UntypedErrorResponse>();
            response.Jsonrpc.Should().Be("2.0");
            var error = (UntypedErrorResponse) response;
            error.Error.Code.Should().Be(456);
            error.Error.Message.Should().Be("errorMessage");
        }

        batchResponse.Batch[0].Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) batchResponse.Batch[0].Id;
        intId.NumberValue.Should().Be(123);

        batchResponse.Batch[1].Id.Should().BeOfType<StringRpcId>();
        var stringId = (StringRpcId) batchResponse.Batch[1].Id;
        stringId.StringValue.Should().Be("123");

        batchResponse.Batch[2].Id.Should().BeNull();
    }

    [Test]
    public void BatchResponseWithWithResultsAndErrors()
    {
        var json = GetFileContent("BatchResponseWithWithResultsAndErrors.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<BatchResponseWrapper>();
        var batchResponse = (BatchResponseWrapper) deserialized;
        batchResponse.Batch.Should().HaveCount(6);
        foreach (var response in batchResponse.Batch.Take(3))
        {
            response.Should().BeOfType<UntypedResponse>();
            response.Jsonrpc.Should().Be("2.0");
        }

        foreach (var response in batchResponse.Batch.Skip(3))
        {
            response.Should().BeOfType<UntypedErrorResponse>();
            response.Jsonrpc.Should().Be("2.0");
            var error = (UntypedErrorResponse) response;
            error.Error.Code.Should().Be(456);
            error.Error.Message.Should().Be("errorMessage");
        }

        batchResponse.Batch[0].Id.Should().BeOfType<NumberRpcId>();
        var intId1 = (NumberRpcId) batchResponse.Batch[0].Id;
        intId1.NumberValue.Should().Be(123);

        batchResponse.Batch[1].Id.Should().BeOfType<StringRpcId>();
        var stringId1 = (StringRpcId) batchResponse.Batch[1].Id;
        stringId1.StringValue.Should().Be("123");

        batchResponse.Batch[2].Id.Should().BeNull();

        batchResponse.Batch[3].Id.Should().BeOfType<NumberRpcId>();
        var intId2 = (NumberRpcId) batchResponse.Batch[3].Id;
        intId2.NumberValue.Should().Be(789);

        batchResponse.Batch[4].Id.Should().BeOfType<StringRpcId>();
        var stringId2 = (StringRpcId) batchResponse.Batch[4].Id;
        stringId2.StringValue.Should().Be("789");

        batchResponse.Batch[5].Id.Should().BeNull();
    }

    [Test]
    public void BatchWith1Notification()
    {
        // TODO server
    }

    [Test]
    public void BatchWith1Request()
    {
        // TODO server
    }

    [Test]
    public void BatchWithNotifications()
    {
        // TODO server
    }

    [Test]
    public void BatchWithNotificationsAndRequests()
    {
        // TODO server
    }

    [Test]
    public void BatchWithRequests()
    {
        // TODO server
    }

    [Test]
    public void CamelCaseErrorResponse()
    {
        var json = GetFileContent("CamelCaseErrorResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
        var error = (UntypedErrorResponse) response;
        error.Error.Code.Should().Be(456);
        error.Error.Message.Should().Be("errorMessage");
        var errorData = error.Error.Data.Deserialize<TestData>(camelCaseJsonRpcSerializer.Settings);
        errorData.BoolField.Should().BeTrue();
        errorData.StringField.Should().Be("123");
        errorData.IntField.Should().Be(123);
        errorData.DoubleField.Should().BeApproximately(1.23, Precision);
        errorData.EnumField.Should().Be(TestEnum.Two);
        errorData.NullableField.Should().BeNull();
        errorData.NotRequiredField.Should().BeNull();
    }

    [Test]
    public void CamelCaseParamsRequest()
    {
        // TODO server
    }

    [Test]
    public void CamelCaseResultResponse()
    {
        var json = GetFileContent("CamelCaseResultResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
        var result = (UntypedResponse) response;
        var resultData = result.Result.Deserialize<TestData>(camelCaseJsonRpcSerializer.Settings);
        resultData.BoolField.Should().BeTrue();
        resultData.StringField.Should().Be("123");
        resultData.IntField.Should().Be(123);
        resultData.DoubleField.Should().BeApproximately(1.23, Precision);
        resultData.EnumField.Should().Be(TestEnum.Two);
        resultData.NullableField.Should().BeNull();
        resultData.NotRequiredField.Should().BeNull();
    }

    [Test]
    public void IntIdErrorResponse()
    {
        var json = GetFileContent("IntIdErrorResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
    }

    [Test]
    public void IntIdRequest()
    {
        // TODO server
    }

    [Test]
    public void IntIdResponse()
    {
        var json = GetFileContent("IntIdResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
    }

    [Test]
    public void Notification()
    {
        // TODO server
    }

    [Test]
    public void NullIdErrorResponse()
    {
        var json = GetFileContent("NullIdErrorResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeNull();
    }

    [Test]
    public void NullIdRequest()
    {
        // TODO server
    }

    [Test]
    public void NullIdResponse()
    {
        var json = GetFileContent("NullIdResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeNull();
    }

    [Test]
    public void SnakeCaseErrorResponse()
    {
        var json = GetFileContent("SnakeCaseErrorResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
        var error = (UntypedErrorResponse) response;
        error.Error.Code.Should().Be(456);
        error.Error.Message.Should().Be("errorMessage");
        var errorData = error.Error.Data.Deserialize<TestData>(snakeCaseJsonRpcSerializer.Settings);
        errorData.BoolField.Should().BeTrue();
        errorData.StringField.Should().Be("123");
        errorData.IntField.Should().Be(123);
        errorData.DoubleField.Should().BeApproximately(1.23, Precision);
        errorData.EnumField.Should().Be(TestEnum.Two);
        errorData.NullableField.Should().BeNull();
        errorData.NotRequiredField.Should().BeNull();
    }

    [Test]
    public void SnakeCaseParamsRequest()
    {
        // TODO server
    }

    [Test]
    public void SnakeCaseResultResponse()
    {
        var json = GetFileContent("SnakeCaseResultResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<NumberRpcId>();
        var intId = (NumberRpcId) response.Id;
        intId.NumberValue.Should().Be(123);
        var result = (UntypedResponse) response;
        var resultData = result.Result.Deserialize<TestData>(snakeCaseJsonRpcSerializer.Settings);
        resultData.BoolField.Should().BeTrue();
        resultData.StringField.Should().Be("123");
        resultData.IntField.Should().Be(123);
        resultData.DoubleField.Should().BeApproximately(1.23, Precision);
        resultData.EnumField.Should().Be(TestEnum.Two);
        resultData.NullableField.Should().BeNull();
        resultData.NotRequiredField.Should().BeNull();
    }

    [Test]
    public void StringIdErrorResponse()
    {
        var json = GetFileContent("StringIdErrorResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedErrorResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<StringRpcId>();
        var intId = (StringRpcId) response.Id;
        intId.StringValue.Should().Be("123");
    }

    [Test]
    public void StringIdRequest()
    {
        // TODO server
    }

    [Test]
    public void StringIdResponse()
    {
        var json = GetFileContent("StringIdResponse.json");

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headerJsonRpcSerializer.Settings);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SingleResponseWrapper>();
        var singleResponse = (SingleResponseWrapper) deserialized;
        var response = singleResponse.Single;
        response.Should().BeOfType<UntypedResponse>();
        response.Jsonrpc.Should().Be("2.0");
        response.Id.Should().BeOfType<StringRpcId>();
        var intId = (StringRpcId) response.Id;
        intId.StringValue.Should().Be("123");
    }

    private static string GetFileContent(string fileName) => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", fileName));
    private const double Precision = 0.001;
}
