using Tochka.JsonRpc.Common.Old.Models.Id;

namespace Tochka.JsonRpc.Common.Old.Models.Response
{
    public interface IResponse
    {
        IRpcId Id { get; set; }

        string Jsonrpc { get; set; }
    }
}
