﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils.Integration;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Client.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
{
    private Mock<IResponseProvider> responseProviderMock;
    private Mock<IRequestValidator> requestValidatorMock;
    private AsyncServiceScope scope;
    private IJsonRpcClient snakeCaseJsonRpcClient;
    private IJsonRpcClient camelCaseJsonRpcClient;

    public override async Task OneTimeSetup()
    {
        await base.OneTimeSetup();
        responseProviderMock = new Mock<IResponseProvider>();
        requestValidatorMock = new Mock<IRequestValidator>();
        scope = ApplicationFactory.Services.CreateAsyncScope();
        var idGenerator = scope.ServiceProvider.GetRequiredService<IJsonRpcIdGenerator>();

        snakeCaseJsonRpcClient = new SnakeCaseJsonRpcClient(ApiClient, idGenerator);
        camelCaseJsonRpcClient = new CamelCaseJsonRpcClient(ApiClient, idGenerator);
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddTransient(_ => responseProviderMock.Object);
        services.AddTransient(_ => requestValidatorMock.Object);
        services.AddJsonRpcClient<SnakeCaseJsonRpcClient>();
        services.AddJsonRpcClient<CamelCaseJsonRpcClient>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await scope.DisposeAsync();

    [Test]
    public async Task SendNotification_SendRequestWithNullData_SerializeSuccessfully()
    {
        var expectedRequestJson = @"{
    ""method"": ""some-method"",
    ""params"": null,
    ""jsonrpc"": ""2.0""
}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        await camelCaseJsonRpcClient.SendNotification<object>(NotificationUrl, Method, null, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendNotification_SendRequestWithPlainDataCamelCase_SerializeSuccessfully()
    {
        var requestData = TestData.Plain;
        var expectedRequestJson = @"{
    ""method"": ""some-method"",
    ""params"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": null
    },
    ""jsonrpc"": ""2.0""
}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        await camelCaseJsonRpcClient.SendNotification(NotificationUrl, Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendNotification_SendRequestWithPlainDataSnakeCase_SerializeSuccessfully()
    {
        var requestData = TestData.Plain;
        var expectedRequestJson = @"{
    ""method"": ""some-method"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    },
    ""jsonrpc"": ""2.0""
}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        await snakeCaseJsonRpcClient.SendNotification(NotificationUrl, Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendNotification_SendRequestWithNestedDataCamelCase_SerializeSuccessfully()
    {
        var requestData = TestData.Nested;
        var expectedRequestJson = @"{
    ""method"": ""some-method"",
    ""params"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": {
            ""boolField"": true,
            ""stringField"": ""456"",
            ""intField"": 456,
            ""doubleField"": 4.56,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }
    },
    ""jsonrpc"": ""2.0""
}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        await camelCaseJsonRpcClient.SendNotification(NotificationUrl, Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendNotification_SendRequestWithNestedDataSnakeCase_SerializeSuccessfully()
    {
        var requestData = TestData.Nested;
        var expectedRequestJson = @"{
    ""method"": ""some-method"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": {
            ""bool_field"": true,
            ""string_field"": ""456"",
            ""int_field"": 456,
            ""double_field"": 4.56,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    ""jsonrpc"": ""2.0""
}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        await snakeCaseJsonRpcClient.SendNotification(NotificationUrl, Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_SendRequestWithStringId_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
    }

    [Test]
    public async Task SendRequest_SendRequestWithIntId_SerializeSuccessfully()
    {
        var id = 123;
        var expectedRequestJson = @$"{{
    ""id"": {id},
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": {id},
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new NumberRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
    }

    [Test]
    public async Task SendRequest_SendRequestWithNullId_SerializeSuccessfully()
    {
        var expectedRequestJson = @"{
    ""id"": null,
    ""method"": ""some-method"",
    ""params"": {},
    ""jsonrpc"": ""2.0""
}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@"{
    ""id"": null,
    ""result"": {},
    ""jsonrpc"": ""2.0""
}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new NullRpcId(), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
    }

    [Test]
    public async Task SendRequest_SendRequestWithNullData_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": null,
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": null,
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest<object>(RequestUrl, new StringRpcId(id), Method, null, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
        response.GetResponseOrThrow<object?>().Should().BeNull();
    }

    [Test]
    public async Task SendRequest_SendRequestWithPlainDataCamelCase_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
        response.GetResponseOrThrow<TestData>().Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_SendRequestWithPlainDataSnakeCase_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""bool_field"": true,
    ""string_field"": ""123"",
    ""int_field"": 123,
    ""double_field"": 1.23,
    ""enum_field"": ""two"",
    ""nullable_field"": null,
    ""not_required_field"": null,
    ""nested_field"": null
}";
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await snakeCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
        response.GetResponseOrThrow<TestData>().Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_SendRequestWithNestedDataCamelCase_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Nested;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": {
        ""boolField"": true,
        ""stringField"": ""456"",
        ""intField"": 456,
        ""doubleField"": 4.56,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": null
    }
}";
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
        response.GetResponseOrThrow<TestData>().Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_SendRequestWithNestedDataSnakeCase_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Nested;
        var expectedRequestData = @"{
    ""bool_field"": true,
    ""string_field"": ""123"",
    ""int_field"": 123,
    ""double_field"": 1.23,
    ""enum_field"": ""two"",
    ""nullable_field"": null,
    ""not_required_field"": null,
    ""nested_field"": {
        ""bool_field"": true,
        ""string_field"": ""456"",
        ""int_field"": 456,
        ""double_field"": 4.56,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}";
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {expectedRequestData},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await snakeCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, requestData, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
        response.GetResponseOrThrow<TestData>().Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_GetErrorWithCamelCaseData_DeserializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse($@"{{
    ""id"": ""{id}"",
    ""error"": {{
        ""code"": {errorCode},
        ""message"": ""{errorMessage}"",
        ""data"": {{
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }}
    }},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = TestData.Plain;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeTrue();
        var error = response.AsTypedError<TestData>();
        error.Code.Should().Be(errorCode);
        error.Message.Should().Be(errorMessage);
        error.Data.Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_GetErrorWithSnakeCaseData_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse($@"{{
    ""id"": ""{id}"",
    ""error"": {{
        ""code"": {errorCode},
        ""message"": ""{errorMessage}"",
        ""data"": {{
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }}
    }},
    ""jsonrpc"": ""2.0""
}}");
        var expectedResponseData = TestData.Plain;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await snakeCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeTrue();
        var error = response.AsTypedError<TestData>();
        error.Code.Should().Be(errorCode);
        error.Message.Should().Be(errorMessage);
        error.Data.Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendRequest_GetErrorWithoutData_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse($@"{{
    ""id"": ""{id}"",
    ""error"": {{
        ""code"": {errorCode},
        ""message"": ""{errorMessage}""
    }},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await snakeCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeTrue();
        var error = response.AsTypedError<TestData>();
        error.Code.Should().Be(errorCode);
        error.Message.Should().Be(errorMessage);
        error.Data.Should().BeNull();
    }

    [Test]
    public async Task SendRequest_GetResponseWithErrorInResult_DeserializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {{
        ""error"": ""errorValue""
    }},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeFalse();
    }

    [Test]
    public async Task SendRequest_GetResponseWithResultInError_DeserializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""error"": {{
        ""code"": 123,
        ""message"": ""errorMessage"",
        ""data"": {{
            ""result"": ""resultValue""
        }}
    }},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeTrue();
    }

    [Test]
    public async Task SendRequest_GetErrorResponseWithNullId_DeserializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@"{
    ""id"": null,
    ""error"": {
        ""code"": 123,
        ""message"": ""errorMessage""
    },
    ""jsonrpc"": ""2.0""
}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var response = await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError().Should().BeTrue();
    }

    [Test]
    public async Task SendRequest_GetResponseWithoutId_ThrowArgumentException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_GetResponseWithoutResultOrError_ThrowArgumentException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_GetResponseWithBothResultAndError_ThrowArgumentException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""result"": {{}},
    ""error"": {{
        ""code"": 123,
        ""message"": ""errorMessage""
    }},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_GetResponseNot200Code_ThrowJsonRpcException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Throws<ArgumentOutOfRangeException>();

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_GetResponseWithDifferentId_ThrowJsonRpcException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{Guid.NewGuid().ToString()}"",
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendRequest_GetSuccessfulResponseWithNullId_ThrowJsonRpcException()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"{{
    ""id"": ""{id}"",
    ""method"": ""some-method"",
    ""params"": {{}},
    ""jsonrpc"": ""2.0""
}}".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": null,
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var act = async () => await camelCaseJsonRpcClient.SendRequest(RequestUrl, new StringRpcId(id), Method, new { }, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendBatch_BatchWithRequestWithStringId_LinkResponseSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {{}},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id}"",
        ""result"": {{}},
        ""jsonrpc"": ""2.0""
    }}
]");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<object>(new StringRpcId(id), Method, new { })
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id)).Should().BeFalse();
    }

    [Test]
    public async Task SendBatch_BatchWithRequestWithIntId_LinkResponseSuccessfully()
    {
        var id = 123;
        var expectedRequestJson = @$"[
    {{
        ""id"": {id},
        ""method"": ""some-method"",
        ""params"": {{}},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": {id},
        ""result"": {{}},
        ""jsonrpc"": ""2.0""
    }}
]");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<object>(new NumberRpcId(id), Method, new { })
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new NumberRpcId(id)).Should().BeFalse();
    }

    [Test]
    public async Task SendBatch_BatchWithRequestWithNullId_LinkResponseSuccessfully()
    {
        var expectedRequestJson = @"[
    {
        ""id"": null,
        ""method"": ""some-method"",
        ""params"": {},
        ""jsonrpc"": ""2.0""
    }
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@"[
    {
        ""id"": null,
        ""result"": {},
        ""jsonrpc"": ""2.0""
    }
]");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<object>(new NullRpcId(), Method, new { })
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(null).Should().BeFalse();
    }

    [Test]
    public async Task SendBatch_BatchWith1Request_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id}"",
        ""result"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id)).Should().BeFalse();
        response.GetResponseOrThrow<TestData>(new StringRpcId(id)).Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_BatchWithRequests_SerializeSuccessfully()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id1}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id1}"",
        ""result"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""result"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id1), Method, requestData),
            new Request<TestData>(new StringRpcId(id2), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id1)).Should().BeFalse();
        response.HasError(new StringRpcId(id2)).Should().BeFalse();
        response.GetResponseOrThrow<TestData>(new StringRpcId(id1)).Should().BeEquivalentTo(expectedResponseData);
        response.GetResponseOrThrow<TestData>(new StringRpcId(id2)).Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_BatchWith1Notification_SerializeSuccessfully()
    {
        var requestData = TestData.Plain;
        var expectedRequestJson = @"[
    {
        ""method"": ""some-method"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        },
        ""jsonrpc"": ""2.0""
    }
]".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(JsonDocument.Parse("{}"));

        var calls = new List<ICall>()
        {
            new Notification<TestData>(Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.Should().BeNull();
    }

    [Test]
    public async Task SendBatch_BatchWithNotifications_SerializeSuccessfully()
    {
        var requestData = TestData.Plain;
        var expectedRequestJson = @"[
    {
        ""method"": ""some-method"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        },
        ""jsonrpc"": ""2.0""
    },
    {
        ""method"": ""some-method"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        },
        ""jsonrpc"": ""2.0""
    }
]".TrimAllLines();

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });

        var calls = new List<ICall>()
        {
            new Notification<TestData>(Method, requestData),
            new Notification<TestData>(Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.Should().BeNull();
    }

    [Test]
    public async Task SendBatch_BatchWithRequestsAndNotifications_SerializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id}"",
        ""result"": {expectedRequestData}
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id), Method, requestData),
            new Notification<TestData>(Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id)).Should().BeFalse();
        response.GetResponseOrThrow<TestData>(new StringRpcId(id)).Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_GetResponseWith1Error_DeserializeSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id}"",
        ""error"": {{
            ""code"": {errorCode},
            ""message"": ""{errorMessage}"",
            ""data"": {expectedRequestData}
        }},
        ""jsonrpc"": ""2.0""
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id)).Should().BeTrue();
        var error = response.AsTypedError<TestData>(new StringRpcId(id));
        error.Code.Should().Be(errorCode);
        error.Message.Should().Be(errorMessage);
        error.Data.Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_GetResponseWithErrors_DeserializeSuccessfully()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id1}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id1}"",
        ""error"": {{
            ""code"": {errorCode},
            ""message"": ""{errorMessage}"",
            ""data"": {expectedRequestData}
        }},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""error"": {{
            ""code"": {errorCode},
            ""message"": ""{errorMessage}"",
            ""data"": {expectedRequestData}
        }},
        ""jsonrpc"": ""2.0""
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id1), Method, requestData),
            new Request<TestData>(new StringRpcId(id2), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id1)).Should().BeTrue();
        response.HasError(new StringRpcId(id2)).Should().BeTrue();
        var error1 = response.AsTypedError<TestData>(new StringRpcId(id1));
        error1.Code.Should().Be(errorCode);
        error1.Message.Should().Be(errorMessage);
        error1.Data.Should().BeEquivalentTo(expectedResponseData);
        var error2 = response.AsTypedError<TestData>(new StringRpcId(id2));
        error2.Code.Should().Be(errorCode);
        error2.Message.Should().Be(errorMessage);
        error2.Data.Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_GetResponseWithResultsAndErrors_DeserializeSuccessfully()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id1}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{id1}"",
        ""error"": {{
            ""code"": {errorCode},
            ""message"": ""{errorMessage}"",
            ""data"": {expectedRequestData}
        }},
        ""jsonrpc"": ""2.0""
    }},
    {{
        ""id"": ""{id2}"",
        ""result"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]");
        var expectedResponseData = requestData;

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id1), Method, requestData),
            new Request<TestData>(new StringRpcId(id2), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        response.HasError(new StringRpcId(id1)).Should().BeTrue();
        response.HasError(new StringRpcId(id2)).Should().BeFalse();
        var error1 = response.AsTypedError<TestData>(new StringRpcId(id1));
        error1.Code.Should().Be(errorCode);
        error1.Message.Should().Be(errorMessage);
        error1.Data.Should().BeEquivalentTo(expectedResponseData);
        response.GetResponseOrThrow<TestData>(new StringRpcId(id2)).Should().BeEquivalentTo(expectedResponseData);
    }

    [Test]
    public async Task SendBatch_GetSingleErrorResponse_ThrowJsonRpcException()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var responseBody = JsonDocument.Parse(@$"{{
    ""id"": ""{id}"",
    ""error"": {{
        ""code"": {errorCode},
        ""message"": ""{errorMessage}"",
        ""data"": {expectedRequestData}
    }},
    ""jsonrpc"": ""2.0""
}}");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id), Method, requestData)
        };
        var act = async () => await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
    }

    [Test]
    public async Task SendBatch_GetResponseWithDifferentId_ThrowJsonRpcException()
    {
        var id = Guid.NewGuid().ToString();
        var requestData = TestData.Plain;
        var expectedRequestData = @"{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""notRequiredField"": null,
    ""nestedField"": null
}";
        var expectedRequestJson = @$"[
    {{
        ""id"": ""{id}"",
        ""method"": ""some-method"",
        ""params"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]".TrimAllLines();
        var responseBody = JsonDocument.Parse(@$"[
    {{
        ""id"": ""{Guid.NewGuid().ToString()}"",
        ""result"": {expectedRequestData},
        ""jsonrpc"": ""2.0""
    }}
]");

        string actualContentType = null;
        string actualRequestJson = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<HttpRequest>()))
            .Callback<HttpRequest>(request =>
            {
                using var streamReader = new StreamReader(request.Body);
                actualRequestJson = actualRequestJson = streamReader.ReadToEndAsync().Result.TrimAllLines();
                actualContentType = request.ContentType;
            });
        responseProviderMock.Setup(static p => p.GetResponse())
            .Returns(responseBody);

        var calls = new List<ICall>()
        {
            new Request<TestData>(new StringRpcId(id), Method, requestData)
        };
        var response = await camelCaseJsonRpcClient.SendBatch(BatchUrl, calls, CancellationToken.None);

        actualContentType.Should().Contain("application/json");
        actualRequestJson.Should().Be(expectedRequestJson);
        var act = () => response.GetResponseOrThrow<TestData>(new StringRpcId(id));
        act.Should().Throw<JsonRpcException>();
    }

    private const string Method = "some-method";
    private const string NotificationUrl = "notification";
    private const string RequestUrl = "request";
    private const string BatchUrl = "batch";
}
