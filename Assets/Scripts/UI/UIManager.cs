using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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

    void Start()
    {
        splashScreen.SetActive(true);
    }

    #region PUBLIC API
    /// <summary>
    /// Go to selection screen.
    /// </summary>
    public void GoToSelectScreen()
    {
        splashScreen.SetActive(false);
        gameGUIPanel.SetActive(true);
        characterSelectUI.SetActive(true);
    }

    /// <summary>
    /// Set a player as ready.
    /// </summary>
    /// <param name="_playerIndex">Player index.</param>
    public void PlayerReady(int _playerIndex)
    {
        readyUI[_playerIndex].enabled = true;
    }

    /// <summary>
    /// Enable the match UI.
    /// </summary>
    public void EnableMatchUI()
    {
        characterSelectUI.SetActive(false);
        roundUI.SetActive(true);
    }

    /// <summary>
    /// Update the round count.
    /// </summary>
    /// <param name="_roundCount">Round number.</param>
    public void UpdateRound(int _roundCount)
    {
        roundText.text = "Round " + _roundCount;
    }

    /// <summary>
    /// Update the time left in seconds.
    /// </summary>
    /// <param name="_secondsLeft">Seconds left in the round.</param>
    public void UpdateTimeLeft(int _secondsLeft)
    {
        roundTimeText.text = _secondsLeft.ToString();
    }

    /// <summary>
    /// Update the message displayed during the gameplay.
    /// </summary>
    /// <param name="_message">The message.</param>
    public void UpdateGameplayMessage(string _message)
    {
        roundTimeText.text = _message;
    }

    /// <summary>
    /// Update the player's health bar.
    /// </summary>
    /// <param name="_playerIndex">Player index.</param>
    /// <param name="_ratio">Bar ratio. (0f to 1f)</param>
    public void UpdatePlayerHealthBar(int _playerIndex, float _ratio)
    {
        playersHealthBar[_playerIndex].fillAmount = _ratio;
        playersHealthBar[_playerIndex].color = healthBarGradient.Evaluate(_ratio);
    }

    /// <summary>
    /// Updates a player's wins count.
    /// </summary>
    /// <param name="_playerIndex">Player index.</param>
    /// <param name="_winsCount">Player's wins count.</param>
    public void UpdatePlayerWinsCount(int _playerIndex, int _winsCount)
    {
        playersWinCount[_playerIndex].text = _winsCount.ToString();
    }

    /// <summary>
    /// Announce a player as winner.
    /// </summary>
    /// <param name="_winnerIndex">Player index.</param>
    public void AnnounceWinner(int _winnerIndex)
    {
        winnerAnnouncement.text = "Player " + (_winnerIndex + 1).ToString() + " wins!";
        winScreen.SetActive(true);
    }
    #endregion
}
