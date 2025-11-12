using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI hudText; // Text element for the HUD
    [SerializeField] private Image[] hearts; // Array of heart images

    /// <summary>
    /// Updates the HUD text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void UpdateHUDText(string message)
    {
        if (hudText != null)
        {
            hudText.text = message;
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
}