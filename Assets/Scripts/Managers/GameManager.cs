using System.Collections;
using UnityEngine;
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

    [Header("Camera")]
    [SerializeField] private CameraController cameraController = null;

    [Header("UI")]
    [SerializeField] private UIManager uiManager = null;

    private GamePhase currentPhase = GamePhase.splashScreen;
    private CharacterManager characterManager = null;

    private Player[] players = null;

    private int currentRound = 0;

    private int roundSecondsLeft = 0;
    private float roundEndTime = 0f;

    private const int playersCount = 2;
    private const int roundTime = 99;

    #region UNITY
    void Start()
    {
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
            uiManager.GoToSelectScreen();
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
            uiManager.PlayerReady(_playerIndex);
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
        uiManager.EnableMatchUI();

        Transform[] characterTransforms = characterManager.GetCharacterTransforms();

        if (characterTransforms != null && characterTransforms.Length > 1)
            cameraController.Initialize(characterTransforms[0], characterTransforms[1]);

        SubscribeToCharacterEvents();

        StartCoroutine(RoundStartRoutine());
        currentPhase = GamePhase.preparingRound;
    }

    IEnumerator RoundStartRoutine()
    {
        currentRound++;
        uiManager.UpdateRound(currentRound);

        int secondsLeft = 3;
        float startTime = Time.time + secondsLeft;
        uiManager.UpdateTimeLeft(secondsLeft);

        // Reset characters.
        characterManager.ResetCharacters();

        // Set healthbars.
        for (int i = 0; i < playersCount; i++)
            uiManager.UpdatePlayerHealthBar(i, 1f);

        // Countdown to start round.
        while (Time.time < startTime)
        {
            if (startTime - Time.time <= secondsLeft - 1)
            {
                secondsLeft--;
                uiManager.UpdateTimeLeft(secondsLeft);
            }

            yield return null;
        }

        uiManager.UpdateGameplayMessage("GO!");

        // Unlock players.
        characterManager.LockCharacters(false);

        // Preview "GO" for 1 second.
        float goPreview = Time.time + 1;

        while (Time.time < goPreview)
        {
            yield return null;
        }

        uiManager.UpdateTimeLeft(roundTime - 1);
        roundSecondsLeft = roundTime - 1;
        roundEndTime = Time.time + roundSecondsLeft;

        currentPhase = GamePhase.playingRound;
    }

    void ProcessRound()
    {
        if (roundEndTime - Time.time <= roundSecondsLeft - 1)
        {
            roundSecondsLeft--;
            uiManager.UpdateTimeLeft(roundSecondsLeft);
        }
        else if (roundSecondsLeft == 0)
        {
            uiManager.UpdateTimeLeft(0);
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

        uiManager.UpdatePlayerHealthBar(_playerIndex, ratio);

        if (_playerHealth == 0)
        {
            ConcludeRound();
            return;
        }
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
            uiManager.UpdatePlayerWinsCount(winnerIndex.Value, players[winnerIndex.Value].winsCount);

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

        uiManager.AnnounceWinner(_winnerIndex);

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
