using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class PlayFabUserMgt : MonoBehaviour
{
    [SerializeField]InputField userEmailOrUsername,userPassword, userEmail;
    [SerializeField]Text Msg;
    [SerializeField]GameObject PanelLogin, PanelRegister, PanelForgotPassword;
    

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

        // Return true if input is a valid email or username, otherwise false
        return isEmail;
    }
    void OnError(PlayFabError e){
        //Debug.Log("Error"+e.GenerateErrorReport());
        UpdateMsg("Error"+e.GenerateErrorReport());
    }
    void UpdateMsg(string msg){ //to display in console and messagebox
        Debug.Log(msg);
        Msg.text=msg;
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
       SceneManager.LoadScene(1);
    }
    public void OnButtonLogout(){
        PlayFabClientAPI.ForgetAllCredentials();
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

