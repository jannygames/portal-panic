using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRSettingsManager : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("The input action that triggers the settings menu (typically the menu button)")]
    public InputActionProperty menuButtonAction;
    
    [Header("Settings Panel")]
    [Tooltip("The settings panel GameObject to toggle")]
    public GameObject settingsPanel;
    
    [Header("Laser Pointer Integration")]
    [Tooltip("Reference to the VRLaserPointer component")]
    public VRLaserPointer laserPointer;
    
    [Header("Settings")]
    [Tooltip("Should the settings panel start as active?")]
    public bool startWithPanelOpen = false;
    
    private bool isSettingsPanelOpen = false;

    
    /// <summary>
    /// Initialize the settings manager
    /// </summary>
    void Start()
    {
        // Set initial state of settings panel
        if (settingsPanel != null)
        {
            isSettingsPanelOpen = startWithPanelOpen;
            settingsPanel.SetActive(isSettingsPanelOpen);
        }
        
        // Subscribe to menu button input
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.performed += OnMenuButtonPressed;
        }
    }
    
    /// <summary>
    /// Handle menu button press
    /// </summary>
    private void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        ToggleSettingsPanel();
    }
    
    /// <summary>
    /// Toggle the settings panel open/closed
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        
        isSettingsPanelOpen = !isSettingsPanelOpen;
        settingsPanel.SetActive(isSettingsPanelOpen);
        
        // Enable/disable laser pointer based on settings panel state
        if (laserPointer != null)
        {
            laserPointer.SetLaserActive(isSettingsPanelOpen);
        }
        
        Debug.Log($"Settings panel {(isSettingsPanelOpen ? "opened" : "closed")}");
    }
    
    /// <summary>
    /// Open the settings panel
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel == null) return;
        
        isSettingsPanelOpen = true;
        settingsPanel.SetActive(true);
        
        // Enable laser pointer when settings panel opens
        if (laserPointer != null)
        {
            laserPointer.SetLaserActive(true);
        }
        
        Debug.Log("Settings panel opened");
    }
    
    /// <summary>
    /// Close the settings panel
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;
        
        isSettingsPanelOpen = false;
        settingsPanel.SetActive(false);
        
        // Disable laser pointer when settings panel closes
        if (laserPointer != null)
        {
            laserPointer.SetLaserActive(false);
        }
        
        Debug.Log("Settings panel closed");
    }
    
    /// <summary>
    /// Check if settings panel is currently open
    /// </summary>
    public bool IsSettingsPanelOpen()
    {
        return isSettingsPanelOpen;
    }
    
    /// <summary>
    /// Clean up event subscriptions
    /// </summary>
    void OnDestroy()
    {
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.performed -= OnMenuButtonPressed;
        }
    }
    
    /// <summary>
    /// Enable/disable input handling
    /// </summary>
    void OnEnable()
    {
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.Enable();
        }
    }
    
    void OnDisable()
    {
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.Disable();
        }
    }
}
