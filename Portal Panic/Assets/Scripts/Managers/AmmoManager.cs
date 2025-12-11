using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AmmoManager : MonoBehaviour
{
	[Header("Ammo Settings")]
	[SerializeField] private int bulletsPerMagazine = 30;
	[SerializeField] private int numberOfMagazines = 3;

	[Header("Reload Settings")]
	[SerializeField] private AudioClip reloadSound;
	[SerializeField] private float gunAlphaOnEmpty = 0.5f;

	[Header("HUD")]
	[SerializeField] private HUDController hud;
	[SerializeField] private Renderer gunRenderer;

	[Header("Pickup Settings")]
	[SerializeField] private GameObject ammoPickupPrefab;
	[SerializeField] private Transform playerSpawnPoint;
	[SerializeField] private float pickupRespawnTime = 15f;

	[Header("Input System")]
	[SerializeField] private InputActionProperty reloadAction;

	private int currentBullets;
	private int currentMagazines;
	private GameObject activePickup;

	private AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

		initialPickupPosition = playerSpawnPoint.position + Vector3.up * 1f;
		StartCoroutine(PickupSpawner());

		ResetAmmo();
		UpdateHUD();
		StartCoroutine(PickupSpawner());
	}

	void Update()
	{
		HandleReloadInput();
	}

	public bool CanShoot()
	{
		return currentBullets > 0;
	}

	public void ConsumeBullet()
	{
		if (currentBullets > 0)
		{
			currentBullets--;
			UpdateHUD();
			if (currentBullets == 0 && gunRenderer != null)
			{
				SetGunAlpha(gunAlphaOnEmpty);
			}
		}
	}

	private void HandleReloadInput()
	{
		if (reloadAction.action != null && reloadAction.action.WasPressedThisFrame() && currentBullets < bulletsPerMagazine)
		{
			Reload();
		}
	}


	private void Reload()
	{
		if (currentMagazines <= 0) return;

		int bulletsToReload = bulletsPerMagazine - currentBullets;
		currentBullets += bulletsToReload;
		currentMagazines--;

		if (reloadSound != null)
		{
			audioSource.PlayOneShot(reloadSound);
		}

		UpdateHUD();
		SetGunAlpha(1f); // Reset gun visibility
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
		{
			hud.UpdateAmmoText($"{currentMagazines} | {currentBullets}");
		}
	}

	public void ResetAmmo()
	{
		currentBullets = bulletsPerMagazine;
		currentMagazines = numberOfMagazines;
		SetGunAlpha(1f);
		UpdateHUD();
	}

	#region Pickup
	private Vector3 initialPickupPosition;
	

	private IEnumerator PickupSpawner()
	{
		while (true)
		{
			yield return new WaitForSeconds(pickupRespawnTime);

			if (activePickup == null)
			{
				activePickup = Instantiate(ammoPickupPrefab, initialPickupPosition, Quaternion.identity);
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