using UnityEngine;

public class DroneEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void MineHit()
    {
        GetComponent<DroneDeath>().Play();
        agent.state = AgentState.Idle;
        _rb.isKinematic = false;
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
