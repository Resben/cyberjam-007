using UnityEngine;
using FMODUnity;

public class Mine : Effect
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float upwardsModifier = 1f;

    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private EventReference _mineNoise;

    [SerializeField] private bool isStartBomb = false;

    protected override void Start()
    {
        base.Start();

        var gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        var volume = gameManager.GetVolume();
        var instance = RuntimeManager.CreateInstance(_mineNoise);
        instance.setVolume(volume);
        RuntimeManager.AttachInstanceToGameObject(instance, gameObject, GetComponent<Rigidbody>());
        instance.start();
    }

    public override void Play()
    {
        explosionEffect.Play();

        if (isStartBomb)
        {
            GameManager.Instance.currentLevelManager.BlowDoorUp();
        }

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
