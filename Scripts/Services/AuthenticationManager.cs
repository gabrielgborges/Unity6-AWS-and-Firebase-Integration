using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;

public class AuthenticationManager : MonoBehaviour, IAuthenticationService
{
   private Amazon.RegionEndpoint _region = Amazon.RegionEndpoint.APEast2;

   // In production, should properly keep these in a config file
   private const string IdentityPool = "ID_HERE"; //insert your Cognito Identity Pool ID, found under General Settings
   private const string AppClientID = "ID_HERE"; //insert App client ID, found under App Client Settings
   private const string UserPoolId = "ID_HERE"; //insert your Cognito User Pool ID, found under General Settings
   private const string CognitoApiURL = "ID_HERE"; //insert your Cognito API url, found on the main screen
   
   public string GetAccessToken() => SaveDataManager.GetString("AccessToken");
   public string GetIdToken() => SaveDataManager.GetString("IdToken");
   public string GetRefreshToken() => SaveDataManager.GetString("Token");
   public string GetUserId => SaveDataManager.GetString("Username");
   
   private void Awake()
   {
      System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12; 
      ServiceLocator.AddService<IAuthenticationService>(this);
   }

   public async UniTask<AuthenticationResponse> SignUpAsync(string email, string accountName, string nickname,
      string password)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.SignUp");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1; charset=UTF-8");

      string body = $@"
    {{
        ""ClientId"": ""{AppClientID}"",
        ""Username"": ""{accountName}"",
        ""Password"": ""{password}"",
        ""UserAttributes"": [
            {{
                ""Name"": ""email"",
                ""Value"": ""{email}""
            }},
            {{
                ""Name"": ""nickname"",
                ""Value"": ""{nickname}""
            }}
        ]
    }}";

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Register successful: " + request.downloadHandler.text;
            response.Successful = true;
         }
         else
         {
            response.ResponseMessage = "Register error: " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during SignUp: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }

   public async UniTask<AuthenticationResponse> ConfirmSignUpAsync(string confirmationCode, string email)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""ClientId"": ""{AppClientID}"",
            ""Username"": ""{email.Trim()}"",
            ""ConfirmationCode"": ""{confirmationCode.Trim()}""
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ConfirmSignUp");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "User confirmed successfully: " + request.downloadHandler.text;
            response.Successful = true;
         }
         else
         {
            CognitoResponse responseObject = JsonUtility.FromJson<CognitoResponse>(request.downloadHandler.text);
            response.ResponseMessage = $"ConfirmSignUp failed: {responseObject.__type} - {request.downloadHandler.text}";
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during ConfirmSignUp: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }

   private async UniTask<AuthenticationResponse> ResendSignupCode(string email)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""ClientId"": ""{AppClientID}"",
            ""Username"": ""{email}""
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ResendConfirmationCode");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Confirmation code resent: " + request.downloadHandler.text;
            response.Successful = true;
         }
         else
         {
            response.ResponseMessage = "Resend failed: " + request.responseCode + " - " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during ResendSignupCode: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }
   
   public async UniTask<AuthenticationResponse> RequestPasswordResetCodeAsync(string email)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""ClientId"": ""{AppClientID}"",
            ""Username"": ""{email}""
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ForgotPassword");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1; charset=UTF-8");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Reset code sent successfully: " + request.downloadHandler.text;
            response.Successful = true;
         }
         else
         {
            response.ResponseMessage = "ForgotPassword failed: " + request.responseCode + " - " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during ForgotPassword: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }

   public async UniTask<AuthenticationResponse> ConfirmPasswordResetAsync(string email, string confirmationCode, string newPassword)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""ClientId"": ""{AppClientID}"",
            ""Username"": ""{email}"",
            ""ConfirmationCode"": ""{confirmationCode}"",
            ""Password"": ""{newPassword}""
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ConfirmForgotPassword");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Password reset successful!";
            response.Successful = true;
         }
         else
         {
            response.ResponseMessage = "ConfirmForgotPassword failed: " + request.responseCode + " - " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during ConfirmPasswordReset: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }
   
   public async Task<AuthenticationResponse> LoginAsync(string username, string password)
   {
      AuthenticationResponse response = new AuthenticationResponse();

        string body = $@"
        {{
            ""AuthFlow"": ""USER_PASSWORD_AUTH"",
            ""ClientId"": ""{AppClientID}"",
            ""AuthParameters"": {{
                ""USERNAME"": ""{username}"",
                ""PASSWORD"": ""{password}""
            }}
        }}";

        UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
        request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");
        request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();

        try
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                CognitoAuthResponse authResponse = JsonUtility.FromJson<CognitoAuthResponse>(json);

                SaveDataManager.SetString("Token", authResponse.AuthenticationResult.RefreshToken);
                SaveDataManager.SetString("Username", username);
                SaveDataManager.SetString("AccessToken", authResponse.AuthenticationResult.AccessToken);
                SaveDataManager.SetString("IdToken", authResponse.AuthenticationResult.IdToken);

                MakeInventoryRetrieveTests(authResponse.AuthenticationResult.IdToken);

                response.Successful = true;
                response.ResponseMessage = "Login successful!";
            }
            else
            {
                response.Successful = false;
                response.ResponseMessage = $"Login failed: {request.responseCode} - {request.downloadHandler.text}";
            }
        }
        catch (System.Exception ex)
        {
            response.Successful = false;
            response.ResponseMessage = "Exception during login: " + ex.Message;
        }

        return response;
   }

   private async void MakeInventoryRetrieveTests(string tokenID)
   {
      //valid post
      IDatabaseService databaseService = await ServiceLocator.GetService<IDatabaseService>();
      DatabaseResponse validResponse = await databaseService.PostData(tokenID);
      Debug.Log(validResponse.Response);
      //expired token test
      DatabaseResponse invalidResponse = await databaseService.PostData("eyJraWQiOiJiN2hGUkVEV0d6djZRY0t4UVFSN01YQlpwT3BsMG5FeUxFNjZZNm93Y1dNPSIsImFsZyI6IlJTMjU2In0");
      Debug.Log(invalidResponse.Response);
   }

   public async UniTask<AuthenticationResponse> RefreshTokenAsync(string refreshToken)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""AuthFlow"": ""REFRESH_TOKEN_AUTH"",
            ""ClientId"": ""{AppClientID}"",
            ""AuthParameters"": {{
                ""REFRESH_TOKEN"": ""{refreshToken}""
            }}
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Refreshed tokens successful!";
            response.Successful = true;
         }
         else
         {
            ClearTokens();
            response.ResponseMessage = "Refreshed tokens failed: " + request.responseCode + " - " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during RefreshToken: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }
   
   public async UniTask<AuthenticationResponse> SignOutAsync(string accessToken)
   {
      AuthenticationResponse response = new AuthenticationResponse();

      string body = $@"
        {{
            ""AccessToken"": ""{accessToken}""
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.GlobalSignOut");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();
         ClearTokens();

         if (request.result == UnityWebRequest.Result.Success)
         {
            response.ResponseMessage = "Global sign-out successful.";
            response.Successful = true;
         }
         else
         {
            response.ResponseMessage = "GlobalSignOut failed: " + request.responseCode + " - " + request.downloadHandler.text;
            response.Successful = false;
         }
      }
      catch (System.Exception ex)
      {
         response.ResponseMessage = "Exception during SignOut: " + ex.Message;
         response.Successful = false;
      }

      return response;
   }
   
   public async UniTask<GetUserAttributesResponse> GetCognitoUserAttributes(string accessToken)
   {
      string body = $@"
    {{
        ""AccessToken"": ""{accessToken}""
    }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.GetUser");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
      request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      request.downloadHandler = new DownloadHandlerBuffer();

      await request.SendWebRequest();
      try
      {
         if (request.result == UnityWebRequest.Result.Success)
         {
            string json = request.downloadHandler.text;
            Debug.Log("User attributes: " + json);

            var user = JsonUtility.FromJson<GetUserAttributesResponse>(json);
            return user;
         }
         else
         {
            Debug.LogError("GetUser failed: " + request.responseCode + " - " + request.downloadHandler.text);
         }
      }
      catch (System.Exception ex)
      {
         Debug.LogError("Exception during GetUser: " + ex.Message);
      }

      return null;
   }
   
   public async UniTask<string> GetCognitoIdentityId(string idToken)
   {
      string body = $@"
        {{
            ""IdentityPoolId"": ""{IdentityPool}"",
            ""Logins"": {{
                ""{CognitoApiURL + UserPoolId}"": ""{idToken}""
            }}
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityService.GetId");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            string json = request.downloadHandler.text;
            Debug.Log("Identity ID received: " + json);
            GetIdResponse identityId = JsonUtility.FromJson<GetIdResponse>(json);
            return identityId.IdentityId;
         }
         else
         {
            Debug.LogError("GetId failed: " + request.responseCode + " - " + request.downloadHandler.text);
         }
      }
      catch (System.Exception ex)
      {
         Debug.LogError("Exception during GetId: " + ex.Message);
      }

      return String.Empty;
   }
   
   public async UniTask<AwsCredentials> GetAwsCredentials(string identityId, string idToken)
   {
      AwsCredentials credentials = new AwsCredentials();
      
      string body = $@"
        {{
            ""IdentityId"": ""{identityId}"",
            ""Logins"": {{
                ""{CognitoApiURL + UserPoolId}"": ""{idToken}""
            }}
        }}";

      UnityWebRequest request = new UnityWebRequest(CognitoApiURL, "POST");
      request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityService.GetCredentialsForIdentity");
      request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");

      request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
      request.downloadHandler = new DownloadHandlerBuffer();

      try
      {
         await request.SendWebRequest();

         if (request.result == UnityWebRequest.Result.Success)
         {
            string json = request.downloadHandler.text;
            Debug.Log("AWS Credentials: " + json);
            credentials = JsonUtility.FromJson<AwsCredentials>(json);
         }
         else
         {
            Debug.LogError("GetCredentialsForIdentity failed: " + request.responseCode + " - " + request.downloadHandler.text);
         }
      }
      catch (System.Exception ex)
      {
         Debug.LogError("Exception during GetCredentialsForIdentity: " + ex.Message);
      }

      return credentials;
   }

   private void ClearTokens()
   {
      SaveDataManager.SetString("Token", string.Empty);
      SaveDataManager.SetString("Username", string.Empty);
      SaveDataManager.SetString("AccessToken", string.Empty);
      SaveDataManager.SetString("IdToken", string.Empty);
   }
}
