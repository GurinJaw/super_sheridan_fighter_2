using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class CharacterManager : MonoBehaviour
{
    public delegate void PlayerDamageDelegate(int _characterIndex, int _playerHealth);
    public PlayerDamageDelegate OnCharacterTakenDamage = null;

    [SerializeField] private GameObject[] characterPrefabs = null;
    [SerializeField] private Transform[] spawnPoints = null;

    [SerializeField] private Text[] playerNameUI = null;

    private CharacterController[] characterControllers = null;

    private int[] playersSelectionIndex = null;
    private GameObject[] playerPrefabs = null;

    #region UNITY
    private void OnDestroy()
    {
        if (characterControllers != null)
        {
            for (int i = 0; i < characterControllers.Length; i++)
            {
                if (characterControllers[i] != null)
                    characterControllers[i].OnTakenDamage -= OnTakenDamage;
            }
        }
    }
    #endregion

    #region PUBLIC API
    /// <summary>
    /// Initialize the character manager.
    /// </summary>
    /// <param name="_playersCount">Number of players.</param>
    /// <param name="_playersSelection">Current players selection.</param>
    public void Initialize(int _playersCount, int[] _playersSelection)
    {
        playerPrefabs = new GameObject[_playersCount];
        characterControllers = new CharacterController[_playersCount];

        playersSelectionIndex = _playersSelection;

        SpawnCharacters();
    }

    /// <summary>
    /// Initialize the characters.
    /// </summary>
    public void InitializeCharacters()
    {
        for (int i = 0; i < characterControllers.Length; i++)
        {
            characterControllers[i] = playerPrefabs[i].GetComponent<CharacterController>();
            characterControllers[i].OnTakenDamage += OnTakenDamage;
        }

        characterControllers[0].InitializeCharacter(0, playerPrefabs[1].transform, CharacterMaxHealth);
        characterControllers[1].InitializeCharacter(1, playerPrefabs[0].transform, CharacterMaxHealth);
    }

    /// <summary>
    /// Unlock characters movement.
    /// </summary>
    /// <param name="_lock">Lock</param>
    public void LockCharacters(bool _lock)
    {
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            characterControllers[i].LockCharacter(_lock);
        }
    }

    /// <summary>
    /// Toggle a new character for the current player.
    /// </summary>
    /// <param name="_playerIndex">Player index.</param>
    public void SelectCharacter(int _playerIndex)
    {
        // Toggle between two characters for each player.
        if (_playerIndex * 2 == playersSelectionIndex[_playerIndex])
        {
            playersSelectionIndex[_playerIndex]++;
        }
        else
        {
            playersSelectionIndex[_playerIndex] = _playerIndex * 2;
        }

        SpawnCharacter(_playerIndex);
    }

    /// <summary>
    /// Max player health.
    /// </summary>
    public int CharacterMaxHealth { get { return 100; } }

    /// <summary>
    /// Round winner index.
    /// </summary>
    /// <returns></returns>
    public int? RoundWinnerIndex()
    {
        if (characterControllers[0].CurrentHealth > characterControllers[1].CurrentHealth)
        {
            return 0;
        }
        else if (characterControllers[1].CurrentHealth > characterControllers[0].CurrentHealth)
        {
            return 1;
        }

        return null;
    }

    /// <summary>
    /// Reset characters.
    /// </summary>
    public void ResetCharacters()
    {
        for (int i = 0; i < characterControllers.Length; i++)
        {
            characterControllers[i].ResetCharacter();
            characterControllers[i].transform.position = spawnPoints[i].position;
        }
    }

    /// <summary>
    /// Resets character properties immediately after a round ends.
    /// </summary>
    public void StandbyCharacters()
    {
        for (int i = 0; i < characterControllers.Length; i++)
        {
            characterControllers[i].ResetFireOrbs();
            characterControllers[i].ResetAnimator();
        }
    }

    /// <summary>
    /// Returns an array of the transforms of the characters.
    /// </summary>
    /// <returns></returns>
    public Transform[] GetCharacterTransforms()
    {
        if (characterControllers == null) return null;

        Transform[] characterTransforms = new Transform[characterControllers.Length];

        for (int i = 0; i < characterControllers.Length; i++)
        {
            characterTransforms[i] = characterControllers[i].transform;
        }

        return characterTransforms;
    }
    #endregion

    #region PRIVATE
    void SpawnCharacters()
    {
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            SpawnCharacter(i);
        }
    }

    void SpawnCharacter(int _playerIndex)
    {
        if (playerPrefabs[_playerIndex] != null) Destroy(playerPrefabs[_playerIndex].gameObject);

        InstantiatePrefab(_playerIndex, playersSelectionIndex[_playerIndex], spawnPoints[_playerIndex]);
    }

    void InstantiatePrefab(int _playerIndex, int _prefabIndex, Transform _spawnPoint)
    {
        playerPrefabs[_playerIndex] = Instantiate(characterPrefabs[_prefabIndex]);
        playerPrefabs[_playerIndex].transform.position = _spawnPoint.position;
        playerPrefabs[_playerIndex].transform.rotation = _spawnPoint.rotation;
        playerNameUI[_playerIndex].text = "P." + (_playerIndex + 1) + ": " + playerPrefabs[_playerIndex].GetComponent<CharacterController>().GetCharacterName();
    }

    void OnTakenDamage(int _characterIndex, int _playerHealth)
    {
        if (OnCharacterTakenDamage != null)
            OnCharacterTakenDamage(_characterIndex, _playerHealth);
    }
    #endregion
}
