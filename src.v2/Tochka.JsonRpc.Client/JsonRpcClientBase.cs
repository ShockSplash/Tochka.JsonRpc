using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client
{
    /// <summary>
    /// Base class for JSON Rpc clients
    /// </summary>
    public abstract class JsonRpcClientBase : IJsonRpcClient
    {
        protected HeaderJsonRpcSerializer JsonRpcSerializer { get; }
        protected ILogger Log { get; }
        protected HttpClient Client { get; }
        protected IJsonRpcIdGenerator RpcIdGenerator { get; }

        protected internal virtual Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Default is "Tochka.JsonRpc.Client"
        /// </summary>
        public virtual string UserAgent => typeof(JsonRpcClientBase).Namespace!;

        [SuppressMessage("Usage", "CA2214:Не вызывайте переопределяемые методы в конструкторах")]
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
        public virtual async Task SendNotification<T>(string requestUrl, Notification<T> notification, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = notification.WithSerializedParams(Serializer);
            context.WithSingle(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(Notification<T> notification, CancellationToken cancellationToken)
        {
            await SendNotification(null, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
        {
            var notification = new Notification<T>
            {
                Method = method,
                Params = parameters
            };
            await SendNotification(requestUrl, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(string method, T parameters, CancellationToken cancellationToken)
        {
            var notification = new Notification<T>
            {
                Method = method,
                Params = parameters
            };
            await SendNotification(null, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, Request<T> request, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = request.WithSerializedParams(Serializer);
            context.WithSingle(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync((string) null, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
            var contentString = await GetContent(httpResponseMessage.Content);
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

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(Request<T> request, CancellationToken cancellationToken)
        {
            return await SendRequest(null, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
        {
            var id = RpcIdGenerator.GenerateId();
            var request = new Request<T>
            {
                Id = id,
                Method = method,
                Params = parameters
            };
            return await SendRequest(requestUrl, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string method, T parameters, CancellationToken cancellationToken)
        {
            return await SendRequest((string) null, method, parameters, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, IRpcId id, string method, T parameters, CancellationToken cancellationToken)
        {
            var request = new Request<T>
            {
                Id = id,
                Method = method,
                Params = parameters
            };
            return await SendRequest(requestUrl, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(IRpcId id, string method, T parameters, CancellationToken cancellationToken)
        {
            return await SendRequest(null, id, method, parameters, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IBatchJsonRpcResult> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = calls.Select(x => x.WithSerializedParams(Serializer)).ToList();
            context.WithBatch(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
            if (context.ExpectedBatchResponseCount == 0)
            {
                // "If there are no Response objects contained within the Response array as it is to be sent to the client,
                // the server MUST NOT return an empty Array and should return nothing at all."
                Log.LogTrace("Batch count [{batchCount}] success: no response expected", data.Count);
                return null;
            }

            var contentString = await GetContent(httpResponseMessage.Content);
            context.WithHttpContent(httpResponseMessage.Content, contentString);
            var responseWrapper = ParseBody(contentString);
            switch (responseWrapper)
            {
                case BatchResponseWrapper batchResponseWrapper:
                    context.WithBatchResponse(batchResponseWrapper.Batch);
                    Log.LogTrace("Batch count [{batchCount}] success: response count {responseCount}", data.Count, batchResponseWrapper.Batch.Count);
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

        /// <inheritdoc />
        public virtual async Task<IBatchJsonRpcResult> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken)
        {
            return await SendBatch(null, calls, cancellationToken);
        }

        /// <inheritdoc />
        [SuppressMessage("Naming", "CA1716:Идентификаторы не должны совпадать с ключевыми словами", Justification = "call")]
        public virtual async Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken)
        {
            var data = call.WithSerializedParams(Serializer);
            var content = CreateHttpContent(data);
            return await Client.PostAsync(requestUrl, content, cancellationToken);
        }

        /// <inheritdoc />
        [SuppressMessage("Naming", "CA1716:Идентификаторы не должны совпадать с ключевыми словами", Justification = "call")]
        public virtual async Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken)
        {
            return await Send(null, call, cancellationToken);
        }

        /// <inheritdoc />
        public IJsonRpcSerializer Serializer { get; }

        /// <summary>
        /// Set client properties from base options
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        protected internal virtual void InitializeClient(HttpClient client, JsonRpcClientOptionsBase options)
        {
            client.BaseAddress = new Uri(options.Url, UriKind.Absolute);
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.Timeout = options.Timeout;
            Log.LogTrace("Client initialized: url {baseUrl}, user-agent {userAgent}, timeout {timeout}s", client.BaseAddress, client.DefaultRequestHeaders.UserAgent, client.Timeout.TotalSeconds);
        }

        /// <summary>
        /// Serialize data to JSON body, set Content-Type
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected internal virtual HttpContent CreateHttpContent(object data)
        {
            var body = JsonSerializer.Serialize(data, JsonRpcSerializer.Settings);
            return new StringContent(body, Encoding, JsonRpcConstants.ContentType);
        }

        protected internal virtual async Task<string> GetContent(HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            using (var stream = await content.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(stream, Encoding))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Parse single or batch response from http content string
        /// </summary>
        /// <param name="contentString"></param>
        /// <returns></returns>
        protected internal virtual IResponseWrapper ParseBody(string contentString)
        {
            return JsonSerializer.Deserialize<IResponseWrapper>(contentString, JsonRpcSerializer.Settings);
        }

        protected internal virtual IJsonRpcCallContext CreateContext()
        {
            return new JsonRpcCallContext();
        }
    }
}
