using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Old.Models.Response.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BatchResponseWrapper : IResponseWrapper
    {
        public List<IResponse> Batch { get; set; }
    }
}
