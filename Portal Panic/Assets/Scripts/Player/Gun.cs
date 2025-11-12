using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float range = 50.0f; // Maximum range of the gun
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private LineRenderer laserLine; // Visual representation of the ray

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Left mouse button or controller trigger
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // Perform the raycast
        if (Physics.Raycast(origin, direction, out hit, range, enemyLayer))
        {
            EnemyAbstract enemy = hit.collider.GetComponent<EnemyAbstract>();
            if (enemy != null)
            {
                enemy.TakeDamageFromGun();
            }
        }

        // Visualize the ray
        if (laserLine != null)
        {
            StartCoroutine(ShowLaser(origin, hit.point));
        }
    }

    private IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        laserLine.enabled = true;

        yield return new WaitForSeconds(0.1f);

        laserLine.enabled = false;
    }
}