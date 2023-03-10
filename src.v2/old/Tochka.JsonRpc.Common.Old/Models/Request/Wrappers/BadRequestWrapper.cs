using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Old.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BadRequestWrapper : IRequestWrapper
    {
        public Exception Exception { get; set; }
    }
}
