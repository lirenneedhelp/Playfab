using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using TMPro;
using Photon.Pun;

public class PlayFabUserMgt : MonoBehaviour
{
    public static PlayFabUserMgt Instance = null;
    [SerializeField]TMP_InputField userEmailOrUsername,userPassword;
    [SerializeField]InputField userEmail;
    [SerializeField]TMP_Text Msg, forgotMsg;
    [SerializeField]GameObject PanelLogin, PanelRegister, PanelForgotPassword;

    [SerializeField] Toggle rememberMeToggle, hidePassword;

    [SerializeField] Sprite eyeEnabled, eyeDisabled;

    private const string PlayerPrefsUsernameKey = "Username";
    private const string PlayerPrefsPasswordKey = "Password";

    private int InputSelected;

    public void UsernameSelected() => InputSelected = 0;
    public void PasswordSelected() => InputSelected = 1;

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
        HidePassword();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            InputSelected--;
            if (InputSelected < 0) InputSelected = 1;
            SelectInputField();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputSelected++;
            if (InputSelected > 1) InputSelected = 0;
            SelectInputField();
        }

        void SelectInputField()
        {
            switch (InputSelected)
            {
                case 0: 
                    userEmailOrUsername.Select();
                    break;
                case 1:
                    userPassword.Select();
                    break;
            }

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
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest,OnLoginSuccess,
        error =>
        {
            ClearLoginFields();
            
            switch (error.Error)
            {
                case PlayFabErrorCode.InvalidParams:
                    UpdateMsg("Invalid Email/Password");
                    break;

                case PlayFabErrorCode.AccountNotFound:
                    UpdateMsg("Account not found. Please check your email and try again.");
                    break;

                default:
                    UpdateMsg("An error occurred during login.");
                    break;
            }
            
        });
    }
    public void LoginWithUsername()
    {
        var loginRequest = new LoginWithPlayFabRequest{
            Username = userEmailOrUsername.text,
            Password = userPassword.text
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest,OnLoginSuccess,
        error =>
        {
            ClearLoginFields();

            switch (error.Error)
            {
                case PlayFabErrorCode.InvalidUsernameOrPassword:
                    UpdateMsg("Invalid Username or Password");
                    break;

                case PlayFabErrorCode.InvalidParams:
                    UpdateMsg("Fill out the fields");
                    break;

                case PlayFabErrorCode.AccountNotFound:
                    UpdateMsg("Account not found. Please check your username and try again.");
                    break;

                default:
                    UpdateMsg("An error occurred during login.");
                    break;
            }
           
        });
    }
    public void GuestLogin()
    {
        var guestReq = new LoginWithCustomIDRequest
        {
                CustomId=SystemInfo.deviceUniqueIdentifier,CreateAccount=true,
                InfoRequestParameters=new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile=true
                }
        };
        PlayFabClientAPI.LoginWithCustomID(guestReq, 
        r =>
        {
            UpdateMsg("Guest Login!");
            DataCarrier.Instance.playfabID = r.PlayFabId;

            var req = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = RandomNameGenerator.GenerateName(),
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(req, 
            r =>{
                Debug.Log("display name updated!" + r.DisplayName);
            }, OnError);
            SceneManager.LoadScene(1);
        },
        OnError);


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
       DataCarrier.Instance.sessionTicket = r.SessionTicket;
       SceneManager.LoadScene(1);
    }


    public void OnButtonLogout(){
        PlayFabClientAPI.ForgetAllCredentials();
        //PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        Destroy(LevelSystem.Instance.gameObject);
        Destroy(Inventory.Instance.gameObject);

        Time.timeScale = 1;
        SceneManager.LoadScene("LoginScn");
        Debug.Log("logged out");
    }
    public void PasswordResetRequest(){
        var req=new SendAccountRecoveryEmailRequest{
            Email=userEmail.text,
            TitleId=PlayFabSettings.TitleId //no need hardcode!
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req,OnPasswordReset,OnError);

    
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult r){
        forgotMsg.text="Password reset email sent.";
        userEmail.text = "";
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
    private void ClearLoginFields()
    {
        if (!rememberMeToggle.isOn)
        {
            userEmailOrUsername.text = "";
            userPassword.text = "";
        }
    }
    public void HidePassword()
    {
        userPassword.contentType = hidePassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        hidePassword.transform.Find("Background").GetComponent<Image>().sprite = hidePassword.isOn ? eyeEnabled : eyeDisabled;
        userPassword.ForceLabelUpdate();
    }
}


