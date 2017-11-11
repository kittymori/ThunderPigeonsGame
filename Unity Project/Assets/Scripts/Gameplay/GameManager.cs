﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using XboxCtrlrInput;

public class GameManager : MonoBehaviour
{
    float[] scores = new float[4];
    float[] respawnTimer = new float[4];

    [SerializeField]
    float respawnTime;

    [SerializeField]
    int winScore;

    [SerializeField]
    EventSystem Event;

    bool gameWon = false;

    [SerializeField]
    bool allowContest = false;

    [SerializeField]
    float startTime;
    float timer;

    [SerializeField]
    float deathYLevel = -3f;

    [SerializeField]
    [Range(0, 3)]
    float drunknessScoreScale;

    [SerializeField]
    int maximumDrunkeness = 300;

    GameObject zone;
    ZoneControl zoneControl;
    ZoneManager zoneManager;

    // Keep a list of each player in the game
    public GameObject[] players = new GameObject[4];

    [SerializeField] // Each players score text
    Text player1Score, player2Score, player3Score, player4Score;

    [SerializeField]
    Text timeText;

    [SerializeField]
    GameObject outerSpawnParent;
    [SerializeField]
    GameObject centerSpawnParent;

    Transform[] outerSpawns = new Transform[4];
    Transform[] centreSpawns = new Transform[4];

    [Header("")]
    [SerializeField]
    Text winMessageText;
	[SerializeField]
	Text restartMessage;

    [SerializeField]
    [Tooltip("The message to display when a player has won (all lower case x are replaced with the winning player number)")]
    string winMessageString;

    bool gamePaused = false;
    bool options = false;

    [SerializeField]
    [Tooltip("Pause Menu and ingame Options Panel")]
    Transform optionsPanel;
    [SerializeField]
    Transform pauseMenu;

    PlayerColourPicker playerColourPicker;

    int winningPlayerNumber = 0;

    XboxButton backButton = XboxButton.B;
    XboxButton videoTab = XboxButton.LeftBumper;
    XboxButton audioTab = XboxButton.RightBumper;

    [Header("Option Tabs")]
    [SerializeField]
    GameObject pnl_Audio;
    [SerializeField]
    GameObject pnl_Video;
    [SerializeField]
    GameObject res_DropDown;
    [SerializeField]
    GameObject Sldr_Master;
    [SerializeField]
    GameObject btn_Resume;

    void Awake()
    {
        zone = GameObject.FindGameObjectWithTag("Zone");
        zoneControl = zone.GetComponent<ZoneControl>();
        zoneManager = zone.transform.parent.GetComponent<ZoneManager>();
        playerColourPicker = GetComponent<PlayerColourPicker>();

        for (int i = 0; i < 4; i++)
        {
            outerSpawns[i] = outerSpawnParent.transform.GetChild(i);
            centreSpawns[i] = centerSpawnParent.transform.GetChild(i);
        }

    }

    void Start()
    {
        timer = startTime;

        gameWon = false;

        winMessageText.enabled = false;
        restartMessage.enabled = false;

        FindObjectOfType<AudioManager>().Play("Audio-Theme");
        FindObjectOfType<AudioManager>().Play("SFX-Ocean");
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Search for players in the scene
        GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("PlayerBall");
        for (int i = 0; i < playersInScene.Length; i++)
        {
            //DontDestroyOnLoad(playersInScene[i]);
            //playersInScene[i].transform.SetParent(newObject.transform);

            PlayerController playerController = playersInScene[i].GetComponent<PlayerController>();

            players[playerController.playerNumber - 1] = playersInScene[i];

            RespawnPlayer(playerController.playerNumber);
        }

    }

    void Update()
    {
        //RJ codes pause
        //waits for Start to press or escape then pauses game
        if (XCI.GetButtonDown(XboxButton.Start, XboxController.All) || (Input.GetKeyDown(KeyCode.Escape)))
        {
            if (!gamePaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }

        if (options)
        {
            StartOptions();
        }

        // Restart scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Loop through players
        for (int i = 0; i < 4; i++)
        {
            if (players[i] != null)
            {
                // Get the current player
                PlayerController playerController = players[i].GetComponent<PlayerController>();

                // If the player falls below a y level
                if (playerController.transform.position.y <= deathYLevel && playerController.isAlive == true)
                {
                    // Kill the player
                    FindObjectOfType<AudioManager>().Play("SFX-Splash");
                    FindObjectOfType<AudioManager>().Play("VO_Barry_Death");
                    playerController.isAlive = false;
                    respawnTimer[i] = respawnTime;
                }

                // If the player is dead
                if (playerController.isAlive == false)
                {
                    if (respawnTimer[i] <= 0)
                    {
                        RespawnPlayer(playerController.playerNumber);
                    }
                    else
                    {
                        respawnTimer[i] -= Time.deltaTime;
                    }
                }
            }
        }

        UpdateTimer();

        GivePoints();
        SetPlayerDrunkenness();

        // After the scores are added 
        UpdateScoreBoard();

        CheckForWinningPlayer();

        // If the game has been won
        if (gameWon)
        {
            // Turn on the messages
            restartMessage.enabled = true;
            winMessageText.enabled = true;
            SetWinMessage();
            
            // If a is pressed 
            if (XCI.GetButtonDown(XboxButton.A, XboxController.All))
            {
                GoToMainMenu();
            }
            // If b is pressed restart the level
            if (XCI.GetButtonDown(XboxButton.B, XboxController.All))
            {
                RestartScene();
            }
        }
    }

    public void Pause()
    {
        gamePaused = true;

        optionsPanel.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
        Event.SetSelectedGameObject(btn_Resume);
        Time.timeScale = 0; //pauses game
    }

    public void Unpause()
    {
        gamePaused = false;

        optionsPanel.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1; //unpauses game
    }

    public void StartOptions()
    {
        gamePaused = true;
        options = true;
        optionsPanel.gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);
        

        if (XCI.GetButtonDown(backButton, XboxController.All) || Input.GetKeyDown(KeyCode.B))
        {
            options = false;
            Pause();
        }

        if (XCI.GetButtonDown(videoTab, XboxController.All) || Input.GetKeyDown(KeyCode.C))
        {
            pnl_Video.gameObject.SetActive(true);
            pnl_Audio.gameObject.SetActive(false);
            FindObjectOfType<AudioManager>().Play("SFX-Button-Click");
            Event.SetSelectedGameObject(res_DropDown);
        }

        if (XCI.GetButtonDown(audioTab, XboxController.All) || Input.GetKeyDown(KeyCode.V))
        {
            pnl_Video.gameObject.SetActive(false);
            pnl_Audio.gameObject.SetActive(true);
            FindObjectOfType<AudioManager>().Play("SFX-Button-Click");
            Event.SetSelectedGameObject(Sldr_Master);
        }

    }

    public void GoToMainMenu()
    {
        Unpause();
        SceneManager.LoadScene("Main Menu");
        for (int x = 0; x < 4; x++)
        {
            if (players[x] != null)
            {
                Destroy(players[x].transform.parent.gameObject);
            }
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Beta");

        for (int playerNum = 1; playerNum < 5; playerNum++)
        {
            RespawnPlayer(playerNum);
        }
    }

    void RespawnPlayer(int playerNumber)
    {
        if (players[playerNumber - 1] != null)
        {
            if (zoneManager.zonePosition == ZoneManager.ZonePosition.Centre) // If the zone is in the centre
            {
                players[playerNumber - 1].transform.position = outerSpawns[playerNumber - 1].position; // Spawn on outer spawn
            }
            else
            {
                players[playerNumber - 1].transform.position = centreSpawns[playerNumber - 1].position; // Spawn in the centre
            }

            //// Switch on the different players to spawn them in respective spots
            //switch (playerNumber)
            //{
            //    case 1:
            //        players[playerNumber - 1].transform.position = player1Spawn.position;
            //        break;
            //    case 2:
            //        players[playerNumber - 1].transform.position = player2Spawn.position;
            //        break;
            //    case 3:
            //        players[playerNumber - 1].transform.position = player3Spawn.position;
            //        break;
            //    case 4:
            //        players[playerNumber - 1].transform.position = player4Spawn.position;
            //        break;
            //    default:
            //        break;
            //}

            // Set player as alive
            players[playerNumber - 1].GetComponent<PlayerController>().isAlive = true;

            players[playerNumber - 1].GetComponent<Rigidbody>().velocity = Vector3.zero;

            LookRotation lookRotation = players[playerNumber - 1].transform.parent.GetComponentInChildren<LookRotation>();
            lookRotation.LookTowards(zone.transform.position);
        }

        //// Set the gameObject name
        //newPlayer.name = "Player " + playerNumber.ToString();

        //// Add to the player array in the correct spot
        //players[playerNumber - 1] = newPlayer;

        //PlayerController newPlayerController = newPlayer.GetComponentInChildren<PlayerController>();
        //// Set each players number
        //newPlayerController.playerNumber = playerNumber;
        //// Set the controller number
        //newPlayerController.controller = (XboxCtrlrInput.XboxController)playerNumber;
        //// Set player as alive
        //newPlayerController.isAlive = true;

        //LookRotation newLookRotation = newPlayer.GetComponentInChildren<LookRotation>();
        //newLookRotation.StartUp();
        //newLookRotation.LookTowards(zone.transform.position);

        ////TODO do this is a better spot or a better way
        //playerColourPicker.SetUp(players);
    }

    void GivePoints()
    {
        if (gameWon == false)
        {
            // Every second give the players in the zoneControl.playersInZone a point
            if (allowContest && zoneControl.playersInZone.Count == 1)
            {
                foreach (GameObject player in zoneControl.playersInZone)
                {
                    scores[player.GetComponent<PlayerController>().playerNumber - 1] += Time.deltaTime;
                    zoneManager.PointsGiven(Time.deltaTime);
                }
            }
            else if (!allowContest)
            {
                foreach (GameObject player in zoneControl.playersInZone)
                {
                    scores[player.GetComponent<PlayerController>().playerNumber - 1] += Time.deltaTime;
                    zoneManager.PointsGiven(Time.deltaTime);
                }
            }
        }
    }

    void UpdateScoreBoard()
    {
        player1Score.text = ((int)scores[0]).ToString();
        player2Score.text = ((int)scores[1]).ToString();
        player3Score.text = ((int)scores[2]).ToString();
        player4Score.text = ((int)scores[3]).ToString();
    }

    void UpdateTimer()
    {
        // If there is still time left
        if (timer > 0.0f)
        {
            timer -= Time.deltaTime; // Count down
        }
        else
        {
            // Timer Run out!
            SetHightestScorerAsWinner();
            gameWon = true;
        }

        // Miniutes and seconds
        //timeText.text = ((int)(timer / 60f)).ToString() + ":" + ((int)(timer % 60)).ToString();
        // Timer in seconds
        timeText.text = ((int)timer).ToString();
    }

    void SetPlayerDrunkenness()
    {
        for (int i = 0; i < 4; i++)
        {
            if (players[i] != null)
            {
                if (scores[i] * drunknessScoreScale > maximumDrunkeness)
                {
                    players[i].GetComponentInChildren<PlayerController>().drunkenness = maximumDrunkeness;
                }
                else
                {
                    players[i].GetComponentInChildren<PlayerController>().drunkenness = scores[i] * drunknessScoreScale;
                }
            }
        }
    }

    void SetWinMessage()
    {
        if (winningPlayerNumber == 0)
        {
            winMessageText.text = "It's a tie";
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("VO_Barry_Win");
            winMessageText.text = winMessageString.Replace('x', winningPlayerNumber.ToString()[0]);
        }
    }

    void CheckForWinningPlayer()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] >= winScore)
            {
                gameWon = true;
                winningPlayerNumber = i + 1;
            }
        }
    }

    void SetHightestScorerAsWinner()
    {
        float highestScore = 0;
        int playernum = 0;
        for (int i = 0; i < 4; i++)
        {
            if (scores[i] > highestScore)
            {
                highestScore = scores[i];
                playernum = i + 1;
            }
        }

        winningPlayerNumber = playernum;
    }

    public void ExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}

