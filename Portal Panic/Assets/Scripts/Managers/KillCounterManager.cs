using UnityEngine;
using UnityEngine.SceneManagement;

public class KillCounterManager : MonoBehaviour
{
	public static KillCounterManager Instance { get; private set; }

	private int totalKills = 0;
	private int roundsSurvived = 0;
	private int score = 0;

	[SerializeField] private HUDController hudController;

	// --- New scoring mechanics ---
	[Header("Scoring Settings")]
	[SerializeField] private int basePoints = 50;          // base points per kill (lower since enemies die fast)
	[SerializeField] private int headshotBonus = 115;       // extra points for headshots
	[SerializeField] private int roundBonusMultiplier = 250; // bonus per round survived
	[SerializeField] private float comboWindow = 3f;       // time window for combo chaining

	private int killStreak = 0;        // consecutive kills without damage
	private int comboMultiplier = 1;   // multiplier for rapid kills
	private float comboTimer = 0f;     // countdown for combo chaining

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Try to find HUDController in the new scene
		hudController = FindFirstObjectByType<HUDController>();
		if (hudController != null)
		{
			hudController.UpdateScoreText($"Pts. {score}");
		}
	}

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

	void Update()
	{
		// Handle combo timer countdown
		if (comboTimer > 0)
		{
			comboTimer -= Time.deltaTime;
			if (comboTimer <= 0)
			{
				comboMultiplier = 1; // reset combo if timer expires
			}
		}
	}

	/// <summary>
	/// Increments the kill count and score.
	/// </summary>
	public void AddKill(bool isHeadshot = false)
	{
		totalKills++;
		killStreak++;

		// Handle combo multiplier
		if (comboTimer > 0)
		{
			comboMultiplier++;
		}
		else
		{
			comboMultiplier = 1;
		}
		comboTimer = comboWindow; // reset combo window

		// Calculate points
		int pointsEarned = basePoints * comboMultiplier;
		if (isHeadshot) pointsEarned += headshotBonus;

		score += pointsEarned;

		if (hudController != null)
		{
			hudController.UpdateScoreText($"Pts. {score}");
		}

		Debug.Log($"Enemy killed! Streak: {killStreak}, Combo x{comboMultiplier}, Headshot: {isHeadshot}, Earned: {pointsEarned}, Total Score: {score}");
	}

	/// <summary>
	/// Resets kill streak (e.g., when player takes damage).
	/// </summary>
	public void ResetStreak()
	{
		killStreak = 0;
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

		// Heal player by 1 heart if not at max
		if (Instance != null)
		{
			PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
			if (player != null)
			{
				player.HealOneHeart();
			}
		}
	}

	/// <summary>
	/// Awards round survival bonus.
	/// </summary>
	public void EndRound()
	{
		int roundBonus = roundsSurvived * roundBonusMultiplier;
		score += roundBonus;

		if (hudController != null)
		{
			hudController.UpdateScoreText($"Pts. {score}");
		}

		Debug.Log($"Round survived! Bonus {roundBonus} points. Total Score: {score}");
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
		killStreak = 0;
		comboMultiplier = 1;
		comboTimer = 0f;

		if (hudController != null)
		{
			hudController.UpdateScoreText($"Pts. {score}");
			hudController.ResetHUD(); // restore alphas so score/round are visible again
		}
	}
}
