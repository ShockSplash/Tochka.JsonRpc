namespace Tochka.JsonRpc.Client.Settings;

/// <summary>
/// Base class for JSON Rpc client with sane default values
/// </summary>
public abstract record JsonRpcClientOptionsBase
{
    /// <summary>
    /// HTTP endpoint
    /// </summary>
    public virtual string Url { get; init; } = null!;

    /// <summary>
    /// Request timeout, default is 10 seconds
    /// </summary>
    public virtual TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}
