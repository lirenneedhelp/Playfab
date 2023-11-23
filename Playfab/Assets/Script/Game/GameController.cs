using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SocialPlatforms.Impl;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public GameObject gameOverObject;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;
    public int difficultyMax = 5;

    [HideInInspector]
    public bool isGameOver = false;
    public float scrollSpeed = -5f;

    private int score = 0;
    private int highestScore = 0;

    public Spawner spawner;

    public TMP_Text coin_Text;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        LoadHighScore();
        scoreText.text = "0";
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOverObject.SetActive(false);
        InvokeRepeating("AddScore", 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void AddScore()
    {
        int multiplier = Inventory.Instance.usage > 0 ? 2 : 1;
        score += 10 * multiplier;
        scoreText.text = score.ToString();

        //Check and update the level
        CheckLevel();

        if (score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    public void AddScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();

        //Check and update the level
        CheckLevel();

        if (score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    void CheckLevel()
    {
        //500 to 1000
        if (score > 500 && score < 1000)
        {
            spawner.SetLevel(1);
            scrollSpeed = -6;
        }
        else if (score > 1000 && score < 2500)
        {
            spawner.SetLevel(2);
            scrollSpeed = -7.5f;
        }
        else if (score > 2500)
        {
            spawner.SetLevel(3);
            scrollSpeed = -10.0f;
        }
    }

    public void ResetGame()
    {
        //Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToLanding()
    {
        if (!DataCarrier.Instance.isGuest)
        {
            //PFDataMgr.SetUserData((int)LevelSystem.Instance.currentXp, LevelSystem.Instance.nextLevelXp, LevelSystem.Instance.level);
            PFDataMgr.UpdateDataJSON(LevelSystem.Instance.level, (int)LevelSystem.Instance.maxLevel, LevelSystem.Instance.currentXp, LevelSystem.Instance.nextLevelXp, LevelSystem.Instance.skillPoints);
        }
        SceneManager.LoadScene("Landing");
    }

    public void GameOver()
    {
        gameOverObject.SetActive(true);
        isGameOver = true;
        CancelInvoke();

        finalScoreText.text = score.ToString();
        finalHighScoreText.text = highestScore.ToString();

        LevelSystem.Instance.GainExperienceFlatRate(score * 0.1f);

        if (!DataCarrier.Instance.isGuest)
            PFDataMgr.AddPlayerScore(int.Parse(finalScoreText.text));

        AddMoney();
    }

    private void SaveHighScore(int score)
    {
        highestScore = score;
        PlayerPrefs.SetInt("highestScore", highestScore);
        //highScoreText.text = highestScore.ToString();
    }

    private void LoadHighScore()
    {
        if(PlayerPrefs.HasKey("highestScore"))
        {
            highestScore = PlayerPrefs.GetInt("highestScore");
            //highScoreText.text = highestScore.ToString();
        }
    }

    private void AddMoney()
    {
        var addMoneyReq = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "CC",
            Amount = score / 50
        };

        PlayFabClientAPI.AddUserVirtualCurrency(addMoneyReq, r =>
        {
            coin_Text.text = (score / 50).ToString();
            Debug.Log("Successfully Added Money");
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
        });

    }
}
