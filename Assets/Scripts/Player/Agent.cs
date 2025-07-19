using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Events;

public enum AgentState
{
    Idle,
    Tracking,
    Disabled
}

public class Agent : MonoBehaviour
{
    public UnityEvent OnHit;
    public UnityEvent OnPriorityReached;
    public UnityEvent OnEndReached;
    [SerializeField] private NavMeshAgent _agent;
    private Target _currentTarget;
    private Queue<Target> _targetQueue;
    private float _lastTime;
    private float _minTime = 0.1f;
    private Target _priorityTarget;
    private float _distanceToPriority;

    public AgentState state = AgentState.Idle;
    private AgentState _lastState = AgentState.Idle;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void SetPriorityTarget(Target priority, float distanceRequired)
    {
        _priorityTarget = priority;
        _distanceToPriority = distanceRequired;
    }

    public void UpdateAgent()
    {
        if (state == AgentState.Tracking && _lastState == AgentState.Idle)
        {
            _targetQueue?.Clear();

            _targetQueue = new Queue<Target>(GameObject.FindGameObjectsWithTag("Target")
                .Select(go => go.GetComponent<Target>())
                .Where(t => t != null && !t.IsPlayer() && (t.transform.position.z - transform.position.z) > 0)
                .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
            );

            _currentTarget = _targetQueue.Dequeue();
            _agent.SetDestination(_currentTarget.transform.position);
        }

        _lastState = state;

        if (state == AgentState.Idle)
            return;

        if (_currentTarget.IsUnlocked() && !_currentTarget.IsEnd())
        {
            _currentTarget = _targetQueue.Dequeue();
        }

        // Update agent pathing every part distance
        if ((Time.time - _lastTime) > _minTime)
        {
            if (TargetPriority())
            {
                _agent.SetDestination(GetPriorityPosition());

                if (DidReachPriorityTarget())
                {
                    OnPriorityReached?.Invoke();
                }
            }
            else
            {
                _agent.SetDestination(_currentTarget.transform.position);

                if (_currentTarget.IsEnd() && DidReachTarget())
                {
                    OnEndReached?.Invoke();
                    state = AgentState.Disabled;
                }
            }

            _lastTime = Time.time;
        }
    }

    private bool DidReachTarget()
    {
        return (_currentTarget.transform.position.z - transform.position.z) < 0.5f;
    }

    private bool DidReachPriorityTarget()
    {
        return _priorityTarget ? (_priorityTarget.transform.position.z - _priorityTarget.transform.position.z) < _distanceToPriority : false;
    }

    private bool TargetPriority()
    {
        // return _priorityTarget ? _priorityTarget.transform.position.z > _currentTarget.transform.position.z : false;
        return _priorityTarget != null;
    }

    private Vector3 GetPriorityPosition()
    {
        Vector3 pos = _priorityTarget.transform.position;
        pos.z -= 2.5f; // Little offset off the character
        return pos;
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
        state = AgentState.Disabled;
    }

    public void EnableNavigation()
    {
        _agent.enabled = true;
        state = AgentState.Disabled;
    }

    public void Warp(Vector3 position)
    {
        _agent.Warp(position);
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
    }

    public void Hit()
    {
        OnHit?.Invoke();
    }
}
