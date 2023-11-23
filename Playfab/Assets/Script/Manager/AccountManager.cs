using UnityEngine;
using PlayFab;
using PlayFab.AdminModels;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab.ClientModels;

public class AccountManager : MonoBehaviour
{
    [SerializeField] TMP_InputField confirmDName;

    public void DeleteAccount()
    {
        if (confirmDName.text == DataCarrier.Instance.displayName)
        {
            if (string.IsNullOrEmpty(DataCarrier.Instance.playfabID) || string.IsNullOrEmpty(DataCarrier.Instance.sessionTicket))
            {
                Debug.LogError("Player is not authenticated. Please authenticate first.");
                return;
            }

            // Define the CloudScript function request
            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "deletePlayerAccount", // Specify the CloudScript function name
                FunctionParameter = new { PlayFabId = DataCarrier.Instance.playfabID }
            };

            // Call the CloudScript function
            PlayFabClientAPI.ExecuteCloudScript(request,
                result =>
                {
                    Debug.Log("Account deletion successful!");
                    PlayFabClientAPI.ForgetAllCredentials();
                    Destroy(LevelSystem.Instance.gameObject);
                    SceneManager.LoadScene(0);
                    //PhotonNetwork.LeaveRoom();
                    //PhotonNetwork.Disconnect();
                },
                error =>
                {
                    Debug.LogError("Account deletion failed: " + error.ErrorMessage);
                });


            //var request = new DeleteMasterPlayerAccountRequest()
            //{
            //    PlayFabId = DataCarrier.Instance.playfabID
            //};


            //// Call the DeletePlayer API
            //PlayFabAdminAPI.DeleteMasterPlayerAccount(request, 
            //result=>{
            //    Debug.Log("Account deletion successful!");
            //    PlayFabClientAPI.ForgetAllCredentials();
            //    //PhotonNetwork.LeaveRoom();
            //    //PhotonNetwork.Disconnect();
            //    Destroy(LevelSystem.Instance.gameObject);
            //    SceneManager.LoadScene(0);
            //}, 
            //error=>{
            //    Debug.LogError("Account deletion failed: " + error.ErrorMessage);
            //});
        }
    }
}
