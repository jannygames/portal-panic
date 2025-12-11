using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI hudTextRound;
	[SerializeField] private TextMeshProUGUI hudTextScore;
	[SerializeField] private TextMeshProUGUI hudTextNextRound;
	[SerializeField] private TextMeshProUGUI hudTextAmmo;
	[SerializeField] private TextMeshProUGUI hudTextGameOver;
	[SerializeField] private Image[] hearts;

	void Start()
	{
		hudTextGameOver.alpha = 0;
	}

    /// <summary>
    /// Updates the HUD text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void UpdateRoundText(string message)
    {
        if (hudTextRound != null)
        {
			hudTextRound.text = message;
        }
    }

	public void UpdateScoreText(string message)
	{
		if (hudTextScore != null)
		{
			hudTextScore.text = message;
		}
	}

	public void UpdateNextRoundText(string message)
	{
		if (hudTextNextRound != null)
		{
			hudTextNextRound.text = message;
		}
	}

	public void UpdateAmmoText(string message)
	{
		if (hudTextAmmo != null)
		{
			hudTextAmmo.text = message;
		}
	}

	public IEnumerator FadeGameOverText()
	{
		float t = 0f;
		while (t < 1.5f)
		{
			t += Time.deltaTime;
			hudTextGameOver.alpha = Mathf.Lerp(0f, 1f, t);
			hudTextAmmo.alpha = Mathf.Lerp(1f, 0f, t);
			hudTextNextRound.alpha = Mathf.Lerp(1f, 0f, t);
			hudTextScore.alpha = Mathf.Lerp(1f, 0f, t);
			hudTextRound.alpha = Mathf.Lerp(1f, 0f, t);
			yield return null;
		}
	}

	public void UpdateGameOverText(string message)
	{
		if (hudTextGameOver != null)
		{
			hudTextGameOver.text = message;
		}
	}

	/// <summary>
	/// Updates the hearts display based on the player's current health.
	/// </summary>
	/// <param name="currentHealth">The player's current health.</param>
	public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].enabled = i < currentHealth; // Show hearts up to the current health
            }
        }
    }

	public void ResetHUD()
	{
		if (hudTextScore != null) hudTextScore.alpha = 1f;
		if (hudTextRound != null) hudTextRound.alpha = 1f;
		if (hudTextNextRound != null) hudTextNextRound.alpha = 1f;
		if (hudTextAmmo != null) hudTextAmmo.alpha = 1f;
		if (hudTextGameOver != null) hudTextGameOver.alpha = 0f; // hide game over again
	}
}