using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages all the text and button inputs
// Also acts like the main manager script for the game.
public class UIInputManager : MonoBehaviour
{
   [SerializeField] public Button _signupButton;
   [SerializeField] public Button _loginButton;
   [SerializeField] public Button _startButton;
   [SerializeField] public Button _logoutButton;
   [SerializeField] public TMP_InputField _emailFieldLogin;
   [SerializeField] public TMP_InputField _passwordFieldLogin;
   [SerializeField] public TMP_InputField _usernameField;
   [SerializeField] public TMP_InputField _emailField;
   [SerializeField] public TMP_InputField _passwordField;
   [SerializeField] public Button _confirmSignupButton;
   [SerializeField] public TMP_InputField _signUpCodeField;
   [SerializeField] public Button _resetPasswordButton;
   [SerializeField] public Button _confirmNewPasswordButton;
   
   [SerializeField] private GameObject _loading;
   [SerializeField] private GameObject _confirmEmail;
   [SerializeField] private TextMeshProUGUI _feedback;

   private IAuthenticationService _authenticationManager;
   private GameObject _unauthInterface;
   private GameObject _authInterface;

   private async void Awake()
   {
      _unauthInterface = GameObject.Find("UnauthInterface");
      _authInterface = GameObject.Find("AuthInterface");

      _unauthInterface.SetActive(false); 
      _authInterface.SetActive(false);


      _authenticationManager = await ServiceLocator.GetService<IAuthenticationService>();
      
      TryAutoLogin();
   }
   
   private void Start()
   {
      _signupButton.onClick.AddListener(OnSignupClicked);
      _loginButton.onClick.AddListener(OnLoginClicked);
      _startButton?.onClick.AddListener(OnStartClick);
      _logoutButton.onClick.AddListener(OnLogoutClick);
      _confirmSignupButton.onClick.AddListener(OnConfirmSignIn);
      _resetPasswordButton.onClick.AddListener(OnResetPasswordClicked);
      _confirmNewPasswordButton.onClick.AddListener(OnConfirmNewPasswordClicked);
   }

   private async void TryAutoLogin()
   {
      string token = SaveDataManager.GetString("Token");
      string username = SaveDataManager.GetString("Username");
      
      // check if user is already authenticated 
      // We perform the refresh here to keep our user's session alive so they don't have to keep logging in.
      if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
      {
         await _authenticationManager.RefreshTokenAsync(token);
      }
   }
   
   private void DisplayComponentsFromAuthStatus(bool authStatus)
   {
      if (authStatus)
      {
         // Debug.Log("User authenticated, show welcome screen with options");
         _loading.SetActive(false);
         _unauthInterface.SetActive(false);
         _authInterface.SetActive(true);
      }
      else
      {
         // Debug.Log("User not authenticated, activate/stay on login scene");
         _loading.SetActive(false);
         _unauthInterface.SetActive(true);
         _authInterface.SetActive(false);
      }

      _passwordFieldLogin.text = "";
      _passwordField.text = "";
   }

   private async void OnLoginClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);
      AuthenticationResponse response = await _authenticationManager.LoginAsync(_emailFieldLogin.text, _passwordFieldLogin.text);
      Debug.Log(response.ResponseMessage);
      _feedback.text = response.ResponseMessage;
      DisplayComponentsFromAuthStatus(response.Successful);
   }

   private async void OnSignupClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);

      AuthenticationResponse signupResponse = await _authenticationManager.SignUpAsync(_emailField.text,
            _usernameField.text, _usernameField.text,
            _passwordField.text);
      Debug.Log(signupResponse.ResponseMessage);
      _feedback.text = signupResponse.ResponseMessage;

      if (signupResponse.Successful)
      {
         _confirmEmail.SetActive(true);

         _emailFieldLogin.text = _emailField.text;
         _passwordFieldLogin.text = _passwordField.text;
      }
      else
      {
         _confirmEmail.SetActive(false);
      }

      _loading.SetActive(false);
      _unauthInterface.SetActive(true);
   }

   private async void OnResetPasswordClicked()
   {
      AuthenticationResponse authenticationResponse = await _authenticationManager.RequestPasswordResetCodeAsync(_emailFieldLogin.text);
      Debug.Log(authenticationResponse.ResponseMessage);
      _feedback.text = authenticationResponse.ResponseMessage;
   }
   
   private async void OnLogoutClick()
   {
      AuthenticationResponse authenticationResponse = await _authenticationManager.SignOutAsync(SaveDataManager.GetString("AccessToken"));
      Debug.Log(authenticationResponse.ResponseMessage);
      _feedback.text = authenticationResponse.ResponseMessage;

      DisplayComponentsFromAuthStatus(false);
   }

   private void OnStartClick()
   {
      RefreshToken();
   }

   private async void RefreshToken()
   {
      AuthenticationResponse authenticationResponse = await _authenticationManager.RefreshTokenAsync(_authenticationManager.GetRefreshToken());
      Debug.Log(authenticationResponse.ResponseMessage);
      _feedback.text = authenticationResponse.ResponseMessage;

      DisplayComponentsFromAuthStatus(authenticationResponse.Successful);
   }

   private async void OnConfirmNewPasswordClicked()
   {
      AuthenticationResponse authenticationResponse = await _authenticationManager.ConfirmPasswordResetAsync(_emailFieldLogin.text, _signUpCodeField.text, _passwordFieldLogin.text);
      Debug.Log(authenticationResponse.ResponseMessage);
      _feedback.text = authenticationResponse.ResponseMessage;
   }

   private async void OnConfirmSignIn()
   {
      AuthenticationResponse authenticationResponse = await _authenticationManager.ConfirmSignUpAsync(_signUpCodeField.text, _emailField.text);
      Debug.Log(authenticationResponse.ResponseMessage);
      _feedback.text = authenticationResponse.ResponseMessage;
   }
}
