using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegisterHandler : MonoBehaviour
{
    [SerializeField]InputField userEmail,userName,userPassword,userConfirmPassword;
    [SerializeField]Text Msg;


    public void ValidatePassword()
    {
        if (userPassword.text == userConfirmPassword.text)
            RegisterUser();
        else
            Debug.Log("The Passwords do not match!");
    }
    public void RegisterUser(){ //for button click
        var registerRequest=new RegisterPlayFabUserRequest
        {
            Email=userEmail.text,
            Password=userPassword.text,
            Username=userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,OnRegSuccess,OnError);   
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
        //UpdateMsg("Registration success!");

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
}
