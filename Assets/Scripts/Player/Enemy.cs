using System.Linq;
using UnityEngine;

public class Enemy : Entity
{
    
    private Player _player;
    private Rigidbody _rb;

    void Start()
    {
        var playerObject = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _player = playerObject != null ? playerObject.GetComponent<Player>() : null;
        if (_player)
            agent.SetPriorityTarget(_player.GetTarget(), 5.0f);
        else
            Debug.LogError("Couldn't find a player");
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        agent.SetSpeed(sprintSpeed);
        agent.state = AgentState.Idle;
    }

    void Update()
    {
        agent.UpdateAgent();
    }

    public Rigidbody GetRigidbody()
    {
        return _rb;
    }

    public void SetRagdoll()
    {
        agent.state = AgentState.Idle;
        _rb.isKinematic = false;
        _rb.freezeRotation = false;
        _rb.constraints = RigidbodyConstraints.None;
    }

    public override void Trigger(string type)
    {
        switch (type)
        {
            case "spawn":
                agent.state = AgentState.Tracking;
                break;
            default:
                break;
        }
    }
}
