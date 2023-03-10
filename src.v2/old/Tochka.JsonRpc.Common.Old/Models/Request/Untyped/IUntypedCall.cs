using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Old.Models.Request.Untyped
{
    public interface IUntypedCall : ICall<JContainer>
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to this object
        /// </summary>
        [JsonIgnore]
        string RawJson { get; set; }
    }
}
