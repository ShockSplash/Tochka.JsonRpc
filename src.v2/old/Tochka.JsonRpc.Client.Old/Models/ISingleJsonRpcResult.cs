using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Old.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Old.Models
{
    public interface ISingleJsonRpcResult
    {
        T GetResponseOrThrow<T>();
        T AsResponse<T>();
        bool HasError();
        Error<JToken> AsAnyError();
        Error<T> AsTypedError<T>();
        Error<ExceptionInfo> AsErrorWithExceptionInfo();
    }
}
