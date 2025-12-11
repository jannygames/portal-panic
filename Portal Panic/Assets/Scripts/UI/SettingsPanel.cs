using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The main settings panel GameObject")]
    public GameObject settingsPanel;
    
    [Header("UI Elements")]
    [Tooltip("Close button for the settings panel")]
    public Button closeButton;
    
    [Tooltip("Apply button for settings changes")]
    public Button applyButton;
    
    [Tooltip("Reset button to restore default settings")]
    public Button resetButton;
    
    [Header("Settings Controls")]
    [Tooltip("Volume slider")]
    public Slider volumeSlider;
    
    [Tooltip("Graphics quality dropdown")]
    public Dropdown graphicsQualityDropdown;
    
    [Tooltip("Toggle for haptic feedback")]
    public Toggle hapticFeedbackToggle;
    
    [Tooltip("Toggle for comfort mode")]
    public Toggle comfortModeToggle;
    
    // Default values
    private float defaultVolume = 1.0f;
    private int defaultGraphicsQuality = 2;
    private bool defaultHapticFeedback = true;
    private bool defaultComfortMode = false;
    
    /// <summary>
    /// Initialize the settings panel
    /// </summary>
    void Start()
    {
        InitializeSettings();
        SetupButtonListeners();
        LoadSettings();
    }
    
    /// <summary>
    /// Initialize default settings values
    /// </summary>
    void InitializeSettings()
    {
        // Set up graphics quality options
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.ClearOptions();
            graphicsQualityDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Low",
                "Medium", 
                "High",
                "Ultra"
            });
        }
    }
    
    /// <summary>
    /// Setup button click listeners
    /// </summary>
    void SetupButtonListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySettings);
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
        }
    }
    
    /// <summary>
    /// Load current settings into UI
    /// </summary>
    void LoadSettings()
    {
        // Load volume
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", defaultVolume);
        }
        
        // Load graphics quality
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", defaultGraphicsQuality);
        }
        
        // Load haptic feedback setting
        if (hapticFeedbackToggle != null)
        {
            hapticFeedbackToggle.isOn = PlayerPrefs.GetInt("HapticFeedback", defaultHapticFeedback ? 1 : 0) == 1;
        }
        
        // Load comfort mode setting
        if (comfortModeToggle != null)
        {
            comfortModeToggle.isOn = PlayerPrefs.GetInt("ComfortMode", defaultComfortMode ? 1 : 0) == 1;
        }
    }
    
    /// <summary>
    /// Apply current settings and save them
    /// </summary>
    public void ApplySettings()
    {
        // Apply volume
        if (volumeSlider != null)
        {
            AudioListener.volume = volumeSlider.value;
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        }
        
        // Apply graphics quality
        if (graphicsQualityDropdown != null)
        {
            QualitySettings.SetQualityLevel(graphicsQualityDropdown.value);
            PlayerPrefs.SetInt("GraphicsQuality", graphicsQualityDropdown.value);
        }
        
        // Apply haptic feedback setting
        if (hapticFeedbackToggle != null)
        {
            PlayerPrefs.SetInt("HapticFeedback", hapticFeedbackToggle.isOn ? 1 : 0);
            // You can add haptic feedback control logic here
        }
        
        // Apply comfort mode setting
        if (comfortModeToggle != null)
        {
            PlayerPrefs.SetInt("ComfortMode", comfortModeToggle.isOn ? 1 : 0);
            // You can add comfort mode logic here (like reducing motion sickness effects)
        }
        
        PlayerPrefs.Save();
        Debug.Log("Settings applied and saved");
    }
    
    /// <summary>
    /// Reset all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = defaultVolume;
        }
        
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.value = defaultGraphicsQuality;
        }
        
        if (hapticFeedbackToggle != null)
        {
            hapticFeedbackToggle.isOn = defaultHapticFeedback;
        }
        
        if (comfortModeToggle != null)
        {
            comfortModeToggle.isOn = defaultComfortMode;
        }
        
        Debug.Log("Settings reset to defaults");
    }
    
    /// <summary>
    /// Close the settings panel
    /// </summary>
    public void ClosePanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Also try to find and use the VRSettingsManager if available
        VRSettingsManager settingsManager = Object.FindFirstObjectByType<VRSettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.CloseSettingsPanel();
        }
    }
    
    /// <summary>
    /// Called when the panel is enabled
    /// </summary>
    void OnEnable()
    {
        LoadSettings();
    }
}
