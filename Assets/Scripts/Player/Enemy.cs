using UnityEngine;

public class Enemy : Entity
{
    private Rigidbody _rb;

    void Start()
    {
        _rb.isKinematic = true;
    }

    void Update()
    {
        agent.UpdateAgent();
    }

    public void BlowUp(float force, Vector3 positon, float radius, float upwardsModifier)
    {
        agent.enabled = false;
        _rb.isKinematic = false;
        _rb.AddExplosionForce(force, positon, radius, upwardsModifier);

    }

}
