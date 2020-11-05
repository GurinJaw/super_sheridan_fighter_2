using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum GamePhase
    {
        splashScreen = 0,
        characterSelect = 1,
        prepareRound = 2
    }

    private struct Player
    {
        public int playerIndex;
        public bool isReady;
    }

    [SerializeField] private GameObject splashScreen = null;
    [SerializeField] private GameObject selectScreen = null;
    [SerializeField] private Player[] players = null;

    private GamePhase currentPhase = GamePhase.splashScreen;
    private CharacterManager characterManager = null;
    private const int playersCount = 2;

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
            selectScreen.SetActive(true);
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
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].isReady) return;
        }

        characterManager.InitializeCharacters();

        currentPhase = GamePhase.prepareRound;
    }
    #endregion
}
