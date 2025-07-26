using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public interface IAuthenticationService : IService
{
    public UniTask<AuthenticationResponse> ConfirmSignUpAsync(string confirmationCode, string email);
    public UniTask<AuthenticationResponse> SignUpAsync(string email, string accountName, string nickname, string password);
    public UniTask<AuthenticationResponse> RefreshTokenAsync(string refreshToken);
    public UniTask<AuthenticationResponse> SignOutAsync(string accessToken);
    public UniTask<AuthenticationResponse> RequestPasswordResetCodeAsync(string email);

    public UniTask<AuthenticationResponse> ConfirmPasswordResetAsync(string email, string confirmationCode, string newPassword);
    public Task<AuthenticationResponse> LoginAsync(string username, string password);

    public string GetAccessToken();
    public string GetIdToken();
    public string GetRefreshToken();
}

// can be further moved to a RequestsAndResponses.cs, along with other Data Objects
public struct AuthenticationResponse
{
    public string ResponseMessage;
    public bool Successful;

    public AuthenticationResponse(string responseMessage, bool successful)
    {
        ResponseMessage = responseMessage;
        Successful = successful;
    }

}