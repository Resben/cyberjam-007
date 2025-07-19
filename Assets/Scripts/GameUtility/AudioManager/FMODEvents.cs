using FMODUnity;
using UnityEngine;

// Simple class for all events related to FMOD

public class FMODEvents : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] public EventReference menuBGM;

    [Header("Menu SFX")]
    [SerializeField] public EventReference terminalSFX;

    [Header("Game SFX")]
    [SerializeField] public EventReference doorSFX;
    [SerializeField] public EventReference droneSFX;
    [SerializeField] public EventReference explosionSFX;
    [SerializeField] public EventReference mineSFX;


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
