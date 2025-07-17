using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float walkSpeed = 6.5f;
    [SerializeField] protected float sprintSpeed = 9.5f;

    [Header("Agent")]
    [SerializeField] protected Agent agent;
}
