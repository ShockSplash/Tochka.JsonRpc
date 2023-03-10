using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests;

public class JsonRpcClientBaseTests
{
    private Mock<JsonRpcClientBase> clientMock;
    private TestEnvironment testEnvironment;
    private Mock<IJsonRpcSerializer> serializerMock;
    private Mock<IJsonRpcIdGenerator> generatorMock;
    private Mock<JsonRpcClientOptionsBase> optionsMock;
    private MockHttpMessageHandler handlerMock;

    [SetUp]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        serializerMock = new Mock<IJsonRpcSerializer>();
        serializerMock.Setup(x => x.Settings)
            .Returns(new JsonSerializerOptions());
        optionsMock = new Mock<JsonRpcClientOptionsBase>
        {
            CallBase = true
        };
        optionsMock.Object.Url = BaseUrl;
        generatorMock = new Mock<IJsonRpcIdGenerator>();
        testEnvironment = new TestEnvironment();
        clientMock = new Mock<JsonRpcClientBase>(handlerMock.ToHttpClient(), serializerMock.Object, new HeaderJsonRpcSerializer(), optionsMock.Object, generatorMock.Object, testEnvironment.ServiceProvider.GetRequiredService<ILogger<JsonRpcClientBase>>())
        {
            CallBase = true
        };
    }

    [Test]
    public void Test_UserAgent_DefaultValue() => clientMock.Object.UserAgent.Should().Be("Tochka.JsonRpc.Client");

    [Test]
    public void Test_Encoding_DefaultValue() => clientMock.Object.Encoding.Should().Be(Encoding.UTF8);

    [Test]
    public void Test_Constructor_InitializesHttpClient()
    {
        var options = optionsMock.Object;

        // Needed to call constructor
        var client = clientMock.Object;

        var httpClient = client.Client;
        httpClient.BaseAddress.Should().BeEquivalentTo(new Uri(options.Url, UriKind.Absolute));
        httpClient.DefaultRequestHeaders.Should().ContainKey("User-Agent");
        httpClient.DefaultRequestHeaders.UserAgent.ToString().Should().Be(client.UserAgent);
        httpClient.Timeout.Should().Be(options.Timeout);
    }

    [Test]
    public async Task Test_SendNotification1_ChainsToInternalMethod()
    {
        var url = "test";
        var notification = new Notification<object>();
        clientMock.Setup(x => x.SendNotificationInternal(url, notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(url, notification, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendNotification2_ChainsToInternalMethod()
    {
        var notification = new Notification<object>();
        clientMock.Setup(x => x.SendNotificationInternal(null, notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(notification, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendNotification3_ChainsToInternalMethod()
    {
        var url = "test";
        var method = "method";
        var parameters = new object();

        clientMock.Setup(x => x.SendNotificationInternal(url, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(url, method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendNotification4_ChainsToInternalMethod()
    {
        var method = "method";
        var parameters = new object();

        clientMock.Setup(x => x.SendNotificationInternal(null, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest1_ChainsToInternalMethod()
    {
        var url = "test";
        var request = new Request<object>();
        clientMock.Setup(x => x.SendRequestInternal(url, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(url, request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest2_ChainsToInternalMethod()
    {
        var request = new Request<object>();
        clientMock.Setup(x => x.SendRequestInternal(null, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest3_ChainsToInternalMethod()
    {
        var url = "test";
        var method = "method";
        var parameters = new object();
        var id = Mock.Of<IRpcId>();
        clientMock.Setup(x => x.SendRequestInternal(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(url, id, method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest4_ChainsToInternalMethod()
    {
        var method = "method";
        var parameters = new object();
        var id = Mock.Of<IRpcId>();
        clientMock.Setup(x => x.SendRequestInternal(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(id, method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest5_ChainsToInternalMethod()
    {
        var url = "test";
        var method = "method";
        var parameters = new object();
        var id = Mock.Of<IRpcId>();
        clientMock.Setup(x => x.SendRequestInternal(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();
        generatorMock.Setup(x => x.GenerateId())
            .Returns(id)
            .Verifiable();

        await clientMock.Object.SendRequest(url, method, parameters, new CancellationToken());

        clientMock.Verify();
        generatorMock.Verify();
    }

    [Test]
    public async Task Test_SendRequest6_ChainsToInternalMethod()
    {
        var method = "method";
        var parameters = new object();
        var id = Mock.Of<IRpcId>();
        clientMock.Setup(x => x.SendRequestInternal(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();
        generatorMock.Setup(x => x.GenerateId())
            .Returns(id)
            .Verifiable();

        await clientMock.Object.SendRequest(method, parameters, new CancellationToken());

        clientMock.Verify();
        generatorMock.Verify();
    }

    [Test]
    public async Task Test_SendBatch1_ChainsToInternalMethod()
    {
        var url = "test";
        var batch = new List<ICall>();
        clientMock.Setup(x => x.SendBatchInternal(url, batch, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IBatchJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendBatch(url, batch, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_SendBatch2_ChainsToInternalMethod()
    {
        var batch = new List<ICall>();
        clientMock.Setup(x => x.SendBatchInternal(null, batch, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IBatchJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendBatch(batch, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_Send1_ChainsToInternalMethod()
    {
        var url = "test";
        var request = new Request<object>();
        clientMock.Setup(x => x.SendInternal(url, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(url, request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Test_Send2_ChainsToInternalMethod()
    {
        var request = new Request<object>();
        clientMock.Setup(x => x.SendInternal(null, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendNotificationInternal_PostContentToRequestUrl()
    {
        var notification = new Notification<object> { Params = new object() };
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        await clientMock.Object.SendNotificationInternal(RequestUrl, notification, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendNotificationInternal_FillContext()
    {
        var notification = new Notification<object> { Params = new object() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));

        await clientMock.Object.SendNotificationInternal(RequestUrl, notification, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithSingle(It.IsAny<IUntypedCall>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
    }

    [Test]
    public async Task SendRequestInternal_PostContentToRequestUrlAndParseResponse()
    {
        const string responseContent = "response-content";
        var request = new Request<object> { Params = new object() };
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(Mock.Of<IJsonRpcCallContext>());
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new SingleResponseWrapper())
            .Verifiable();

        await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
        clientMock.Verify();
    }

    [Test]
    public async Task SendRequestInternal_FillContext()
    {
        const string responseContent = "response-content";
        var request = new Request<object> { Params = new object() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        var singleResponse = Mock.Of<IResponse>();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new SingleResponseWrapper { Single = singleResponse })
            .Verifiable();

        await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithSingle(It.IsAny<IUntypedCall>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
        contextMock.Verify(c => c.WithHttpContent(response.Content, responseContent));
        contextMock.Verify(c => c.WithSingleResponse(singleResponse));
    }

    [Test]
    public async Task SendRequestInternal_ThrowOnBatchResponse()
    {
        const string responseContent = "response-content";
        var request = new Request<object> { Params = new object() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new BatchResponseWrapper())
            .Verifiable();

        var act = async () => await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithoutRequests_PostContentToRequestUrl()
    {
        var batch = new List<ICall> { new Notification<object>() };
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        var response = await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        response.Should().BeNull();
        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithRequests_PostContentToRequestUrlAndParseResponse()
    {
        const string responseContent = "response-content";
        var batch = new List<ICall> { new Request<object>() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var batchResponse = new List<IResponse> { Mock.Of<IResponse>() };
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new BatchResponseWrapper { Batch = batchResponse })
            .Verifiable();

        var response = await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        response.Should().NotBeNull();
        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
        clientMock.Verify();
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithoutRequests_FillContext()
    {
        var batch = new List<ICall> { new Request<object>() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));

        await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithBatch(It.IsAny<List<IUntypedCall>>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithRequests_FillContextWithResponse()
    {
        const string responseContent = "response-content";
        var batch = new List<ICall> { new Request<object>() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        var batchResponse = new List<IResponse> { Mock.Of<IResponse>() };
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new BatchResponseWrapper { Batch = batchResponse })
            .Verifiable();

        await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithBatch(It.IsAny<List<IUntypedCall>>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
        contextMock.Verify(c => c.WithHttpContent(response.Content, responseContent));
        contextMock.Verify(c => c.WithBatchResponse(batchResponse));
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithRequests_ThrowOnSingleResponseWithFilledContext()
    {
        const string responseContent = "response-content";
        var batch = new List<ICall> { new Request<object>() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var singleResponse = Mock.Of<IResponse>();
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(new SingleResponseWrapper { Single = singleResponse })
            .Verifiable();

        var act = async () => await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
        contextMock.Verify(c => c.WithSingleResponse(singleResponse));
    }

    [Test]
    public async Task SendBatchInternal_SendBatchWithRequests_ThrowOnUnknownResponse()
    {
        const string responseContent = "response-content";
        var request = new Request<object> { Params = new object() };
        var contextMock = new Mock<IJsonRpcCallContext>();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(responseContent))
            .Returns(Mock.Of<IResponseWrapper>())
            .Verifiable();

        var act = async () => await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
    }

    [Test]
    public async Task SendInternal_PostContentToRequestUrl()
    {
        var call = Mock.Of<ICall>();
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        await clientMock.Object.SendInternal(RequestUrl, call, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    private const string BaseUrl = "http://foo.bar/";
    private const string RequestUrl = "request-url";
    private const string PostUrl = $"{BaseUrl}{RequestUrl}";
}
