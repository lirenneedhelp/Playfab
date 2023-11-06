using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Photon.Pun;


public class PlayFabUserMgt : MonoBehaviour
{
    public static PlayFabUserMgt Instance;
    [SerializeField]InputField userEmailOrUsername,userPassword, userEmail;
    [SerializeField]Text Msg;
    [SerializeField]GameObject PanelLogin, PanelRegister, PanelForgotPassword;

    [SerializeField] Toggle rememberMeToggle;

    private const string PlayerPrefsUsernameKey = "Username";
    private const string PlayerPrefsPasswordKey = "Password";

    void Start()
    {
        // Load saved credentials if "Remember Me" was checked previously
        if (PlayerPrefs.HasKey(PlayerPrefsUsernameKey) && PlayerPrefs.HasKey(PlayerPrefsPasswordKey))
        {
            string savedUsername = PlayerPrefs.GetString(PlayerPrefsUsernameKey);
            string savedPassword = PlayerPrefs.GetString(PlayerPrefsPasswordKey);

            userEmailOrUsername.text = savedUsername;
            userPassword.text = savedPassword;
            rememberMeToggle.isOn = true;
        }
    }
    void OnError(PlayFabError e)
    {
        //Debug.Log("Error"+e.GenerateErrorReport());
        UpdateMsg("Error"+e.GenerateErrorReport());
    }
    void UpdateMsg(string msg)
    { 
        //to display in console and messagebox
        Debug.Log(msg);
        Msg.text=msg;
    }
    // Checks whether an email / username has been entered
    public void CheckString()
    {
        if (IsEmail(userEmailOrUsername.text))
        {
            LoginWithEmail();
            return;
        }

        LoginWithUsername();
    }
    private bool IsEmail(string input)
    {
        // Regular expression patterns for email validation
        string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";

        // Check if the input matches the email pattern 
        bool isEmail = Regex.IsMatch(input, emailPattern);

        return isEmail;
    }
    public void LoginWithEmail(){
        var loginRequest=new LoginWithEmailAddressRequest{
            Email=userEmailOrUsername.text,
            Password=userPassword.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest,OnLoginSuccess,OnError);
    }
    public void LoginWithUsername()
    {
        var loginRequest = new LoginWithPlayFabRequest{
            Username = userEmailOrUsername.text,
            Password = userPassword.text
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest,OnLoginSuccess,OnError);
    }
    void OnLoginSuccess(LoginResult r){
       UpdateMsg("Login Success!");

       if (rememberMeToggle.isOn)
       {
            PlayerPrefs.SetString(PlayerPrefsUsernameKey, userEmailOrUsername.text);
            PlayerPrefs.SetString(PlayerPrefsPasswordKey, userPassword.text);
       }
       else
       {
            // If "Remember Me" is not checked, clear saved credentials from PlayerPrefs
            PlayerPrefs.DeleteKey(PlayerPrefsUsernameKey);
            PlayerPrefs.DeleteKey(PlayerPrefsPasswordKey);
       }
       DataCarrier.Instance.playfabID = r.PlayFabId;
       SceneManager.LoadScene(1);
    }
    public void OnButtonLogout(){
        PlayFabClientAPI.ForgetAllCredentials();
        PhotonNetwork.Disconnect();
        Destroy(LevelSystem.Instance.gameObject);
        Debug.Log("logged out");
        SceneManager.LoadScene("LoginScn");

    }
    public void PasswordResetRequest(){
        var req=new SendAccountRecoveryEmailRequest{
            Email=userEmail.text,
            TitleId=PlayFabSettings.TitleId //no need hardcode!
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req,OnPasswordReset,OnError);

    
    }
    void OnPasswordReset(SendAccountRecoveryEmailResult r){
        Msg.text="Password reset email sent.";
        userEmail.text = "";
    }
      public void OnButtonDeviceLogin(){ //login with device id
        var req0=new LoginWithCustomIDRequest{
            CustomId=SystemInfo.deviceUniqueIdentifier,CreateAccount=true,
            InfoRequestParameters=new GetPlayerCombinedInfoRequestParams{
                GetPlayerProfile=true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(req0,OnLoginSuccess,OnError);
    }
    public void OpenRegisterPanel()
    {
        PanelRegister.SetActive(true);
        PanelLogin.SetActive(false);
        ClearText();
    }
    public void CloseRegisterPanel()
    {
        PanelRegister.SetActive(false);
        PanelLogin.SetActive(true);
        ClearText();
    }
    public void OpenForgotPasswordPanel()
    {
        PanelForgotPassword.SetActive(true);
        PanelLogin.SetActive(false);
        ClearText();
    }
    public void CloseForgotPasswordPanel()
    {
       PanelForgotPassword.SetActive(false);
       PanelLogin.SetActive(true); 
       ClearText();
    }
    private void ClearText()
    {
        Msg.text = "";
    }
}

