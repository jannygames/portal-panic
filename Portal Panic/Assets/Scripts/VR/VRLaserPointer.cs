using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRLaserPointer : MonoBehaviour
{
    [Header("Laser Pointer Settings")]
    [Tooltip("The line renderer for the laser beam")]
    public LineRenderer laserLine;
    
    [Tooltip("Maximum distance for the laser pointer")]
    public float maxDistance = 10f;
    
    [Tooltip("Layer mask for raycast targets")]
    public LayerMask raycastMask = -1;
    
    [Tooltip("Color of the laser when pointing at UI")]
    public Color uiHitColor = Color.green;
    
    [Tooltip("Color of the laser when pointing at nothing")]
    public Color defaultColor = Color.red;
    
    [Tooltip("Width of the laser line")]
    public float lineWidth = 0.02f;
    
    [Header("Input Settings")]
    [Tooltip("Input action for the trigger button")]
    public InputActionProperty triggerAction;
    
    [Tooltip("Input action for the grip button (optional)")]
    public InputActionProperty gripAction;
    
    [Header("Settings Panel Integration")]
    [Tooltip("Reference to the VRSettingsManager")]
    public VRSettingsManager settingsManager;
    
    [Tooltip("Should the laser be active only when settings panel is open?")]
    public bool onlyActiveWhenSettingsOpen = false;
    
    [Header("Audio Feedback")]
    [Tooltip("Audio source for click feedback")]
    public AudioSource audioSource;
    
    [Tooltip("Click sound effect")]
    public AudioClip clickSound;
    
    // Private variables
    private bool isLaserActive = false;
    private GameObject currentTarget;
    private Button currentButton;
    private Toggle currentToggle;
    private Slider currentSlider;
    private Dropdown currentDropdown;
    private bool isPressed = false;
    
    /// <summary>
    /// Initialize the laser pointer
    /// </summary>
    void Start()
    {
        SetupLaserLine();
        SetupInputActions();
        
        // Initially disable laser if it should only be active when settings are open
        if (onlyActiveWhenSettingsOpen)
        {
            SetLaserActive(false);
        }
        else
        {
            SetLaserActive(true);
        }
    }
    
    /// <summary>
    /// Setup the laser line renderer
    /// </summary>
    void SetupLaserLine()
    {
        if (laserLine == null)
        {
            // Create line renderer if not assigned
            laserLine = gameObject.AddComponent<LineRenderer>();
        }
        
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.material.color = defaultColor;
        laserLine.startWidth = lineWidth;
        laserLine.endWidth = lineWidth;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;
        laserLine.enabled = isLaserActive;
    }
    
    /// <summary>
    /// Setup input action callbacks
    /// </summary>
    void SetupInputActions()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.canceled += OnTriggerReleased;
        }
        
        if (gripAction.action != null)
        {
            gripAction.action.performed += OnGripPressed;
        }
    }
    
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Check if laser should be active based on settings panel state
        if (onlyActiveWhenSettingsOpen && settingsManager != null)
        {
            SetLaserActive(settingsManager.IsSettingsPanelOpen());
        }
        
        if (isLaserActive)
        {
            UpdateLaserPointer();
        }
    }
    
    /// <summary>
    /// Update the laser pointer position and interaction
    /// </summary>
    void UpdateLaserPointer()
    {
        // Perform raycast from controller
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, raycastMask);
        
        Vector3 endPoint = transform.position + transform.forward * maxDistance;
        
        if (hitSomething)
        {
            endPoint = hit.point;
            HandleHitTarget(hit.collider.gameObject);
        }
        else
        {
            ClearCurrentTarget();
        }
        
        // Update laser line
        laserLine.SetPosition(0, transform.position);
        laserLine.SetPosition(1, endPoint);
        
        // Handle UI interaction
        if (hitSomething)
        {
            HandleUIInteraction(hit.collider.gameObject);
        }
    }
    
    /// <summary>
    /// Handle hitting a target object
    /// </summary>
    void HandleHitTarget(GameObject target)
    {
        if (currentTarget != target)
        {
            ClearCurrentTarget();
            currentTarget = target;
            
            // Change laser color based on target type
            if (target.GetComponent<Button>() || target.GetComponent<Toggle>() || 
                target.GetComponent<Slider>() || target.GetComponent<Dropdown>())
            {
                laserLine.material.color = uiHitColor;
            }
            else
            {
                laserLine.material.color = defaultColor;
            }
        }
    }
    
    /// <summary>
    /// Clear the current target
    /// </summary>
    void ClearCurrentTarget()
    {
        currentTarget = null;
        currentButton = null;
        currentToggle = null;
        currentSlider = null;
        currentDropdown = null;
        laserLine.material.color = defaultColor;
    }
    
    /// <summary>
    /// Handle UI element interaction
    /// </summary>
    void HandleUIInteraction(GameObject target)
    {
        // Cache UI components for performance
        if (currentButton == null && target.GetComponent<Button>())
        {
            currentButton = target.GetComponent<Button>();
        }
        
        if (currentToggle == null && target.GetComponent<Toggle>())
        {
            currentToggle = target.GetComponent<Toggle>();
        }
        
        if (currentSlider == null && target.GetComponent<Slider>())
        {
            currentSlider = target.GetComponent<Slider>();
        }
        
        if (currentDropdown == null && target.GetComponent<Dropdown>())
        {
            currentDropdown = target.GetComponent<Dropdown>();
        }
    }
    
    /// <summary>
    /// Handle trigger button press
    /// </summary>
    void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!isLaserActive || isPressed) return;
        
        isPressed = true;
        PlayClickSound();
        
        // Handle UI interactions
        if (currentButton != null && currentButton.interactable)
        {
            currentButton.onClick.Invoke();
        }
        else if (currentToggle != null && currentToggle.interactable)
        {
            currentToggle.isOn = !currentToggle.isOn;
        }
        else if (currentSlider != null && currentSlider.interactable)
        {
            // For sliders, we'll need to calculate the position on the slider
            HandleSliderInteraction();
        }
        else if (currentDropdown != null && currentDropdown.interactable)
        {
            currentDropdown.Show();
        }
    }
    
    /// <summary>
    /// Handle trigger button release
    /// </summary>
    void OnTriggerReleased(InputAction.CallbackContext context)
    {
        isPressed = false;
    }
    
    /// <summary>
    /// Handle grip button press (optional secondary interaction)
    /// </summary>
    void OnGripPressed(InputAction.CallbackContext context)
    {
        if (!isLaserActive) return;
        
        // Optional: Add secondary interaction here
        // For example, scroll through dropdown options
        if (currentDropdown != null && currentDropdown.interactable)
        {
            if (currentDropdown.value < currentDropdown.options.Count - 1)
            {
                currentDropdown.value++;
            }
            else
            {
                currentDropdown.value = 0;
            }
        }
    }
    
    /// <summary>
    /// Handle slider interaction based on laser position
    /// </summary>
    void HandleSliderInteraction()
    {
        if (currentSlider == null) return;
        
        // Get the slider's rect transform
        RectTransform sliderRect = currentSlider.GetComponent<RectTransform>();
        if (sliderRect == null) return;
        
        // Convert world position to local position on slider
        Vector3 localPoint = sliderRect.InverseTransformPoint(laserLine.GetPosition(1));
        
        // Calculate normalized position (0-1)
        float normalizedPosition = Mathf.InverseLerp(0, sliderRect.rect.width, localPoint.x);
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
        // Set slider value
        float newValue = Mathf.Lerp(currentSlider.minValue, currentSlider.maxValue, normalizedPosition);
        currentSlider.value = newValue;
    }
    
    /// <summary>
    /// Set the laser active state
    /// </summary>
    public void SetLaserActive(bool active)
    {
        isLaserActive = active;
        if (laserLine != null)
        {
            laserLine.enabled = active;
        }
        
        if (!active)
        {
            ClearCurrentTarget();
        }
    }
    
    /// <summary>
    /// Toggle the laser active state
    /// </summary>
    public void ToggleLaser()
    {
        SetLaserActive(!isLaserActive);
    }
    
    /// <summary>
    /// Play click sound feedback
    /// </summary>
    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    /// <summary>
    /// Clean up input action subscriptions
    /// </summary>
    void OnDestroy()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
            triggerAction.action.canceled -= OnTriggerReleased;
        }
        
        if (gripAction.action != null)
        {
            gripAction.action.performed -= OnGripPressed;
        }
    }
    
    /// <summary>
    /// Enable/disable input actions
    /// </summary>
    void OnEnable()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
        }
        
        if (gripAction.action != null)
        {
            gripAction.action.Enable();
        }
    }
    
    void OnDisable()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.Disable();
        }
        
        if (gripAction.action != null)
        {
            gripAction.action.Disable();
        }
    }
}
