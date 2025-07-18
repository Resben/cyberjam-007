using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Events;

public class Agent : MonoBehaviour
{
    public UnityEvent OnEndReached;
    [SerializeField] private NavMeshAgent _agent;
    private Target _currentTarget;
    private Queue<Target> _targetQueue;
    private float _lastTime;
    private float _minTime = 0.1f;
    private bool disable = false;
    private Target _priorityTarget;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetQueue = new Queue<Target>(GameObject.FindGameObjectsWithTag("Target")
            .Select(go => go.GetComponent<Target>())
            .Where(t => t != null && !t.IsPlayer() && (t.transform.position.z - transform.position.z) > 0)
            .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
        );

        _currentTarget = _targetQueue.Dequeue();
        _agent.SetDestination(_currentTarget.transform.position);
    }

    public void SetPriorityTarget(Target priority)
    {
        _priorityTarget = priority;
    }

    public void UpdateAgent()
    {
        if (_currentTarget.IsUnlocked() && !_currentTarget.IsEnd())
        {
            _currentTarget = _targetQueue.Dequeue();
        }

        // Update agent pathing every part distance
        if ((Time.time - _lastTime) > _minTime)
        {
            if (TargetPriority())
                _agent.SetDestination(_priorityTarget.transform.position);
            else
            {
                _agent.SetDestination(_currentTarget.transform.position);
            }

            _lastTime = Time.time;
        }

        if (_currentTarget.IsEnd() && DidReachTarget())
        {
            OnEndReached?.Invoke();
            disable = true;
        }
    }

    private bool DidReachTarget()
    {
        return (_currentTarget.transform.position.z - transform.position.z) < 0.5f;
    }

    private bool TargetPriority()
    {
        return _priorityTarget ? _priorityTarget.transform.position.z > _currentTarget.transform.position.z : false;
    }

    public bool CanProceed()
    {
        return _currentTarget.IsUnlocked();
    }

    public Vector3 GetDesiredVelocity()
    {
        return _agent.desiredVelocity;
    }

    public void DisableNavigation()
    {
        _agent.enabled = false;
        disable = true;
    }

    public void EnableNavigation()
    {
        _agent.enabled = true;
        disable = false;
    }

    public void Warp(Vector3 position)
    {
        _agent.Warp(position);
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
    }
}
