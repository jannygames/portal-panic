using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class SimpleUIInteractor : MonoBehaviour
{
    [Header("XR Components")]
    [Tooltip("The XR Ray Interactor component (should already exist)")]
    public XRRayInteractor rayInteractor;
    
    [Header("Input Settings")]
    [Tooltip("Input action for UI interaction (typically trigger or primary button)")]
    public InputActionProperty uiInteractAction;
    
    [Header("Settings Panel Integration")]
    [Tooltip("Reference to the VRSettingsManager (optional)")]
    public VRSettingsManager settingsManager;
    
    [Tooltip("Should UI interaction be active only when settings panel is open?")]
    public bool onlyActiveWhenSettingsOpen = false;
    
    private bool isUIInteractionActive = true;
    
    /// <summary>
    /// Initialize the UI interactor
    /// </summary>
    void Start()
    {
        // Get XR Ray Interactor if not assigned
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
        }
        
        // Ensure we have the required components
        if (rayInteractor == null)
        {
            Debug.LogError("XRRayInteractor not found! Please assign or add XRRayInteractor component.");
            return;
        }
        
        // Setup input actions
        SetupInputActions();
        
        // Set initial state
        if (onlyActiveWhenSettingsOpen)
        {
            SetUIInteractionActive(false);
        }
        else
        {
            SetUIInteractionActive(true);
        }
    }
    
    /// <summary>
    /// Setup input action callbacks
    /// </summary>
    void SetupInputActions()
    {
        if (uiInteractAction.action != null)
        {
            uiInteractAction.action.performed += OnUIInteractPressed;
        }
    }
    
    /// <summary>
    /// Handle UI interaction button press
    /// </summary>
    void OnUIInteractPressed(InputAction.CallbackContext context)
    {
        if (!isUIInteractionActive) return;
        
        // The XR Ray Interactor will handle the actual UI interaction
        // We just need to make sure it's active and properly configured
        Debug.Log("UI Interact button pressed");
    }
    
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Check if UI interaction should be active based on settings panel state
        if (onlyActiveWhenSettingsOpen && settingsManager != null)
        {
            SetUIInteractionActive(settingsManager.IsSettingsPanelOpen());
        }
    }
    
    /// <summary>
    /// Set the UI interaction active state
    /// </summary>
    public void SetUIInteractionActive(bool active)
    {
        isUIInteractionActive = active;
        
        if (rayInteractor != null)
        {
            rayInteractor.enabled = active;
        }
    }
    
    /// <summary>
    /// Toggle the UI interaction active state
    /// </summary>
    public void ToggleUIInteraction()
    {
        SetUIInteractionActive(!isUIInteractionActive);
    }
    
    /// <summary>
    /// Clean up input action subscriptions
    /// </summary>
    void OnDestroy()
    {
        if (uiInteractAction.action != null)
        {
            uiInteractAction.action.performed -= OnUIInteractPressed;
        }
    }
    
    /// <summary>
    /// Enable/disable input actions
    /// </summary>
    void OnEnable()
    {
        if (uiInteractAction.action != null)
        {
            uiInteractAction.action.Enable();
        }
    }
    
    void OnDisable()
    {
        if (uiInteractAction.action != null)
        {
            uiInteractAction.action.Disable();
        }
    }
}
