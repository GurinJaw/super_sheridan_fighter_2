using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum GamePhase
    {
        splashScreen = 0,
        characterSelect = 1
    }

    [SerializeField] private GameObject splashScreen = null;
    [SerializeField] private GameObject selectScreen = null;

    private GamePhase currentPhase = GamePhase.splashScreen;

    #region UNITY
    void Start()
    {

    }

    void Update()
    {
        ProcessPlayerInput();
    }
    #endregion

    #region PRIVATE
    void ProcessPlayerInput()
    {
        switch (currentPhase)
        {
            case GamePhase.splashScreen:
                if (Input.GetButtonDown("A0"))
                {
                    splashScreen.SetActive(false);
                    selectScreen.SetActive(true);
                    currentPhase = GamePhase.characterSelect;
                }
                break;
        }
    }
    #endregion
}
