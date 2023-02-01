namespace Tochka.JsonRpc.Client.Models;

public class BatchResponseWrapper : IResponseWrapper
{
    public List<IResponse> Batch { get; set; }
}
