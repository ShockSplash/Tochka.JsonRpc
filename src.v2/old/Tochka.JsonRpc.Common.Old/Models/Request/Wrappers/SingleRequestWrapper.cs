using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Old.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Old.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class SingleRequestWrapper : IRequestWrapper
    {
        public IUntypedCall Call { get; set; }
    }
}
