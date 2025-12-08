using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [Tooltip("Bullet prefab to instantiate when shooting. Must have a Bullet component attached.")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Layers to ignore (e.g., UI, Player). Bullets will pass through these.")]
    [SerializeField] private LayerMask ignoreLayers;
    [Tooltip("Transform representing the point where the bullet originates from (e.g., gun barrel). If not assigned, uses gun's position.")]
    [SerializeField] private Transform shootPoint;
    [Tooltip("Offset forward from shoot point to avoid starting inside colliders")]
    [SerializeField] private float spawnOffset = 0.1f;

    [Header("Input Settings")]
    [Tooltip("Input action for firing the gun (typically trigger button)")]
    [SerializeField] private InputActionProperty fireAction;

    [Header("Effects")]
    [Tooltip("Optional: Visual line renderer to show shot direction briefly")]
    [SerializeField] private LineRenderer laserLine;
    [Tooltip("Duration to show the laser line (0 = disabled)")]
    [SerializeField] private float laserDisplayDuration = 0.1f;

    private bool wasPressed = false;

    void Start()
    {
        // Validate bullet prefab
        if (bulletPrefab == null)
        {
            Debug.LogError("Gun: Bullet prefab is not assigned in Inspector! Cannot shoot.");
        }
        else if (bulletPrefab.GetComponent<Bullet>() == null)
        {
            Debug.LogError("Gun: Bullet prefab does not have a Bullet component! Add the Bullet script to the prefab.");
        }

        // Setup optional laser line renderer
        if (laserLine != null && laserDisplayDuration > 0)
        {
            SetupLaserLine();
        }

        // Enable the fire action if assigned
        if (fireAction.action != null)
        {
            fireAction.action.Enable();
            Debug.Log("Gun: Fire action enabled.");
        }
        else
        {
            Debug.LogWarning("Gun: Fire Action is not assigned in Inspector!");
        }
    }

    void Update()
    {
        // Check if fire action is assigned
        if (fireAction.action == null)
        {
            Debug.LogWarning("Gun: Fire Action is not assigned! Cannot shoot.");
            return;
        }

        // Enable the action if it's not enabled
        if (!fireAction.action.enabled)
        {
            fireAction.action.Enable();
        }

        // Check for button press event (works for both VR triggers and mouse buttons)
        bool buttonPressed = fireAction.action.WasPressedThisFrame();
        
        // Also check analog trigger value (for VR controllers)
        float triggerValue = fireAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f && !wasPressed;

        // Shoot if button was pressed this frame OR trigger was just pressed
        if (buttonPressed || triggerPressed)
        {
            Shoot();
        }

        // Update wasPressed state for trigger
        wasPressed = triggerValue > 0.5f;
    }

    private void SetupLaserLine()
    {
        if (laserLine == null) return;

        // Try multiple shader options for better compatibility
        Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material laserMaterial = new Material(shader);
        laserMaterial.color = Color.red;
        laserMaterial.SetColor("_Color", Color.red);
        laserMaterial.SetColor("_TintColor", Color.red);
        laserLine.material = laserMaterial;
        
        laserLine.startWidth = 0.005f;
        laserLine.endWidth = 0.005f;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;
        laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laserLine.receiveShadows = false;
        laserLine.sortingOrder = 1000;
        laserLine.enabled = false;
    }

    private void Shoot()
    {
        // Validate bullet prefab
        if (bulletPrefab == null)
        {
            Debug.LogError("Gun: Cannot shoot - bullet prefab is not assigned!");
            return;
        }

        // Calculate spawn position and direction
        Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;
        Vector3 direction;
        
        if (shootPoint != null)
        {
            direction = shootPoint.forward;
        }
        else if (transform.rotation != Quaternion.identity)
        {
            direction = transform.forward;
        }
        else if (transform.parent != null)
        {
            direction = transform.parent.forward;
        }
        else
        {
            direction = Vector3.forward;
        }
        
        // Offset spawn position slightly forward to avoid starting inside colliders
        spawnPosition += direction * spawnOffset;
        
        // Instantiate bullet
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        // Initialize bullet with direction and settings
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, enemyLayer, ignoreLayers);
        }
        else
        {
            Debug.LogError("Gun: Bullet prefab does not have a Bullet component! Bullet will not work correctly.");
        }
        
        Debug.Log($"Gun: Fired bullet from {spawnPosition} in direction {direction}");

        // Optional: Show laser line for visual feedback
        if (laserLine != null && laserDisplayDuration > 0)
        {
            Vector3 endPoint = spawnPosition + direction * 10f; // Show line for 10 meters
            StartCoroutine(ShowLaser(spawnPosition, endPoint));
        }
    }

    private IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        if (laserLine == null) yield break;

        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        laserLine.enabled = true;

        yield return new WaitForSeconds(laserDisplayDuration);

        laserLine.enabled = false;
    }
}