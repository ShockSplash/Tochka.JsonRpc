using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client;

internal class JsonRpcClientBase : IJsonRpcClient
{
    /// <summary>
    ///     Default is "Tochka.JsonRpc.Client"
    /// </summary>
    public virtual string UserAgent => typeof(JsonRpcClientBase).Namespace!;

    public IJsonRpcSerializer Serializer { get; }

    protected internal virtual Encoding Encoding => Encoding.UTF8;
    protected readonly ILogger log;

    protected internal JsonRpcClientBase(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, JsonRpcClientOptionsBase options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log)
    {

    }

    public Task SendNotification<T>(string requestUrl, Notification<T> notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendNotification<T>(Notification<T> notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendNotification<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendNotification<T>(string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, Request<T> request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(Request<T> request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, IRpcId id, string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ISingleJsonRpcResult> SendRequest<T>(IRpcId id, string method, T parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IBatchJsonRpcResult> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IBatchJsonRpcResult> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Set client properties from base options
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    protected internal virtual void InitializeClient(HttpClient client, JsonRpcClientOptionsBase options)
    {
        client.BaseAddress = new Uri(options.Url, UriKind.Absolute);
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.Timeout = options.Timeout;
        log.LogTrace("Client initialized: url = [{baseUrl}], user-agent = [{userAgent}], timeout = [{timeoutTotalSeconds}]s", client.BaseAddress, client.DefaultRequestHeaders.UserAgent, client.Timeout.TotalSeconds);
    }

    /// <summary>
    /// Serialize data to JSON body, set Content-Type
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected internal virtual HttpContent CreateHttpContent(object data)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse single or batch response from http content string
    /// </summary>
    /// <param name="contentString"></param>
    /// <returns></returns>
    protected internal virtual IResponseWrapper ParseBody(string contentString)
    {
        throw new NotImplementedException();
    }
}
