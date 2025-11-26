using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float range = 50.0f; // Maximum range of the gun
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Layers to exclude from raycast (e.g., UI, Player). Set this to exclude layers that shouldn't block shots.")]
    [SerializeField] private LayerMask excludeLayers;
    [Tooltip("Transform representing the point where the bullet/laser originates from (e.g., gun barrel). If not assigned, uses gun's position.")]
    [SerializeField] private Transform shootPoint;
    [Tooltip("Offset forward from shoot point to avoid starting inside colliders")]
    [SerializeField] private float raycastStartOffset = 0.1f;

    [Header("Input Settings")]
    [Tooltip("Input action for firing the gun (typically trigger button)")]
    [SerializeField] private InputActionProperty fireAction;

    [Header("Effects")]
    [SerializeField] private LineRenderer laserLine; // Visual representation of the ray

    private bool wasPressed = false;

    void Start()
    {
        // Create LineRenderer if not assigned
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
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
        
        // Check layer collision settings - CRITICAL for raycast detection!
        CheckLayerCollisionSettings();
    }
    
    private void CheckLayerCollisionSettings()
    {
        int gunLayer = gameObject.layer;
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int shootPointLayer = shootPoint != null ? shootPoint.gameObject.layer : gunLayer;
        
        string gunLayerName = LayerMask.LayerToName(gunLayer);
        string enemyLayerName = LayerMask.LayerToName(enemyLayerIndex);
        
        Debug.Log($"[Gun] Layer Collision Check:");
        Debug.Log($"[Gun] Gun layer = '{gunLayerName}' ({gunLayer})");
        Debug.Log($"[Gun] ShootPoint layer = '{LayerMask.LayerToName(shootPointLayer)}' ({shootPointLayer})");
        Debug.Log($"[Gun] Enemy layer = '{enemyLayerName}' ({enemyLayerIndex})");
        
        if (enemyLayerIndex == -1)
        {
            Debug.LogError("[Gun] ERROR: 'Enemy' layer does not exist! Raycasts will never hit enemies!");
            return;
        }
        
        // Check if collisions are ignored between gun layer and enemy layer
        bool ignoringGunEnemy = Physics.GetIgnoreLayerCollision(gunLayer, enemyLayerIndex);
        bool ignoringShootPointEnemy = shootPoint != null ? Physics.GetIgnoreLayerCollision(shootPointLayer, enemyLayerIndex) : false;
        
        Debug.Log($"[Gun] Gun layer <-> Enemy layer collisions ignored: {ignoringGunEnemy}");
        if (shootPoint != null)
        {
            Debug.Log($"[Gun] ShootPoint layer <-> Enemy layer collisions ignored: {ignoringShootPointEnemy}");
        }
        
        if (ignoringGunEnemy)
        {
            Debug.LogError($"[Gun] CRITICAL: Collisions between '{gunLayerName}' and '{enemyLayerName}' are IGNORED! Raycasts will not work!");
            Debug.LogError($"[Gun] Fixing: Enabling collisions between gun and enemy layers...");
            Physics.IgnoreLayerCollision(gunLayer, enemyLayerIndex, false);
            Debug.Log($"[Gun] Fixed! Collisions enabled.");
        }
        
        if (shootPoint != null && ignoringShootPointEnemy)
        {
            Debug.LogError($"[Gun] CRITICAL: Collisions between ShootPoint layer and '{enemyLayerName}' are IGNORED! Raycasts will not work!");
            Debug.LogError($"[Gun] Fixing: Enabling collisions between ShootPoint and enemy layers...");
            Physics.IgnoreLayerCollision(shootPointLayer, enemyLayerIndex, false);
            Debug.Log($"[Gun] Fixed! Collisions enabled.");
        }
        
        // Check enemy layer mask value
        Debug.Log($"[Gun] Enemy layer mask value: {enemyLayer.value} (should include layer {enemyLayerIndex})");
        bool layerMaskIncludesEnemy = ((1 << enemyLayerIndex) & enemyLayer.value) != 0;
        Debug.Log($"[Gun] Enemy layer mask includes enemy layer: {layerMaskIncludesEnemy}");
        
        if (!layerMaskIncludesEnemy)
        {
            Debug.LogError($"[Gun] ERROR: Enemy layer mask does not include the Enemy layer! Raycasts will never hit enemies!");
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
        if (laserLine != null)
        {
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
            
            // Make the line more visible
            laserLine.startWidth = 0.005f; // Thicker line for VR visibility
            laserLine.endWidth = 0.005f;
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
            laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserLine.receiveShadows = false;
            laserLine.sortingOrder = 1000; // Render on top
            laserLine.enabled = false;
            
            Debug.Log($"Gun: Laser line renderer setup complete. Using shader: {shader?.name ?? "None"}");
        }
        else
        {
            Debug.LogWarning("Gun: Laser line renderer is null!");
        }
    }

    private void Shoot()
    {
        // Use shootPoint position if assigned
        Vector3 baseOrigin = shootPoint != null ? shootPoint.position : transform.position;
        
        // Use shootPoint's forward direction (gun barrel direction) if available
        // This is the correct direction the gun is pointing in VR
        Vector3 direction;
        if (shootPoint != null)
        {
            direction = shootPoint.forward;
        }
        else if (transform.rotation != Quaternion.identity)
        {
            // Fallback to gun's forward if shootPoint not available but gun is rotated
            direction = transform.forward;
        }
        else if (transform.parent != null)
        {
            // Last resort: use parent's forward (hand controller direction)
            direction = transform.parent.forward;
        }
        else
        {
            // Ultimate fallback: world forward
            direction = Vector3.forward;
        }
        
        // Start raycast slightly forward to avoid starting inside colliders
        Vector3 origin = baseOrigin + direction * raycastStartOffset;
        
        // Also try without offset if we get no hits (in case offset causes issues)
        Vector3 originNoOffset = baseOrigin;
        
        Vector3 endPoint = origin + direction * range; // Default endpoint at max range

        // Create layer mask: everything except excluded layers
        int layerMask = ~excludeLayers.value; // Invert to exclude those layers
        
        // Draw debug line in Scene view (blue color)
        Debug.DrawRay(origin, direction * range, Color.blue, 2.0f);
        Debug.DrawRay(originNoOffset, direction * range, Color.yellow, 2.0f);
        
        // Removed excessive debug logging for performance
        
        // Get all hits - use triggers enabled since enemies appear to have trigger colliders
        // Try with offset first
        RaycastHit[] allHits = Physics.RaycastAll(origin, direction, range, layerMask, QueryTriggerInteraction.Collide);
        
        // If no hits with offset, try without offset
        if (allHits.Length == 0)
        {
            allHits = Physics.RaycastAll(originNoOffset, direction, range, layerMask, QueryTriggerInteraction.Collide);
            if (allHits.Length > 0)
            {
                origin = originNoOffset; // Use the origin that worked
            }
        }
        
        // Only log if we found hits (reduces console spam)
        if (allHits.Length > 0)
        {
            Debug.Log($"Gun: Raycast found {allHits.Length} total hits");
        }
        
        // Sort hits by distance (closest first)
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));
        
        // The first hit is what we actually hit (closest object blocks the shot)
        RaycastHit? closestHit = null;
        bool isEnemyHit = false;
        
        if (allHits.Length > 0)
        {
            closestHit = allHits[0];
            // Check if the closest hit is on the enemy layer
            isEnemyHit = ((1 << closestHit.Value.collider.gameObject.layer) & enemyLayer.value) != 0;
            Debug.Log($"Gun: Closest hit is '{closestHit.Value.collider.gameObject.name}' on layer {LayerMask.LayerToName(closestHit.Value.collider.gameObject.layer)}. Is enemy: {isEnemyHit}");
        }
        
        if (isEnemyHit && closestHit.HasValue)
        {
            RaycastHit enemyHit = closestHit.Value;
            
            // Draw red line to hit point in Scene view (hit!)
            Debug.DrawLine(origin, enemyHit.point, Color.red, 2.0f);
            
            Debug.Log($"Gun: Raycast hit enemy at {enemyHit.point}. Distance: {enemyHit.distance:F2}m");
            Debug.Log($"Gun: Hit object: {enemyHit.collider.gameObject.name} (layer: {LayerMask.LayerToName(enemyHit.collider.gameObject.layer)})");
            
            // Try to find EnemyAbstract component on the hit object, its parent, or children
            EnemyAbstract enemy = enemyHit.collider.GetComponent<EnemyAbstract>();
            
            // If not found, try parent
            if (enemy == null)
            {
                enemy = enemyHit.collider.GetComponentInParent<EnemyAbstract>();
            }
            
            // If still not found, try children
            if (enemy == null)
            {
                enemy = enemyHit.collider.GetComponentInChildren<EnemyAbstract>();
            }
            
            if (enemy != null)
            {
                // Get instance ID to track specific enemy instances
                int instanceID = enemy.GetInstanceID();
                int healthBefore = enemy.health;
                
                Debug.Log($"Gun: About to damage enemy '{enemy.gameObject.name}' (Instance ID: {instanceID}). Current health: {healthBefore}");
                
                enemy.TakeDamageFromGun();
                
                // Check if enemy still exists (might have been destroyed)
                if (enemy != null && enemy.gameObject != null)
                {
                    int healthAfter = enemy.health;
                    Debug.Log($"Gun: ✓ Hit enemy '{enemy.gameObject.name}' (Instance ID: {instanceID})! Dealt 3 damage. Health: {healthBefore} → {healthAfter}");
                }
                else
                {
                    Debug.Log($"Gun: ✓ Hit enemy (Instance ID: {instanceID})! Dealt 3 damage. Enemy destroyed (health was {healthBefore}).");
                }
                
                // Draw green line for successful enemy hit
                Debug.DrawLine(origin, enemyHit.point, Color.green, 2.0f);
            }
            else
            {
                Debug.LogWarning($"Gun: Hit object '{enemyHit.collider.gameObject.name}' but EnemyAbstract component not found on object, parent, or children!");
                Debug.LogWarning($"Gun: Hit object layer: {LayerMask.LayerToName(enemyHit.collider.gameObject.layer)}");
            }
            endPoint = enemyHit.point;
        }
        else if (closestHit.HasValue)
        {
            // Hit something, but it's not an enemy (might be blocked by player collider or wall)
            // Check if there's a close enemy that should still be hit
            EnemyAbstract[] allEnemies = FindObjectsOfType<EnemyAbstract>();
            EnemyAbstract closestEnemy = null;
            float closestEnemyDistance = float.MaxValue;
            float closestEnemyAngle = float.MaxValue;
            
            foreach (EnemyAbstract enemy in allEnemies)
            {
                if (enemy == null || enemy.gameObject == null) continue;
                
                Vector3 toEnemy = enemy.transform.position - origin;
                float distanceToEnemy = toEnemy.magnitude;
                float angleToEnemy = Vector3.Angle(direction, toEnemy.normalized);
                
                // For very close enemies (< 3m), use wide angle tolerance
                float angleTolerance = distanceToEnemy < 3f ? 45f : 10f;
                
                if (distanceToEnemy < 3f && angleToEnemy < angleTolerance && distanceToEnemy < closestEnemyDistance)
                {
                    closestEnemy = enemy;
                    closestEnemyDistance = distanceToEnemy;
                    closestEnemyAngle = angleToEnemy;
                }
            }
            
            // If we found a very close enemy, hit it regardless of what blocked the raycast
            if (closestEnemy != null)
            {
                Debug.Log($"Gun: Close-range override! Enemy at {closestEnemyDistance:F2}m ({closestEnemyAngle:F1}°) blocked by '{closestHit.Value.collider.gameObject.name}' but hitting anyway!");
                
                int instanceID = closestEnemy.GetInstanceID();
                int healthBefore = closestEnemy.health;
                
                closestEnemy.TakeDamageFromGun();
                
                if (closestEnemy != null && closestEnemy.gameObject != null)
                {
                    int healthAfter = closestEnemy.health;
                    Debug.Log($"Gun: ✓ Hit very close enemy '{closestEnemy.gameObject.name}' (Instance ID: {instanceID})! Dealt 3 damage. Health: {healthBefore} → {healthAfter}");
                }
                
                endPoint = closestEnemy.transform.position;
            }
            else
            {
                // Just hit a wall/obstacle, no close enemies
                Debug.Log($"Gun: Raycast hit '{closestHit.Value.collider.gameObject.name}' (layer: {LayerMask.LayerToName(closestHit.Value.collider.gameObject.layer)}) but it's not an enemy. Distance: {closestHit.Value.distance:F2}m");
                endPoint = closestHit.Value.point;
            }
        }
        else
        {
            // Hit nothing - check if we're aiming near any enemies (angle tolerance)
            // This handles cases where small angular differences cause misses at medium/close range
            EnemyAbstract[] allEnemies = FindObjectsOfType<EnemyAbstract>();
            RaycastHit? bestEnemyHit = null;
            float bestAngle = 999f; // Track best (smallest) angle
            EnemyAbstract bestEnemy = null;
            
            if (allEnemies.Length > 0)
            {
                foreach (EnemyAbstract enemy in allEnemies)
                {
                    if (enemy == null || enemy.gameObject == null) continue;
                    
                    Vector3 toEnemy = enemy.transform.position - origin;
                    float distanceToEnemy = toEnemy.magnitude;
                    float angleToEnemy = Vector3.Angle(direction, toEnemy.normalized);
                    
                    // Dynamic angle tolerance: wider for close enemies, tighter for far ones
                    float angleTolerance = distanceToEnemy < 5f ? 15f : (distanceToEnemy < 15f ? 10f : 5f);
                    
                    // Check if enemy is within angle tolerance and range
                    if (angleToEnemy < angleTolerance && distanceToEnemy < range)
                    {
                        // Try multiple origins for close enemies (in case player collider is blocking)
                        Vector3[] testOrigins = distanceToEnemy < 3f 
                            ? new Vector3[] { origin + direction * 0.5f, origin + direction * 1.0f, origin }
                            : new Vector3[] { origin };
                        
                        foreach (Vector3 testOrigin in testOrigins)
                        {
                            // Cast directly at the enemy to see if we can hit it
                            RaycastHit directHit;
                            if (Physics.Raycast(testOrigin, toEnemy.normalized, out directHit, distanceToEnemy + 2f, layerMask, QueryTriggerInteraction.Collide))
                            {
                                // Check if this hit is an enemy
                                bool isEnemy = ((1 << directHit.collider.gameObject.layer) & enemyLayer.value) != 0;
                                if (isEnemy)
                                {
                                // Found an enemy hit - use the one with smallest angle
                                if (angleToEnemy < bestAngle)
                                {
                                    bestAngle = angleToEnemy;
                                    bestEnemyHit = directHit;
                                    bestEnemy = enemy; // Store reference for fallback
                                    Debug.Log($"Gun: Near-miss correction! Enemy at {distanceToEnemy:F2}m was {angleToEnemy:F1}° off. Using origin offset: {(testOrigin - origin).magnitude:F2}m");
                                    break; // Found a hit for this enemy, try next enemy
                                }
                                }
                                else
                                {
                                    // Hit something that's not an enemy - might be blocking
                                    if (distanceToEnemy < 3f)
                                    {
                                        Debug.LogWarning($"Gun: Close enemy at {distanceToEnemy:F2}m blocked by '{directHit.collider.gameObject.name}' on layer '{LayerMask.LayerToName(directHit.collider.gameObject.layer)}' at {directHit.distance:F2}m");
                                    }
                                }
                            }
                        }
                        
                        // If still no hit for very close enemies, check if enemy is directly in front (within wider angle)
                        if (!bestEnemyHit.HasValue && distanceToEnemy < 3f && angleToEnemy < 30f)
                        {
                            // For very close enemies, try hitting via spherecast or just check if they're in a cone
                            // Use a more permissive check - if enemy is within 30 degrees and < 3m, hit them anyway
                            if (angleToEnemy < bestAngle)
                            {
                                bestAngle = angleToEnemy;
                                bestEnemy = enemy;
                                // We'll create a "fake" hit point at the enemy position
                                Debug.LogWarning($"Gun: Very close enemy at {distanceToEnemy:F2}m, {angleToEnemy:F1}° - applying close-range hit!");
                            }
                        }
                    }
                }
            }
            
            // If we found a near-miss enemy, treat it as a hit
            if (bestEnemyHit.HasValue && bestEnemy != null)
            {
                RaycastHit enemyHit = bestEnemyHit.Value;
                
                Debug.DrawLine(origin, enemyHit.point, Color.green, 2.0f);
                
                // Try to find EnemyAbstract component from the hit
                EnemyAbstract enemy = enemyHit.collider.GetComponent<EnemyAbstract>();
                if (enemy == null) enemy = enemyHit.collider.GetComponentInParent<EnemyAbstract>();
                if (enemy == null) enemy = enemyHit.collider.GetComponentInChildren<EnemyAbstract>();
                
                // Fallback to bestEnemy if component not found on hit collider
                if (enemy == null) enemy = bestEnemy;
                
                if (enemy != null)
                {
                    int instanceID = enemy.GetInstanceID();
                    int healthBefore = enemy.health;
                    
                    enemy.TakeDamageFromGun();
                    
                    if (enemy != null && enemy.gameObject != null)
                    {
                        int healthAfter = enemy.health;
                        Debug.Log($"Gun: ✓ Hit enemy '{enemy.gameObject.name}' (Instance ID: {instanceID}) via near-miss correction! Dealt 3 damage. Health: {healthBefore} → {healthAfter}");
                    }
                    
                    endPoint = enemyHit.point;
                }
                else
                {
                    endPoint = origin + direction * range;
                }
            }
            else if (bestEnemy != null && !bestEnemyHit.HasValue)
            {
                // Very close enemy but raycast blocked - apply damage anyway for close-range
                Debug.DrawLine(origin, bestEnemy.transform.position, Color.cyan, 2.0f);
                
                int instanceID = bestEnemy.GetInstanceID();
                int healthBefore = bestEnemy.health;
                
                bestEnemy.TakeDamageFromGun();
                
                if (bestEnemy != null && bestEnemy.gameObject != null)
                {
                    int healthAfter = bestEnemy.health;
                    Debug.Log($"Gun: ✓ Hit very close enemy '{bestEnemy.gameObject.name}' (Instance ID: {instanceID}) via close-range override! Dealt 3 damage. Health: {healthBefore} → {healthAfter}");
                }
                
                endPoint = bestEnemy.transform.position;
            }
            else
            {
                endPoint = origin + direction * range;
            }
        }

        // Visualize the ray
        if (laserLine != null)
        {
            StartCoroutine(ShowLaser(origin, endPoint));
        }
    }

    private IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        if (laserLine == null)
        {
            Debug.LogError("Gun: Cannot show laser - LineRenderer is null!");
            yield break;
        }

        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        laserLine.enabled = true;

        yield return new WaitForSeconds(0.1f);

        laserLine.enabled = false;
    }
}