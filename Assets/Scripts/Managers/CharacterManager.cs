using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs = null;
    [SerializeField] private Transform[] spawnPoints = null;

    [SerializeField] private Text[] playerNameUI = null;

    private int[] playersSelectionIndex = null;
    private GameObject[] playerPrefabs = null;

    #region UNITY
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
        playersSelectionIndex = _playersSelection;

        SpawnCharacters();
    }

    /// <summary>
    /// Initialize the characters.
    /// </summary>
    public void InitializeCharacters()
    {
        playerPrefabs[0].GetComponent<CharacterController>().InitializeCharacter(0, playerPrefabs[1].transform);
        playerPrefabs[1].GetComponent<CharacterController>().InitializeCharacter(1, playerPrefabs[0].transform);
    }

    /// <summary>
    /// Unlock characters movement.
    /// </summary>
    /// <param name="_lock">Lock</param>
    public void LockCharacters(bool _lock)
    {
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            playerPrefabs[i].GetComponent<CharacterController>().LockPlayer(_lock);
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
    #endregion
}
