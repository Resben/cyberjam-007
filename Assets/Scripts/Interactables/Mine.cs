using UnityEngine;

public class Mine : Effect
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float upwardsModifier = 1f;

    [SerializeField] private ParticleSystem explosionEffect;

    public override void Play()
    {
        explosionEffect.Play();
        
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("Hit a drone");
                DroneEnemy enemy = hit.GetComponent<DroneEnemy>();
                if (enemy != null)
                {
                    Debug.Log("Drone mine hit");
                    enemy.MineHit();
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
