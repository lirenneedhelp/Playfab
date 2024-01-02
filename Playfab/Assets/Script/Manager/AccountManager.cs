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

            var request = new DeleteMasterPlayerAccountRequest()
            {
                PlayFabId = DataCarrier.Instance.playfabID
            };


            // Call the DeletePlayer API
            PlayFabAdminAPI.DeleteMasterPlayerAccount(request,
            result =>
            {
                Debug.Log("Account deletion successful!");
                PlayFabClientAPI.ForgetAllCredentials();
                //PhotonNetwork.LeaveRoom();
                //PhotonNetwork.Disconnect();
                Destroy(LevelSystem.Instance.gameObject);
                Destroy(Inventory.Instance.gameObject);

                Time.timeScale = 1;
                SceneManager.LoadScene("LoginScn");
            },
            error =>
            {
                Debug.LogError("Account deletion failed: " + error.ErrorMessage);
            });

        }
    }
}
