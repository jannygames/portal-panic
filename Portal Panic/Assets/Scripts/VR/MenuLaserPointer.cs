using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuLaserPointer : MonoBehaviour
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
    
    [Tooltip("Input action for the A/X button (optional)")]
    public InputActionProperty primaryButtonAction;
    
    [Header("Audio Feedback")]
    [Tooltip("Audio source for click feedback")]
    public AudioSource audioSource;
    
    [Tooltip("Click sound effect")]
    public AudioClip clickSound;
    
    [Tooltip("Hover sound effect")]
    public AudioClip hoverSound;
    
    [Header("Visual Feedback")]
    [Tooltip("Cursor object to show at laser end point")]
    public GameObject cursorObject;
    
    [Tooltip("Scale of cursor when hovering over UI")]
    public float cursorHoverScale = 1.2f;
    
    [Tooltip("Scale of cursor when not hovering")]
    public float cursorNormalScale = 1.0f;
    
    // Private variables
    private bool isLaserActive = true;
    private GameObject currentTarget;
    private Button currentButton;
    private Toggle currentToggle;
    private Slider currentSlider;
    private Dropdown currentDropdown;
    private bool isPressed = false;
    private bool hasPlayedHoverSound = false;
    private Vector3 lastCursorPosition;
    
    /// <summary>
    /// Initialize the menu laser pointer
    /// </summary>
    void Start()
    {
        SetupLaserLine();
        SetupInputActions();
        SetupCursor();
        
        // Always active in menu scenes
        SetLaserActive(true);
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
    /// Setup the cursor object
    /// </summary>
    void SetupCursor()
    {
        if (cursorObject == null)
        {
            // Create a simple cursor if not assigned
            cursorObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursorObject.name = "LaserCursor";
            cursorObject.transform.localScale = Vector3.one * 0.02f;
            
            // Remove collider from cursor
            Collider cursorCollider = cursorObject.GetComponent<Collider>();
            if (cursorCollider != null)
            {
                Destroy(cursorCollider);
            }
            
            // Set cursor material
            Renderer cursorRenderer = cursorObject.GetComponent<Renderer>();
            if (cursorRenderer != null)
            {
                cursorRenderer.material = new Material(Shader.Find("Sprites/Default"));
                cursorRenderer.material.color = Color.white;
            }
        }
        
        cursorObject.SetActive(isLaserActive);
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
        
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.performed += OnPrimaryButtonPressed;
        }
    }
    
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (isLaserActive)
        {
            UpdateLaserPointer();
            UpdateCursor();
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
        
        // Store cursor position for cursor object
        lastCursorPosition = endPoint;
        
        // Handle UI interaction
        if (hitSomething)
        {
            HandleUIInteraction(hit.collider.gameObject);
        }
    }
    
    /// <summary>
    /// Update the cursor object position and scale
    /// </summary>
    void UpdateCursor()
    {
        if (cursorObject == null) return;
        
        cursorObject.transform.position = lastCursorPosition;
        
        // Scale cursor based on whether we're hovering over UI
        bool hoveringUI = currentTarget != null && 
                         (currentButton != null || currentToggle != null || 
                          currentSlider != null || currentDropdown != null);
        
        float targetScale = hoveringUI ? cursorHoverScale : cursorNormalScale;
        cursorObject.transform.localScale = Vector3.one * 0.02f * targetScale;
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
            hasPlayedHoverSound = false;
            
            // Change laser color based on target type
            if (target.GetComponent<Button>() || target.GetComponent<Toggle>() || 
                target.GetComponent<Slider>() || target.GetComponent<Dropdown>())
            {
                laserLine.material.color = uiHitColor;
                PlayHoverSound();
            }
            else
            {
                laserLine.material.color = defaultColor;
            }
        }
        else if (!hasPlayedHoverSound && 
                (target.GetComponent<Button>() || target.GetComponent<Toggle>() || 
                 target.GetComponent<Slider>() || target.GetComponent<Dropdown>()))
        {
            PlayHoverSound();
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
        hasPlayedHoverSound = false;
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
    /// Handle grip button press (alternative interaction)
    /// </summary>
    void OnGripPressed(InputAction.CallbackContext context)
    {
        if (!isLaserActive) return;
        
        // Alternative interaction - same as trigger
        OnTriggerPressed(context);
    }
    
    /// <summary>
    /// Handle primary button press (A/X button)
    /// </summary>
    void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        if (!isLaserActive) return;
        
        // Primary button interaction - same as trigger
        OnTriggerPressed(context);
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
        Vector3 localPoint = sliderRect.InverseTransformPoint(lastCursorPosition);
        
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
        
        if (cursorObject != null)
        {
            cursorObject.SetActive(active);
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
    /// Play hover sound feedback
    /// </summary>
    void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null && !hasPlayedHoverSound)
        {
            audioSource.PlayOneShot(hoverSound);
            hasPlayedHoverSound = true;
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
        
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.performed -= OnPrimaryButtonPressed;
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
        
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.Enable();
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
        
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.Disable();
        }
    }
}
