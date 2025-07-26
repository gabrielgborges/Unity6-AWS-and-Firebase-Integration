using Cysharp.Threading.Tasks;

public interface IDatabaseService : IService
{
    public UniTask<DatabaseResponse> GetData(string tokenID);
    public UniTask<DatabaseResponse> PostData(string tokenID);
}

public struct DatabaseResponse
{
    public string Response;
    public bool Successful;

    public DatabaseResponse(string response, bool successful)
    {
        Response = response;
        Successful = successful;
    }

}