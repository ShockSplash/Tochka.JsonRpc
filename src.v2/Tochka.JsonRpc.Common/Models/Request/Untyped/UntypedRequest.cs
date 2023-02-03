using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedRequest : Request<JsonDocument>, IUntypedCall
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to this object
        /// </summary>
        // [JsonIgnore]
        public string RawJson { get; set; }

        /// <summary>
        /// Set on deserialization. JSON content corresponding to id property
        /// </summary>
        // [JsonIgnore]
        public JsonValue RawId { get; set; }
    }
}
