using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ParticleNote : MonoBehaviour
{
    [SerializeField] private AudioClip _successSound;
    [SerializeField] private AudioClip _failSound;
    [SerializeField] private ParticleSystem _particleSystem;
    private AudioSource _audioSource;

    private NoteManager _noteManager;

    private float _startTime;

    // Properties for the note
    private float _duration = 0;
    private float _leadWindowTime = 2f;
    private float _acceptanceWindow = 0.4f;

    // Properties for Note Animation
    [SerializeField] private float _handleDuration = 0.2f;

    private bool _isHandled = false;

    void Awake()
    {
        if (!_particleSystem)
        {
            Debug.LogError("Cannot find particle System");
            Destroy(gameObject);
            return;
        }
        var main = _particleSystem.main;
        main.duration = _leadWindowTime + _acceptanceWindow;
        main.startLifetime = _leadWindowTime;
        
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource || !_successSound || !_failSound)
        {
            Debug.LogError("Audio not linked");
        }

        if (!_noteManager)
        {
            Debug.LogError("Note Manager not linked");
        }

        _startTime = Time.time;

        _particleSystem.Play();
        StartCoroutine(LifespanCoroutine());
    }

    void OnDestroy()
    {
        _noteManager.RemoveLocationSpawn(new Vector2(transform.position.x, transform.position.y));
    }

    public void InitializeNote(NoteManager noteManager, float duration, float leadWindowTime, float acceptanceWindow)
    {
        _duration = duration == -1.0f ? 0.0f : duration;
        _leadWindowTime = leadWindowTime;
        _acceptanceWindow = acceptanceWindow;
        _noteManager = noteManager;
    }

    private IEnumerator LifespanCoroutine()
    {
        yield return new WaitForSecondsRealtime(_leadWindowTime + _duration + _acceptanceWindow * 0.5f);

        StartCoroutine(OnNoteFailCoroutine());
    }

    public void OnNoteClicked()
    {
        if (_isHandled) return;

        StopAllCoroutines();

        if (IsNoteClickCorrect())
        {
            StartCoroutine(OnNoteSuccessCoroutine());
        }
        else
        {
            StartCoroutine(OnNoteFailCoroutine());
        }
    }

    private bool IsNoteClickCorrect()
    {
        float correctTimeWindowStart = _startTime + _leadWindowTime - _acceptanceWindow;
        float correctTimeWindowEnd = _startTime + _leadWindowTime + _duration + _acceptanceWindow;
        float currentTime = Time.time;

        if (currentTime > correctTimeWindowStart && currentTime < correctTimeWindowEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator OnNoteFailCoroutine()
    {
        _isHandled = true;
        _noteManager.AddFailPoint();

        _audioSource.volume = 0.2f;
        _audioSource.PlayOneShot(_failSound);

        yield return new WaitForSecondsRealtime(_handleDuration);
        Destroy(gameObject);
    }

    private IEnumerator OnNoteSuccessCoroutine()
    {
        _isHandled = true;
        _noteManager.AddSuccessPoint();

        _audioSource.volume = 1.0f;
        _audioSource.PlayOneShot(_successSound);

        yield return new WaitForSecondsRealtime(_handleDuration);
        Destroy(gameObject);
    }
}
