using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Events;

public class Agent : MonoBehaviour
{
    public UnityEvent OnEndReached;
    private NavMeshAgent _agent;
    private Target _currentTarget;
    private Queue<Target> _targetQueue;
    private float _lastTime;
    private float _minTime = 0.1f;
    private bool disable = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetQueue = new Queue<Target>(GameObject.FindGameObjectsWithTag("Target")
            .Select(go => go.GetComponent<Target>())
            .Where(t => t != null && !t.IsPlayer() && (t.transform.position.z - transform.position.z) > 0)
            .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
        );

        _currentTarget = _targetQueue.Dequeue();
    }

    public void UpdateAgent()
    {
        if (disable)
            return;

        if (_currentTarget.IsUnlocked() && !_currentTarget.IsEnd())
        {
            _currentTarget = _targetQueue.Dequeue();
        }

        if (_currentTarget.IsEnd() && _agent.remainingDistance < 0.5f)
        {
            OnEndReached?.Invoke();
            disable = true;
        }

        // Update agent pathing every part distance
        if ((Time.time - _lastTime) > _minTime)
        {
            _agent.SetDestination(_currentTarget.transform.position);
            _lastTime = Time.time;
        }
    }

    public bool CanProceed()
    {
        return _currentTarget.IsUnlocked();
    }

    public Vector3 GetDesiredVelocity()
    {
        return _agent.desiredVelocity;
    }

    public void Warp(Vector3 position)
    {
        _agent.Warp(position);
    }
}
