using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    // PlayerPrefs keys for saving stats
    private const string BEST_SCORE_KEY = "BestScore";
    private const string BEST_KILLS_KEY = "BestKills";
    private const string BEST_ROUNDS_KEY = "BestRounds";

    [Header("GameOver UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText; // Text field to display all statistics
    [SerializeField] private Button restartButton; // Button to restart the game
    [SerializeField] private Button mainMenuButton; // Button to return to main menu

    void Start()
    {
        // Save and display statistics
        if (KillCounterManager.Instance != null)
        {
            int roundsSurvived = KillCounterManager.Instance.GetRoundsSurvived();
            int totalKills = KillCounterManager.Instance.GetTotalKills();
            int score = KillCounterManager.Instance.GetScore();

            // Save current game stats and update best scores
            SaveGameStats(score, totalKills, roundsSurvived);

            // Get best scores from PlayerPrefs
            int bestScore = GetBestScore();
            int bestKills = GetBestKills();
            int bestRounds = GetBestRounds();

            // Display current and best statistics
            if (gameOverText != null)
            {
                gameOverText.text = $"Current Game:\n" +
                                    $"Rounds Survived: {roundsSurvived}\n" +
                                    $"Enemies Killed: {totalKills}\n" +
                                    $"Score: {score}\n\n" +
                                    $"- R e c o r d s -\n" +
                                    $"Rounds Survived: {bestRounds}\n" +
                                    $"Enemies Killed: {bestKills}\n" +
                                    $"Score: {bestScore}";
            }
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

    /// <summary>
    /// Saves current game stats and updates best scores if they are higher
    /// </summary>
    private void SaveGameStats(int score, int kills, int rounds)
    {
        // Update best score if current score is higher
        int bestScore = GetBestScore();
        if (score > bestScore)
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, score);
            Debug.Log($"New best score: {score}!");
        }

        // Update best kills if current kills are higher
        int bestKills = GetBestKills();
        if (kills > bestKills)
        {
            PlayerPrefs.SetInt(BEST_KILLS_KEY, kills);
            Debug.Log($"New best kills: {kills}!");
        }

        // Update best rounds if current rounds are higher
        int bestRounds = GetBestRounds();
        if (rounds > bestRounds)
        {
            PlayerPrefs.SetInt(BEST_ROUNDS_KEY, rounds);
            Debug.Log($"New best rounds: {rounds}!");
        }

        // Save PlayerPrefs
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Gets the best score from PlayerPrefs
    /// </summary>
    public static int GetBestScore()
    {
        return PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }

    /// <summary>
    /// Gets the best kills from PlayerPrefs
    /// </summary>
    public static int GetBestKills()
    {
        return PlayerPrefs.GetInt(BEST_KILLS_KEY, 0);
    }

    /// <summary>
    /// Gets the best rounds survived from PlayerPrefs
    /// </summary>
    public static int GetBestRounds()
    {
        return PlayerPrefs.GetInt(BEST_ROUNDS_KEY, 0);
    }

    /// <summary>
    /// Resets all best scores (useful for testing or reset functionality)
    /// </summary>
    public static void ResetBestScores()
    {
        PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
        PlayerPrefs.DeleteKey(BEST_KILLS_KEY);
        PlayerPrefs.DeleteKey(BEST_ROUNDS_KEY);
        PlayerPrefs.Save();
        Debug.Log("Best scores reset!");
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