using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
	[Header("Gun Settings")]
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private float bulletSpeed = 50f;
	[SerializeField] private LayerMask enemyLayer;
	[SerializeField] private LayerMask ignoreLayers;
	[SerializeField] private Transform shootPoint;
	[SerializeField] private float spawnOffset = 0.1f;

	[Header("Input Settings")]
	[SerializeField] private InputActionProperty fireAction;

	[Header("Effects")]
	[SerializeField] private LineRenderer laserLine;
	[SerializeField] private float laserDisplayDuration = 0.1f;
	[SerializeField] private AudioClip shootSound;
	[SerializeField][Range(0, 1)] private float shootSoundVolume = 1f;

	[SerializeField] private PlayerHealth playerHealth;
	[SerializeField] private AmmoManager ammoManager;

	[SerializeField] private AudioClip emptyClickSound;
	[SerializeField][Range(0, 1)] private float emptyClickVolume = 1f;

	private bool wasPressed = false;
	private AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null && shootSound != null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}

		if (bulletPrefab == null)
		{
			Debug.LogError("Gun: Bullet prefab not assigned!");
		}

		if (laserLine != null && laserDisplayDuration > 0)
		{
			SetupLaserLine();
		}

		if (fireAction.action != null)
		{
			fireAction.action.Enable();
		}
	}

	void Update()
	{
		if (fireAction.action == null) return;

		if (!fireAction.action.enabled) fireAction.action.Enable();

		bool buttonPressed = fireAction.action.WasPressedThisFrame();
		float triggerValue = fireAction.action.ReadValue<float>();
		bool triggerPressed = triggerValue > 0.5f && !wasPressed;

		if (buttonPressed || triggerPressed)
		{
			Shoot();
		}

		wasPressed = triggerValue > 0.5f;
	}

	private void SetupLaserLine()
	{
		if (laserLine == null) return;
		Shader shader = Shader.Find("Unlit/Color");
		Material laserMaterial = new Material(shader);
		laserMaterial.color = Color.red;
		laserLine.material = laserMaterial;
		laserLine.startWidth = 0.005f;
		laserLine.endWidth = 0.005f;
		laserLine.positionCount = 2;
		laserLine.enabled = false;
	}

	private void Shoot()
	{
		if (playerHealth != null && playerHealth.IsDead()) return;
		if (ammoManager != null && !ammoManager.CanShoot())
		{
			// No bullets left in current mag
			if (emptyClickSound != null)
			{
				AudioSource.PlayClipAtPoint(emptyClickSound, transform.position, emptyClickVolume);
			}
			Debug.Log("Gun: Tried to shoot with empty magazine.");
			return;
		}

		ammoManager.ConsumeBullet();

		Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;
		Vector3 direction = shootPoint != null ? shootPoint.forward : transform.forward;
		spawnPosition += direction * spawnOffset;

		GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(direction));
		Bullet bullet = bulletObj.GetComponent<Bullet>();
		if (bullet != null)
			bullet.Initialize(direction, bulletSpeed, enemyLayer, ignoreLayers);

		if (shootSound != null && audioSource != null)
			audioSource.PlayOneShot(shootSound, shootSoundVolume);

		if (laserLine != null && laserDisplayDuration > 0)
		{
			Vector3 endPoint = spawnPosition + direction * 10f;
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