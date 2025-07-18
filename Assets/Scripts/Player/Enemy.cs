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
            agent.SetPriorityTarget(_player.GetTarget());
        else
            Debug.LogError("Couldn't find a player");
        agent.DisableNavigation();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        agent.SetSpeed(speed);
    }

    void Update()
    {
        agent.UpdateAgent();

        if (Input.GetKeyDown(KeyCode.Escape))
            Activate();
    }

    public Rigidbody GetRigidbody()
    {
        return _rb;
    }

    public void SetRagdoll()
    {
        agent.DisableNavigation();
        _rb.isKinematic = false;
        _rb.freezeRotation = false;
        _rb.constraints = RigidbodyConstraints.None;
    }

    public void Activate()
    {
        agent.EnableNavigation();
    }
}
