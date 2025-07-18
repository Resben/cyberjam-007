using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Note : MonoBehaviour
{
    // References to the outer ring and its MeshRenderer
    [SerializeField] private Transform _outerRing;
    [SerializeField] private MeshRenderer _outerRingMeshRenderer;

    // Camera reference
    private Transform _cameraTransform;

    [SerializeField] private AudioClip _successSound;
    [SerializeField] private AudioClip _failSound;
    private AudioSource _audioSource;

    private NoteManager _noteManager;

    private float _startTime;

    // Properties for the note
    private float _duration;
    private float _leadWindowTime;
    private float _acceptanceWindow;

    // Properties for Note Animation
    [SerializeField] private float _outerRingStartingScale;    
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeStrength;

    private bool _isHandled;

    void Awake()
    {
        if (!_outerRing || !_outerRingMeshRenderer)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            Destroy(gameObject);
            return;
        }

        _outerRingStartingScale = 5.0f; // Initial scale of the outer ring
        _shakeDuration = 0.2f;
        _shakeStrength = 0.2f;
        _isHandled = false;
    }

    void Start()
    {
        if (!_cameraTransform)
        {
            _cameraTransform = Camera.main.transform;
        }

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource || !_successSound || !_failSound)
        {
            Debug.LogError("Audio not linked");
        }

        if (!_noteManager)
        {
            Debug.LogError("Cannot find Note Manager");
            Destroy(gameObject);
            return;
        }

        _startTime = Time.time;

        // animate alpha channels to opaque over note's lifetime
        _outerRingMeshRenderer.material.DOFade(1.0f, _leadWindowTime + _duration * 0.25f)
            .SetEase(Ease.InOutCubic);

        _outerRing.localScale = Vector3.one * _outerRingStartingScale;
        _outerRing.DOScale(Vector3.one, _leadWindowTime + _duration + 0.25f * _acceptanceWindow)
            .SetEase(Ease.InCubic);

        StartCoroutine(LifespanCoroutine());
    }

    void OnDestroy()
    {
        StopAnimations();
        _noteManager.RemoveLocationSpawn(new Vector2(transform.position.x, transform.position.y));
    }

    public void InitializeNote(NoteManager noteManager, float duration, float leadWindowTime, float acceptanceWindow, Transform cameraTransform)
    {
        _duration = duration == -1.0f ? 0.0f : duration;
        _leadWindowTime = leadWindowTime;
        _acceptanceWindow = acceptanceWindow;
        _cameraTransform = cameraTransform;
        _noteManager = noteManager;
    }

    void Update()
    {
        ClickHandler();
    }

    private void ClickHandler()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    OnNoteClicked();
                }
            }
        }
    }

    private IEnumerator LifespanCoroutine()
    {
        yield return new WaitForSecondsRealtime(_leadWindowTime + _duration + _acceptanceWindow * 0.5f);

        OnNoteFail();
    }

    private void OnNoteClicked()
    {
        StopAnimations();
        StopAllCoroutines();

        if (IsNoteClickCorrect())
        {
            OnNoteSuccess();
        }
        else
        {
            OnNoteFail();
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

    private void OnNoteFail()
    {
        if (_isHandled)
        {
            return;
        }

        _isHandled = true;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        _noteManager.AddFailPoint();

        _audioSource.volume = 0.2f;
        _audioSource.PlayOneShot(_failSound);

        transform.DOShakePosition(_shakeDuration, _shakeStrength)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private void OnNoteSuccess()
    {
        if (_isHandled)
        {
            return;
        }

        _isHandled = true;
        _noteManager.AddSuccessPoint();

        _audioSource.volume = 1.0f;
        _audioSource.PlayOneShot(_successSound);

        // Punch towards the camera
        Vector3 direction = (_cameraTransform.position - transform.position).normalized;
        transform.DOPunchPosition(direction, _shakeDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private void StopAnimations()
    {
        transform.DOKill();
        if (_outerRing)
        {
            _outerRing.DOKill();
        }
    }


}
