using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Old.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Old.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BatchRequestWrapper : IRequestWrapper
    {
        public List<IUntypedCall> Batch { get; set; }
    }
}
