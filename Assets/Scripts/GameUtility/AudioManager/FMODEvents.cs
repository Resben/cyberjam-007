using FMODUnity;
using UnityEngine;

// Simple class for all events related to FMOD

public class FMODEvents : MonoBehaviour
{
    [Header("Music")]

    [Header("Menu SFX")]

    [Header("General SFX")]

    [Header("Player SFX")]

    private static FMODEvents _instance;

    public static FMODEvents Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
