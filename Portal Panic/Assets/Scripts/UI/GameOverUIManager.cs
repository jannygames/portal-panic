using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    [Header("GameOver UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText; // Text field to display all statistics
    [SerializeField] private Button restartButton; // Button to restart the game
    [SerializeField] private Button mainMenuButton; // Button to return to main menu

    void Start()
    {
        // Display rounds survived, kills, and score on separate lines
        if (gameOverText != null && KillCounterManager.Instance != null)
        {
            int roundsSurvived = KillCounterManager.Instance.GetRoundsSurvived();
            int totalKills = KillCounterManager.Instance.GetTotalKills();
            int score = KillCounterManager.Instance.GetScore();

            gameOverText.text = $"RoundsSurvived: {roundsSurvived}\n" +
                                $"EnemiesKilled: {totalKills}\n" +
                                $"Score: {score}";
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void RestartGame()
    {
        // Reset the counters and reload the game scene
        if (KillCounterManager.Instance != null)
        {
            KillCounterManager.Instance.ResetCounters();
        }

        SceneManager.LoadScene("GameScene"); // Replace with your actual game scene name
    }

    private void ReturnToMainMenu()
    {
        // Reset the counters and return to main menu
        if (KillCounterManager.Instance != null)
        {
            KillCounterManager.Instance.ResetCounters();
        }

        SceneManager.LoadScene("MainMenu");
    }
}