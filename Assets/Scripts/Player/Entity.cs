using UnityEngine;

public abstract class Entity : MonoBehaviour, ITrigger
{
    [Header("Movement Settings")]
    [SerializeField] protected float walkSpeed = 6.5f;
    [SerializeField] protected float sprintSpeed = 10.0f;

    [Header("Agent")]
    public Agent agent;

    public abstract void Trigger(string type);

    public abstract void Hit();
}
