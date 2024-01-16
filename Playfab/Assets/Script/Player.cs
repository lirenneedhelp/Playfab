using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    private float moveSpeed = 7.0f;
    private bool canMove = true;

    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float directionX = 0f;
    private GameObject shopPanel;
    private GameObject guildPanel;
    private GameObject friendsPanel;
    private GameObject leaderPanel;
    private GameObject eButton;

    public bool isOffline = false;

    [SerializeField] TMP_Text player_NameTag;
    [SerializeField] GameObject otherPlayerInfo;
    [SerializeField] GameObject tradeRequestPanel;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject inventoryUI;

    [HideInInspector] public Photon.Realtime.Player photonPlayer;
    private bool[] tradeAcceptance;
    [SerializeField] TradeManager tradeManager;

    PhotonView pv;


    enum CONTACT_TYPE
    {
        NIL,
        SHOP,
        GUILD,
        FRIEND,
        LEADERBOARD,
        GAME,
    }

    private CONTACT_TYPE contactType;
    private TextMeshProUGUI currentText;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        guildPanel = GameObject.FindGameObjectWithTag("Guild");
        shopPanel = GameObject.FindGameObjectWithTag("Shop");
        friendsPanel = GameObject.FindGameObjectWithTag("Friends");
        leaderPanel = GameObject.FindGameObjectWithTag("Leaderboard");
        currentText = GameObject.FindGameObjectWithTag("CurrentText").GetComponent<TextMeshProUGUI>();
        eButton = GameObject.Find("EButton");

        canMove = true;
        tradeAcceptance = new bool[2] { false, false };

        pv = GetComponent<PhotonView>();


        if (pv.IsMine)
        {
            eButton.SetActive(false);
            inventoryUI.SetActive(true);
            //Debug.LogError("HELLO FLECKERS");
            //photonPlayer = PhotonNetwork.LocalPlayer;
        }
        else
        {
            inventoryUI.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine || isOffline)
        {
            if (DataCarrier.Instance.displayName != "")
                player_NameTag.text = "(" + LevelSystem.Instance.level + ") " + DataCarrier.Instance.displayName;

            if (player_NameTag.text.Length > 0)
            {
                // Get the position of the player in world space
                Vector3 playerWorldPosition = transform.position;

                playerWorldPosition.y += 1f;

                // Convert the player's world position to screen space
                Vector3 nameTagScreenPosition = Camera.main.WorldToScreenPoint(playerWorldPosition);
                // Calculate the x-coordinate adjustment based on text length
                //Debug.Log(player_NameTag.text.Length);
                //float xOffset = 13.5f * (5f - player_NameTag.text.Length * 0.5f + 1.75f);
                //Debug.Log(xOffset);

                // Adjust the x-coordinate to move the text
                //nameTagScreenPosition.x += xOffset;

                //Debug.Log(nameTagScreenPosition.x);


                // Set the position of the name tag UI
                player_NameTag.transform.position = nameTagScreenPosition;
            }

            if (canMove)
            {
                directionX = Input.GetAxisRaw("Horizontal");
                rigidBody2D.velocity = new Vector2(directionX * moveSpeed, rigidBody2D.velocity.y);

                //If we press E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    switch (contactType)
                    {
                        case CONTACT_TYPE.SHOP:
                            shopPanel.GetComponent<ShopController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.FRIEND:
                            friendsPanel.GetComponent<FriendsController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.LEADERBOARD:
                            leaderPanel.GetComponent<LeaderboardController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.GUILD:
                            guildPanel.GetComponent<GuildController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.GAME:
                            GoToGame();
                            break;
                    }

                    if (contactType != CONTACT_TYPE.NIL)
                    {
                        canMove = false;
                        rigidBody2D.velocity = Vector3.zero;
                        animator.enabled = false;
                        eButton.SetActive(false);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    animator.SetTrigger("Is_Dancing");
                }


                //Update the animation
                UpdateAnimationUpdate();
            }
        }
    }

    public void GoToGame()
    {
        if (!isOffline)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel(2);
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    private void UpdateAnimationUpdate()
    {
        if (directionX > 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = false;

        }
        else if (directionX < 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.SetBool("Is_Running", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine || isOffline)
        {
            if (collision.gameObject.CompareTag("Guild"))
            {
                contactType = CONTACT_TYPE.GUILD;
                eButton.SetActive(true);
                currentText.text = "Guild";
            }
            else if (collision.gameObject.CompareTag("Shop"))
            {
                contactType = CONTACT_TYPE.SHOP;
                eButton.SetActive(true);
                currentText.text = "Shop";
            }
            else if (collision.gameObject.CompareTag("Friends"))
            {
                contactType = CONTACT_TYPE.FRIEND;
                eButton.SetActive(true);
                currentText.text = "Friends";
            }
            else if (collision.gameObject.CompareTag("Leaderboard"))
            {
                contactType = CONTACT_TYPE.LEADERBOARD;
                eButton.SetActive(true);
                currentText.text = "Leaderboard";
            }
            else if (collision.gameObject.CompareTag("Game"))
            {
                contactType = CONTACT_TYPE.GAME;
                eButton.SetActive(true);
                currentText.text = "Game";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine || isOffline)
        {
            contactType = CONTACT_TYPE.NIL;
            eButton.SetActive(false);
            currentText.text = "";
        }
    }

    public void ClosePanel()
    {
        if (photonView.IsMine || isOffline)
        {
            canMove = true;
            animator.enabled = true;
            eButton.SetActive(true);
        }
    }

    public void OpenPlayerInfo()
    {
        if (PhotonNetwork.LocalPlayer.NickName == player_NameTag.text)
            return;

        string PlayerName = player_NameTag.text;
        photonPlayer = FindPlayerByNickname(PlayerName);
        otherPlayerInfo.transform.Find("Display").GetComponent<TMP_Text>().text = PlayerName;
        otherPlayerInfo.SetActive(true);

    }

    public void ClosePlayerInfo()
    {
        otherPlayerInfo.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // If you are the owner, send the value to the network
            stream.SendNext(spriteRenderer.flipX);
            stream.SendNext(player_NameTag.text);
            stream.SendNext(player_NameTag.transform.position);
            //stream.SendNext(transform.position);
        }
        else
        {
            // If you are not the owner, receive the value from the network
            spriteRenderer.flipX = (bool)stream.ReceiveNext();
            player_NameTag.text = (string)stream.ReceiveNext();
            player_NameTag.transform.position = (Vector3)stream.ReceiveNext();
            //transform.position = (Vector3)stream.ReceiveNext();
        }
    }

    public void OpenTradePanel()
    {
        ClosePlayerInfo();
        pv.RPC(nameof(RPC_InitiateTrade), photonPlayer, PhotonNetwork.LocalPlayer.NickName);
    }

    public void CloseTradePanel()
    {
        tradeRequestPanel.SetActive(false);
    }

    public void OpenTrade()
    {
        CloseTradePanel();
        Photon.Realtime.Player[] players = new Photon.Realtime.Player[] { PhotonNetwork.LocalPlayer, photonPlayer };
        pv.RPC(nameof(RPC_OpenTrade), RpcTarget.AllViaServer, players);
    }
    public void CloseTrade()
    {
        Photon.Realtime.Player[] players = new Photon.Realtime.Player[] { PhotonNetwork.LocalPlayer, photonPlayer };
        pv.RPC(nameof(RPC_CloseTrade), RpcTarget.AllViaServer, players);

    }
    public void AcceptTrade()
    {
        tradeAcceptance[0] = true;
        pv.RPC(nameof(RPC_AcceptTrade), photonPlayer, true);

        if (AllPlayersAccepted())
        {
            // Do something when both players accept the trade
            Photon.Realtime.Player[] players = new Photon.Realtime.Player[] { PhotonNetwork.LocalPlayer, photonPlayer };
            pv.RPC(nameof(RPC_ProcessTrade), RpcTarget.All, players);
            pv.RPC(nameof(RPC_CloseTrade), RpcTarget.AllViaServer, players);
        }

    }


    #region RPC_FUNCTION


    [PunRPC]
    void RPC_InitiateTrade(string initiator)
    {
        tradeRequestPanel.SetActive(true);
        tradeRequestPanel.transform.Find("Display").GetComponent<TMP_Text>().text = initiator + " wants to trade with you";
        photonPlayer = FindPlayerByNickname(initiator);
    }

  
    [PunRPC]
    void RPC_OpenTrade(Photon.Realtime.Player[] playerArray)
    {
        Photon.Realtime.Player otherPlayer = PhotonNetwork.LocalPlayer;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (!ArrayContainsPlayer(playerArray, player))
                continue;
            
            if (player != PhotonNetwork.LocalPlayer)
                otherPlayer = player;
            

        }
        tradePanel.SetActive(true);
        tradePanel.transform.Find("Trading_Local").Find("LocalPlayerDisplay").GetComponent<TMP_Text>().text = PhotonNetwork.LocalPlayer.NickName + "'s trade";
        tradePanel.transform.Find("Trading_Other").Find("Other_Display").GetComponent<TMP_Text>().text = otherPlayer.NickName + "'s trade";

    }

    [PunRPC]
    void RPC_CloseTrade(Photon.Realtime.Player[] playerArray)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (!ArrayContainsPlayer(playerArray, player))
                return;
            
        }
        tradePanel.SetActive(false);
        tradeAcceptance = new bool[2] { false, false };
        tradePanel.GetComponent<TradeController>().DestroySlotChild();
        tradePanel.GetComponent<TradeController>().UpdateSprite(null);
    }

    [PunRPC]
    void RPC_AcceptTrade(bool trade_status)
    {
        tradeAcceptance[1] = trade_status;
    }

    [PunRPC]
    void RPC_ProcessTrade(Photon.Realtime.Player[] playerArray)
    {
        Debug.LogError(playerArray[0].NickName);
        Debug.LogError(playerArray[1].NickName);

        Photon.Realtime.Player otherPlayer = PhotonNetwork.LocalPlayer;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (!ArrayContainsPlayer(playerArray, player))
                continue;

            if (player != PhotonNetwork.LocalPlayer)
                otherPlayer = player;


        }
        //Debug.LogError(otherPlayer.NickName);
        tradeManager.GiveItemTo(otherPlayer.NickName, tradePanel.GetComponent<TradeController>().ReturnLocalPlayerItem(), PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    void RPC_CompleteTrade(string gifter, string tradeId)
    {
        tradeManager.AcceptGiftFrom(gifter, tradeId);
    }

    [PunRPC]
    void UpdateTrade(byte[] spriteBytes)
    {
        Texture2D texture = new Texture2D(2, 2); // Adjust dimensions accordingly
        ImageConversion.LoadImage(texture, spriteBytes);
        Sprite receivedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        tradePanel.GetComponent<TradeController>().UpdateSprite(receivedSprite);
    }

    #endregion

    // Function to find Photon player by nickname
    public static Photon.Realtime.Player FindPlayerByNickname(string nickname)
    {
        Photon.Realtime.Player[] photonPlayers = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in photonPlayers)
        {
            if (player.NickName == nickname)
            {
                // Match found
                return player;
            }
        }

        // No match found
        return null;
    }
    bool ArrayContainsPlayer(Photon.Realtime.Player[] players, Photon.Realtime.Player targetPlayer)
    {
        foreach (Photon.Realtime.Player player in players)
        {
            if (player == targetPlayer)
            {
                return true;
            }
        }
        return false;
    }
    bool AllPlayersAccepted()
    {
        // Check if all players in the trade have accepted
        for (int i = 0; i < tradeAcceptance.Length; i++)
        {
            if (!tradeAcceptance[i])
            {
                return false;
            }
        }
        return true;
    }
}
