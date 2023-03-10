using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Naming", "CA1716:Идентификаторы не должны совпадать с ключевыми словами", Justification = "Error")]
    public class Error<T> : IError
    {
        public int Code { get; set; }

        // SHOULD be limited to a concise single sentence.
        public string Message { get; set; }

        // This may be omitted
        [SuppressMessage("Naming", "CA1721:Имена свойств не должны совпадать с именами методов get")]
        public T? Data { get; set; }

        public object GetData()
        {
            return Data;
        }
    }
}
