using UnityEngine;

public class KillCounterManager : MonoBehaviour
{
    public static KillCounterManager Instance { get; private set; }

    private int totalKills = 0;
    private int roundsSurvived = 0;
    private int score = 0;

    void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads
    }

    /// <summary>
    /// Increments the kill count and score.
    /// </summary>
    public void AddKill()
    {
        totalKills++;
        score += 100; // Add 100 points per kill
        Debug.Log($"Enemy killed! Total kills: {totalKills}, Score: {score}");
    }

    /// <summary>
    /// Gets the total number of kills.
    /// </summary>
    public int GetTotalKills()
    {
        return totalKills;
    }

    /// <summary>
    /// Sets the current round (called when a new wave starts).
    /// </summary>
    public void SetCurrentRound(int round)
    {
        roundsSurvived = round - 1; // Rounds survived = current round - 1
        Debug.Log($"Current round set to: {round}, Rounds survived: {roundsSurvived}");
    }

    /// <summary>
    /// Gets the number of rounds survived.
    /// </summary>
    public int GetRoundsSurvived()
    {
        return roundsSurvived;
    }

    /// <summary>
    /// Gets the current score.
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// Resets all counters.
    /// </summary>
    public void ResetCounters()
    {
        totalKills = 0;
        roundsSurvived = 0;
        score = 0;
    }
}