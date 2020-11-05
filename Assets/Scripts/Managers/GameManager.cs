using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private enum GamePhase
    {
        splashScreen = 0,
        characterSelect = 1,
        preparingRound = 2,
        playingRound = 3,
        concludingRound = 4,
        gameOver = 5
    }

    private struct Player
    {
        public int playerIndex;
        public bool isReady;
        public int winsCount;
    }

    [Header("UI")]
    [SerializeField] private GameObject splashScreen = null;
    [SerializeField] private GameObject gameGUIPanel = null;
    [SerializeField] private GameObject characterSelectUI = null;
    [SerializeField] private GameObject roundUI = null;
    [SerializeField] private GameObject winScreen = null;

    [SerializeField] private Text[] readyUI = null;

    [SerializeField] private Text roundText = null;
    [SerializeField] private Text roundTimeText = null;
    [SerializeField] private Text[] playersWinCount = null;

    [SerializeField] private Text winnerAnnouncement = null;

    [SerializeField] private Image[] playersHealthBar = null;
    [SerializeField] private Gradient healthBarGradient = null;

    private GamePhase currentPhase = GamePhase.splashScreen;
    private CharacterManager characterManager = null;

    private Player[] players = null;

    private int currentRound = 0;

    private int roundSecondsLeft = 0;
    private float roundEndTime = 0f;

    private const int playersCount = 2;
    private const int roundTime = 10;

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

        if (currentPhase == GamePhase.gameOver)
            ProcessGameConclusion();
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
            players[i].winsCount = 0;
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

        SubscribeToCharacterEvents();

        StartCoroutine(RoundStartRoutine());
        currentPhase = GamePhase.preparingRound;
    }

    IEnumerator RoundStartRoutine()
    {
        currentRound++;
        roundText.text = "Round " + currentRound;

        int secondsLeft = 3;
        float startTime = Time.time + secondsLeft;
        roundTimeText.text = secondsLeft.ToString();

        // Reset characters.
        characterManager.ResetCharacters();

        // Set healthbars.
        for (int i = 0; i < playersCount; i++)
            UpdatePlayerHealthBar(i, 1f);

        // Countdown to start round.
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

        // Preview "GO" for 1 second.
        float goPreview = Time.time + 1;

        while (Time.time < goPreview)
        {
            yield return null;
        }        

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
        else if (roundSecondsLeft == 0)
        {
            roundTimeText.text = (0).ToString();
            ConcludeRound();
            return;
        }
    }

    void SubscribeToCharacterEvents()
    {
        characterManager.OnCharacterTakenDamage += OnPlayerTakenDamage;
    }

    void OnPlayerTakenDamage(int _playerIndex, int _playerHealth)
    {
        float ratio = (float)_playerHealth / characterManager.CharacterMaxHealth;

        UpdatePlayerHealthBar(_playerIndex, ratio);

        if (_playerHealth == 0)
        {
            ConcludeRound();
            return;
        }
    }

    void UpdatePlayerHealthBar(int _playerIndex, float _ratio)
    {
        playersHealthBar[_playerIndex].fillAmount = _ratio;
        playersHealthBar[_playerIndex].color = healthBarGradient.Evaluate(_ratio);
    }

    void ConcludeRound()
    {
        currentPhase = GamePhase.concludingRound;

        int? winnerIndex = characterManager.RoundWinnerIndex();

        // Lock player characters.
        characterManager.LockCharacters(true);
        // Standby characters.
        characterManager.StandbyCharacters();

        // If someone won the round.
        if (winnerIndex != null)
        {
            players[winnerIndex.Value].winsCount++;
            playersWinCount[winnerIndex.Value].text = players[winnerIndex.Value].winsCount.ToString();

            // If a player won two rounds, conclude the game.
            if (players[winnerIndex.Value].winsCount == 2)
            {
                ConcludeGame(winnerIndex.Value);
                return;
            }
        }        

        StartCoroutine(RoundStartRoutine());
        currentPhase = GamePhase.preparingRound;
    }

    void ConcludeGame(int _winnerIndex)
    {
        currentPhase = GamePhase.gameOver;

        winnerAnnouncement.text = "Player " + (_winnerIndex + 1).ToString() + " wins!";
        winScreen.SetActive(true);

        characterManager.LockCharacters(true);
        characterManager.ResetCharacters();
    }

    void ProcessGameConclusion()
    {
        if (Input.GetButtonDown("Y" + 0) || Input.GetButtonDown("Y" + 1))
        {
            SceneManager.LoadScene(0);
        }
    }
    #endregion
}
