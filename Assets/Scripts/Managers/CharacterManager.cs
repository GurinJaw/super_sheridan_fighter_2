using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs = null;
    [SerializeField] private Transform spawnPointA = null;
    [SerializeField] private Transform spawnPointB = null;

    private GameObject player1 = null;
    private GameObject player2 = null;

    // Start is called before the first frame update
    void Start()
    {
        SpawnCharacters();
    }

    void SpawnCharacters()
    {
        InstantiatePrefab(ref player1, 0, spawnPointA);
        InstantiatePrefab(ref player2, 1, spawnPointB);

        player1.GetComponent<CharacterController>().InitializeCharacter(0, player2.transform);
        player2.GetComponent<CharacterController>().InitializeCharacter(1, player1.transform);
    }

    void InstantiatePrefab(ref GameObject _player, int _prefabIndex, Transform _spawnPoint)
    {
        _player = Instantiate(characterPrefabs[_prefabIndex]);
        _player.transform.position = _spawnPoint.position;
        _player.transform.rotation = _spawnPoint.rotation;
    }
}
