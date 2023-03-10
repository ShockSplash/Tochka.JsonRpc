using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client;

/// <summary>
/// Base class for JSON Rpc clients
/// </summary>
[PublicAPI]
public abstract class JsonRpcClientBase : IJsonRpcClient
{
    /// <summary>
    /// Default is "Tochka.JsonRpc.Client"
    /// </summary>
    public virtual string UserAgent => typeof(JsonRpcClientBase).Namespace!;

    /// <inheritdoc />
    public IJsonRpcSerializer Serializer { get; }

    protected internal virtual Encoding Encoding => Encoding.UTF8;
    protected internal HttpClient Client { get; }

    protected HeaderJsonRpcSerializer JsonRpcSerializer { get; }
    protected ILogger Log { get; }
    protected IJsonRpcIdGenerator RpcIdGenerator { get; }

    protected internal JsonRpcClientBase(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, JsonRpcClientOptionsBase options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log)
    {
        Client = client;
        Serializer = serializer;
        JsonRpcSerializer = headerJsonRpcSerializer;
        RpcIdGenerator = jsonRpcIdGenerator;
        Log = log;
        InitializeClient(client, options);
    }

    /// <inheritdoc />
    public virtual async Task SendNotification<T>(string requestUrl, Notification<T> notification, CancellationToken cancellationToken) =>
        await SendNotificationInternal(requestUrl, notification, cancellationToken);

    /// <inheritdoc />
    public async Task SendNotification<T>(Notification<T> notification, CancellationToken cancellationToken) =>
        await SendNotificationInternal(null, notification, cancellationToken);

    /// <inheritdoc />
    public async Task SendNotification<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
    {
        var notification = new Notification<T>
        {
            Method = method,
            Params = parameters
        };
        await SendNotificationInternal(requestUrl, notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendNotification<T>(string method, T parameters, CancellationToken cancellationToken)
    {
        var notification = new Notification<T>
        {
            Method = method,
            Params = parameters
        };
        await SendNotificationInternal(null, notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, Request<T> request, CancellationToken cancellationToken) =>
        await SendRequestInternal(requestUrl, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(Request<T> request, CancellationToken cancellationToken) =>
        await SendRequestInternal(null, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        var request = new Request<T>
        {
            Id = id,
            Method = method,
            Params = parameters
        };
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(string method, T parameters, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        var request = new Request<T>
        {
            Id = id,
            Method = method,
            Params = parameters
        };
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, IRpcId id, string method, T parameters, CancellationToken cancellationToken)
    {
        var request = new Request<T>
        {
            Id = id,
            Method = method,
            Params = parameters
        };
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<T>(IRpcId id, string method, T parameters, CancellationToken cancellationToken)
    {
        var request = new Request<T>
        {
            Id = id,
            Method = method,
            Params = parameters
        };
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult?> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(requestUrl, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult?> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(null, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken) =>
        await SendInternal(requestUrl, call, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken) =>
        await SendInternal(null, call, cancellationToken);

    internal virtual async Task SendNotificationInternal<T>(string? requestUrl, Notification<T> notification, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = notification.WithSerializedParams(Serializer);
        context.WithSingle(data);
        using var content = CreateHttpContent(data);
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
    }

    internal virtual async Task<ISingleJsonRpcResult> SendRequestInternal<T>(string? requestUrl, Request<T> request, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = request.WithSerializedParams(Serializer);
        context.WithSingle(data);
        using var content = CreateHttpContent(data);
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case SingleResponseWrapper singleResponseWrapper:
                context.WithSingleResponse(singleResponseWrapper.Single);
                Log.LogTrace("Request id [{requestId}]: success", request.Id);
                return new SingleJsonRpcResult(context, JsonRpcSerializer, Serializer);
            default:
                var message = $"Expected single response, got [{responseWrapper}]";
                Log.LogTrace("Request id [{requestId}] failed: {errorMessage}", request.Id, message);
                throw new JsonRpcException(message, context);
        }
    }

    internal virtual async Task<IBatchJsonRpcResult?> SendBatchInternal(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = calls.Select(x => x.WithSerializedParams(Serializer)).ToList();
        context.WithBatch(data);
        using var content = CreateHttpContent(data);
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        if (context.ExpectedBatchResponseCount == 0)
        {
            // "If there are no Response objects contained within the Response array as it is to be sent to the client,
            // the server MUST NOT return an empty Array and should return nothing at all."
            Log.LogTrace("Batch count [{batchCount}] success: no response expected", data.Count);
            return null;
        }

        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case BatchResponseWrapper batchResponseWrapper:
                context.WithBatchResponse(batchResponseWrapper.Batch);
                Log.LogTrace("Batch count [{batchCount}] success: response count [{responseCount}]", data.Count, batchResponseWrapper.Batch.Count);
                return new BatchJsonRpcResult(context, JsonRpcSerializer, Serializer);
            case SingleResponseWrapper singleResponseWrapper:
                // "If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value,
                // the response from the Server MUST be a single Response object."
                context.WithSingleResponse(singleResponseWrapper.Single);
                var message1 = $"Expected batch response, got single, id [{singleResponseWrapper.Single.Id}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Count, message1);
                throw new JsonRpcException(message1, context);
            default:
                var message2 = $"Expected batch response, got [{responseWrapper?.GetType().Name}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Count, message2);
                throw new JsonRpcException(message2, context);
        }
    }

    internal virtual async Task<HttpResponseMessage> SendInternal(string? requestUrl, ICall call, CancellationToken cancellationToken)
    {
        var data = call.WithSerializedParams(Serializer);
        using var content = CreateHttpContent(data);
        return await Client.PostAsync(requestUrl, content, cancellationToken);
    }

    /// <summary>
    /// Parse single or batch response from http content string
    /// </summary>
    /// <param name="contentString"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    internal virtual IResponseWrapper? ParseBody(string contentString) =>
        JsonSerializer.Deserialize<IResponseWrapper>(contentString, JsonRpcSerializer.Settings);

    [ExcludeFromCodeCoverage]
    internal virtual IJsonRpcCallContext CreateContext() => new JsonRpcCallContext();

    /// <summary>
    /// Serialize data to JSON body, set Content-Type
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected internal virtual HttpContent CreateHttpContent(object data)
    {
        var body = JsonSerializer.Serialize(data, JsonRpcSerializer.Settings);
        return new StringContent(body, Encoding, JsonRpcConstants.ContentType);
    }

    [ExcludeFromCodeCoverage]
    protected internal virtual async Task<string> GetContent(HttpContent content, CancellationToken cancellationToken) =>
        await content.ReadAsStringAsync(cancellationToken);

    /// <summary>
    /// Set client properties from base options
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    private void InitializeClient(HttpClient client, JsonRpcClientOptionsBase options)
    {
        client.BaseAddress = new Uri(options.Url, UriKind.Absolute);
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.Timeout = options.Timeout;
        Log.LogTrace("Client initialized: url {baseUrl}, user-agent {userAgent}, timeout {timeout}s", client.BaseAddress, client.DefaultRequestHeaders.UserAgent, client.Timeout.TotalSeconds);
    }
}
