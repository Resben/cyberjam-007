using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float speed = 6.5f;

    [Header("Agent")]
    [SerializeField] protected Agent agent;
}
