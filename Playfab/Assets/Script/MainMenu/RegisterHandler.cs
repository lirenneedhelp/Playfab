using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;

public class RegisterHandler : MonoBehaviour
{
    [SerializeField]TMP_InputField userEmail,userName,userPassword,userConfirmPassword;
    [SerializeField]TMP_Text Msg;

    public void UsernameSelected() => InputSelected = 0;
    public void EmailSelected() => InputSelected = 1;

    public void PasswordSelected() => InputSelected = 2;
    public void ConfirmPasswordSelected() => InputSelected = 3;


    private int InputSelected;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            InputSelected--;
            if (InputSelected < 0) InputSelected = 3;
            SelectInputField();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputSelected++;
            if (InputSelected > 3) InputSelected = 0;
            SelectInputField();
        }

        void SelectInputField()
        {
            switch (InputSelected)
            {
                case 0:
                    userName.Select();
                    break;
                case 1:
                    userEmail.Select();
                    break;
                case 2:
                    userPassword.Select();
                    break;
                case 3:
                    userConfirmPassword.Select();
                    break;
            }

        }
    }
    public void ValidatePassword()
    {
        if (userPassword.text == userConfirmPassword.text)
        {
            RegisterUser();
        }
        else
        {
            string msg = "The Passwords do not match!";
            UpdateMsg(msg);
            //ClearRegisterFields();
            //Debug.Log(msg);
        }

    }
    public void RegisterUser(){ //for button click
        var registerRequest=new RegisterPlayFabUserRequest
        {
            Email=userEmail.text,
            Password=userPassword.text,
            Username=userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,OnRegSuccess,
            e=>
            {
                Debug.LogError(e.GenerateErrorReport());

                switch (e.Error)
                {
                    case PlayFabErrorCode.UsernameNotAvailable:
                        Msg.text = "Username Taken";
                        Debug.LogWarning("Register Fail Username Taken");
                        break;

                    case PlayFabErrorCode.EmailAddressNotAvailable:
                        Msg.text = "Email Already Registered";
                        Debug.LogWarning("Register Fail Email Already Registered");
                        break;

                    case PlayFabErrorCode.InvalidEmailAddress:
                        Msg.text = "Invalid Email";
                        Debug.LogWarning("Register Fail Invalid Email");
                        break;

                    default:
                        //Msg.text = "Username Available";
                        //Msg.text = "Email Available";
                        Debug.LogWarning("Register default");
                        break;
                } 
            });   
    }
    void OnRegSuccess(RegisterPlayFabUserResult r){
        Debug.Log("Register Success!");        

        //To create a player display name 
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userName.text,
        };
        // update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
        ClearRegisterFields();
        UpdateMsg("Registration success!");

    }
    void OnError(PlayFabError e){
        //Debug.Log("Error"+e.GenerateErrorReport());
        Debug.Log("Error"+e.GenerateErrorReport());
    }

    void UpdateMsg(string msg){ //to display in console and messagebox
        Debug.Log(msg);
        Msg.text=msg;
    }
    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        Debug.Log("display name updated!" + r.DisplayName);
    }
    void ClearRegisterFields()
    {
        userEmail.text = "";
        userName.text = "";
        userPassword.text = "";
        userConfirmPassword.text = "";
    }
}
