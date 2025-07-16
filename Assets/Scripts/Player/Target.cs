using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private bool isEnd = false;

    [Header("Debug")]
    [SerializeField] private bool _isUnlocked;

    public void SetUnlock(bool isUnlocked)
    {
        _isUnlocked = isUnlocked;
    }

    public bool IsUnlocked()
    {
        return _isUnlocked;
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }

    public bool IsEnd()
    {
        return isEnd;
    }
}
