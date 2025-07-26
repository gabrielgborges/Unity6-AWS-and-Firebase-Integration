using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class DynamoDBManager : ServiceBase, IDatabaseService
{
    private const string ApiURL = "ID_HERE";//insert your DynamoDB API URL or Lambda URL,
                                            //in this case I used a Lambda that triggers the DynamoDB and correctly confirm user authentication and permissions

    private void Awake()
    {
        ServiceLocator.AddService<IDatabaseService>(this);
    }

    public override void Setup() { }

    public override void Dispose() { }
    
    public async UniTask<DatabaseResponse> GetData(string id)
    {
        return await LambdaGetDataFromDynamoDB(id);
    }

    public async UniTask<DatabaseResponse> PostData(string id)
    {
        return await LambdaPostDataOnDynamoDB(id);
    }
    
    private async UniTask<DatabaseResponse> LambdaGetDataFromDynamoDB(string idToken)
    {
        DatabaseResponse response = new DatabaseResponse();

        UnityWebRequest request = new UnityWebRequest(ApiURL, "GET");
        request.SetRequestHeader("Authorization", idToken);
        request.SetRequestHeader("Content-Type", "application/json");

        request.downloadHandler = new DownloadHandlerBuffer();

        try
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                response.Response = "Lambda response: " + request.downloadHandler.text;
                response.Successful = true;
            }
            else
            {
                response.Response = "API error: " + request.responseCode + " - " + request.downloadHandler.text;
                response.Successful = false;
            }
        }
        catch (Exception ex)
        {
            response.Response = "Exception during LambdaGetData: " + ex.Message;
            response.Successful = false;
        }

        return response;
    }

    private async UniTask<DatabaseResponse> LambdaPostDataOnDynamoDB(string idToken)
    {
        DatabaseResponse response = new DatabaseResponse();

        string body = $@"
    {{
        ""item"": ""B.F. sword""
    }}";

        UnityWebRequest request = new UnityWebRequest(ApiURL, "POST");
        request.SetRequestHeader("Authorization", idToken);
        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        try
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                response.Response = "Lambda response: " + request.downloadHandler.text;
                response.Successful = true;
            }
            else
            {
                response.Response = "API error: " + request.responseCode + " - " + request.downloadHandler.text;
                response.Successful = false;
            }
        }
        catch (Exception ex)
        {
            response.Response = "Exception during LambdaPostData: " + ex.Message;
            response.Successful = false;
        }

        return response;
    }
}
