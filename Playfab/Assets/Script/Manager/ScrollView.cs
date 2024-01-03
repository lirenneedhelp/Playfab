using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class ScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject leaderboardItemPrefab;

    struct PlayerInfo
    {
        public string pUser;
        public int score;
        public int position;
        public bool isP;

        public PlayerInfo(string dName, int pScore, int pos, bool p)
        {

            pUser = dName;
            score = pScore;
            position = pos;
            isP = p;
        }
    }
    void Start()
    {
        GetLeaderboardData();
    }

    public void GetLeaderboardData()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "HighScore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, 
        r =>
        {
             // Populate leaderboardScores with fetched data
            List<PlayerInfo> leaderboardScores = new();
            foreach (var entry in r.Leaderboard)
            {
                PlayerInfo pInfo = new PlayerInfo(entry.DisplayName, entry.StatValue, entry.Position + 1, entry.PlayFabId == DataCarrier.Instance.playfabID);
                leaderboardScores.Add(pInfo);
            }

            // Populate the leaderboard UI with the fetched data
            PopulateLeaderboard(leaderboardScores);
        }, 
        error => 
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void OnGetFriendLB(){
        PlayFabClientAPI.GetFriendLeaderboard(
            new GetFriendLeaderboardRequest{
                StatisticName = "HighScore",
                MaxResultsCount = 10
            },

            r=>{
                // Populate leaderboardScores with fetched data
                List<PlayerInfo> leaderboardScores = new ();
                foreach (var entry in r.Leaderboard)
                {
                    PlayerInfo pInfo = new PlayerInfo(entry.DisplayName, entry.StatValue, entry.Position + 1, entry.PlayFabId == DataCarrier.Instance.playfabID);
                    leaderboardScores.Add(pInfo);
                }

                // Populate the leaderboard UI with the fetched data
                PopulateLeaderboard(leaderboardScores);
            },

            error=>{
                Debug.Log(error.GenerateErrorReport());
            }
        );
    }

    public void GetLeaderboardAroundPlayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "HighScore",
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request,
        r =>
        {
             // Populate leaderboardScores with fetched data
            List<PlayerInfo> leaderboardScores = new();
            foreach (var entry in r.Leaderboard)
            {
                PlayerInfo pInfo = new PlayerInfo(entry.DisplayName, entry.StatValue, entry.Position + 1, entry.PlayFabId == DataCarrier.Instance.playfabID);
                leaderboardScores.Add(pInfo);
            }

            // Populate the leaderboard UI with the fetched data
            PopulateLeaderboard(leaderboardScores);
        }, 
        error => 
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    void PopulateLeaderboard(List<PlayerInfo> scores)
    {
        // Clear existing leaderboard items
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        // Create leaderboard items based on the fetched scores
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject leaderboardItem = Instantiate(leaderboardItemPrefab, _content);
            TMP_Text rankText = leaderboardItem.transform.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text playerNameText = leaderboardItem.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
            TMP_Text scoreText = leaderboardItem.transform.Find("ScoreText").GetComponent<TMP_Text>();

            rankText.text = scores[i].position.ToString(); // Rank starts from 1
            playerNameText.text = scores[i].pUser.ToString();
            scoreText.text = scores[i].score.ToString();

            if (scores[i].isP)
            {
                rankText.color = Color.green;
                playerNameText.color = Color.green;
                scoreText.color = Color.green;
            }
        }
    }
}
