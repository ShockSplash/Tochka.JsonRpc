using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedNotification : Notification<JsonDocument>, IUntypedCall
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to this object
        /// </summary>
        public string RawJson { get; set; }
    }
}
