using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private enum GamePhase
    {
        splashScreen = 0,
        characterSelect = 1,
        preparingRound = 2,
        playingRound = 3,
        concludingRound = 4
    }

    private struct Player
    {
        public int playerIndex;
        public bool isReady;
    }

    [SerializeField] private GameObject splashScreen = null;
    [SerializeField] private GameObject gameGUIPanel = null;
    [SerializeField] private GameObject characterSelectUI = null;
    [SerializeField] private GameObject roundUI = null;
    [SerializeField] private Player[] players = null;

    [SerializeField] private Text[] readyUI = null;

    [SerializeField] private Text roundText = null;
    [SerializeField] private Text roundTimeText = null;
    [SerializeField] private Text[] playersWinCount = null;

    private GamePhase currentPhase = GamePhase.splashScreen;
    private CharacterManager characterManager = null;

    private int currentRound = 0;

    private int roundSecondsLeft = 0;
    private float roundEndTime = 0f;

    private const int playersCount = 2;
    private const int roundTime = 99;

    #region UNITY
    void Start()
    {
        splashScreen.SetActive(true);

        characterManager = GetComponent<CharacterManager>();
        InitializePlayers();
    }

    void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            ProcessPlayerInput(players[i].playerIndex);
        }

        if (currentPhase == GamePhase.playingRound)
            ProcessRound();
    }
    #endregion

    #region PRIVATE
    void InitializePlayers()
    {
        players = new Player[playersCount];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new Player();
            players[i].playerIndex = i;
            players[i].isReady = false;
        }
    }

    void ProcessPlayerInput(int _playerIndex)
    {
        switch (currentPhase)
        {
            case GamePhase.splashScreen:
                ProcessSplashScreen(_playerIndex);
                break;
            case GamePhase.characterSelect:
                ProcessPlayerSelect(_playerIndex);
                break;
        }
    }

    void ProcessSplashScreen(int _playerIndex)
    {
        if (Input.GetButtonDown("A" + _playerIndex))
        {
            splashScreen.SetActive(false);
            gameGUIPanel.SetActive(true);
            characterSelectUI.SetActive(true);
            characterManager.Initialize(playersCount, new int[] { 0, 3 });
            currentPhase = GamePhase.characterSelect;
        }
    }

    void ProcessPlayerSelect(int _playerIndex)
    {
        if (players[_playerIndex].isReady) return;

        if (Input.GetButtonDown("A" + _playerIndex))
        {
            players[_playerIndex].isReady = true;
            readyUI[_playerIndex].enabled = true;
            InitializeMatch();
        }
        else
        {
            if (Input.GetButtonDown("RShoulder1" + _playerIndex) || Input.GetButtonDown("LShoulder1" + _playerIndex))
            {
                characterManager.SelectCharacter(_playerIndex);
            }
        }
    }

    void InitializeMatch()
    {
        // If any of the players isn't ready, return.
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].isReady) return;
        }

        // Initialize the characters
        characterManager.InitializeCharacters();
        // Enable UI elements
        characterSelectUI.SetActive(false);
        roundUI.SetActive(true);

        StartCoroutine(RoundStartRoutine());
        currentPhase = GamePhase.preparingRound;
    }

    IEnumerator RoundStartRoutine()
    {
        int secondsLeft = 3;
        float startTime = Time.time + secondsLeft;
        roundTimeText.text = secondsLeft.ToString();

        // Countdown to start round
        while (Time.time < startTime)
        {
            if (startTime - Time.time <= secondsLeft - 1)
            {
                secondsLeft--;
                roundTimeText.text = secondsLeft.ToString();
            }

            yield return null;
        }

        roundTimeText.text = "GO!";

        // Unlock players.
        characterManager.LockCharacters(false);

        // Show health bars.

        // Preview "GO" for 1 second.
        float goPreview = Time.time + 1;

        while (Time.time < goPreview)
        {
            yield return null;
        }

        currentRound++;
        roundText.text = "Round " + currentRound;

        roundTimeText.text = (roundTime - 1).ToString();
        roundSecondsLeft = roundTime - 1;
        roundEndTime = Time.time + roundSecondsLeft;

        currentPhase = GamePhase.playingRound;
    }

    void ProcessRound()
    {
        if (roundEndTime - Time.time <= roundSecondsLeft - 1)
        {
            roundSecondsLeft--;
            roundTimeText.text = roundSecondsLeft.ToString();
        }
        else
        {
            // End round.
            return;
        }
    }
    #endregion
}
