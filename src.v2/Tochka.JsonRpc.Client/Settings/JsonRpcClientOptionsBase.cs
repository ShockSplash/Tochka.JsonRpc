using System;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Client.Settings
{
    /// <summary>
    /// Base class for JSON Rpc client with sane default values
    /// </summary>
    [PublicAPI]
    public abstract class JsonRpcClientOptionsBase
    {
        /// <summary>
        /// HTTP endpoint
        /// </summary>
        public virtual string Url { get; set; } // TODO change virtual?

        /// <summary>
        /// Request timeout, default is 10 seconds
        /// </summary>
        public virtual TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }

}
