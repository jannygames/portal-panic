using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AmmoManager : MonoBehaviour
{
	[Header("Ammo Settings")]
	[SerializeField] private int bulletsPerMagazine = 30;
	[SerializeField] private int numberOfMagazines = 3;

	[Header("Reload Settings")]
	[SerializeField] private InputActionProperty reloadAction; // X/Y buttons
	[SerializeField] private AudioClip reloadSound;
	[SerializeField] private float gunAlphaOnEmpty = 0.5f;

	[Header("Gun & HUD")]
	[SerializeField] private Renderer gunRenderer;
	[SerializeField] private HUDController hud;

	[Header("Ammo Pickup Settings")]
	[SerializeField] private GameObject ammoPickupPrefab;
	[SerializeField] private Vector3 pickupSpawnPosition;
	[SerializeField] private float pickupRespawnTime = 30f;

	private int currentBullets;
	private int currentMagazines;
	private GameObject activePickup;
	private AudioSource audioSource;

	void OnEnable()
	{
		// Enable reload action and subscribe
		if (reloadAction.action != null)
		{
			reloadAction.action.Enable();
			reloadAction.action.performed += OnReloadPerformed;
		}
	}

	void OnDisable()
	{
		if (reloadAction.action != null)
		{
			reloadAction.action.performed -= OnReloadPerformed;
			reloadAction.action.Disable();
		}
	}

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

		ResetAmmo();
		UpdateHUD();
		StartCoroutine(PickupSpawner());

		// Optional: log which control path is bound
		if (reloadAction.action != null)
		{
			Debug.Log($"AmmoManager: Reload action enabled. Bindings:");
			foreach (var b in reloadAction.action.bindings)
				Debug.Log($" - {b.path}");
		}
		else
		{
			Debug.LogWarning("AmmoManager: reloadAction is not assigned in Inspector.");
		}
	}

	void Update()
	{
		// Backup polling if you prefer WasPressedThisFrame (works too)
		if (reloadAction.action != null && reloadAction.action.WasPressedThisFrame())
		{
			TryReload();
		}
	}

	private void OnReloadPerformed(InputAction.CallbackContext ctx)
	{
		TryReload();
	}

	private void TryReload()
	{
		// Only reload if not full and has magazines
		if (currentBullets < bulletsPerMagazine)
		{
			Reload();
		}
		else
		{
			Debug.Log("AmmoManager: Magazine already full, reload ignored.");
		}
	}

	#region Ammo Logic
	public bool CanShoot()
	{
		return currentBullets > 0;
	}

	public void ConsumeBullet()
	{
		if (currentBullets <= 0) return;

		currentBullets--;
		UpdateHUD();

		if (currentBullets == 0 && gunRenderer != null)
			SetGunAlpha(gunAlphaOnEmpty);
	}

	private void Reload()
	{
		if (currentMagazines <= 0)
		{
			Debug.Log("AmmoManager: No magazines left.");
			return;
		}

		int bulletsToReload = bulletsPerMagazine - currentBullets;
		currentBullets += bulletsToReload;
		currentMagazines--;

		if (reloadSound != null) audioSource.PlayOneShot(reloadSound);

		UpdateHUD();
		SetGunAlpha(1f);

		Debug.Log($"AmmoManager: Reloaded {bulletsToReload} bullets. Magazines left: {currentMagazines}");
	}

	private void SetGunAlpha(float alpha)
	{
		if (gunRenderer == null) return;
		foreach (Material mat in gunRenderer.materials)
		{
			Color color = mat.color;
			color.a = alpha;
			mat.color = color;
		}
	}

	private void UpdateHUD()
	{
		if (hud != null)
			hud.UpdateAmmoText($"{currentMagazines} | {currentBullets}");
	}

	public void ResetAmmo()
	{
		currentBullets = bulletsPerMagazine;
		currentMagazines = numberOfMagazines;
		SetGunAlpha(1f);
		UpdateHUD();
	}
	#endregion

	#region Pickup Logic
	private IEnumerator PickupSpawner()
	{
		while (true)
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			if (activePickup == null && ammoPickupPrefab != null)
			{
				Vector3 spawnPos = pickupSpawnPosition;
				spawnPos.y = 1f; // set Y to 1 meter above ground
				activePickup = Instantiate(ammoPickupPrefab, spawnPos, Quaternion.identity);
			}
		}
	}

	public void PickupAmmo(int magazinesToAdd)
	{
		currentMagazines += magazinesToAdd;
		UpdateHUD();
	}
	#endregion
}