using UnityEngine;

public class Mine : Effect
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float upwardsModifier = 1f;

    [SerializeField] private ParticleSystem explosionEffect;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Play();
        }
    }

    public override void Play()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        explosionEffect.Play();

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.SetRagdoll();
                    enemy.GetRigidbody().AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier);
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
