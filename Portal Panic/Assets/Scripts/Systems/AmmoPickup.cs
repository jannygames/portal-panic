using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
	[SerializeField] private int magazinesAmount = 1;
	[SerializeField] private AudioClip pickupSound;
	[SerializeField] private float spinSpeed = 360f; // degrees per second

	void Update()
	{
		transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
	}

	private void OnTriggerEnter(Collider other)
	{
		AmmoManager ammo = other.GetComponent<AmmoManager>();
		if (ammo == null) ammo = other.GetComponentInChildren<AmmoManager>();
		if (ammo != null)
		{
			ammo.PickupAmmo(magazinesAmount);
			if (pickupSound != null)
				AudioSource.PlayClipAtPoint(pickupSound, transform.position);
			Destroy(gameObject);
		}
	}
}