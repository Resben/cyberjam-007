using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Note : MonoBehaviour
{
    private float _startTime;

    // Properties for the note
    private float _duration;
    private float _leadWindowTime;
    private float _acceptanceWindow;
    private float _outerRingStartingScale = 5.0f; // Initial scale of the outer ring

    // Properties for Note Animation
    private float _shakeDuration { get; set; } = 0.2f;
    private float _shakeStrength { get; set; } = 0.2f;

    // References to the outer ring and its MeshRenderer
    [SerializeField] private Transform _outerRing;
    [SerializeField] private MeshRenderer _outerRingMeshRenderer;

    // Camera reference
    private Transform _cameraTransform;

    [SerializeField] private AudioClip _successSound;
    [SerializeField] private AudioClip _failSound;
    private AudioSource _audioSource;

    void Awake()
    {
        if (!_outerRing || !_outerRingMeshRenderer)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            return;
        }
    }

    void Start()
    {
        if (!_cameraTransform)
        {
            _cameraTransform = Camera.main.transform;
        }

        _startTime = Time.time;

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource || !_successSound || !_failSound)
        {
            Debug.LogError("Audio not linked");
        }

        // animate alpha channels to opaque over note's lifetime
        _outerRingMeshRenderer.material.DOFade(1.0f, _leadWindowTime + _duration * 0.25f)
            .SetEase(Ease.InOutCubic);

        _outerRing.localScale = Vector3.one * _outerRingStartingScale;
        _outerRing.DOScale(Vector3.one, _leadWindowTime + _duration + _acceptanceWindow * 0.5f)
            .SetEase(Ease.OutCirc);

        StartCoroutine(LifespanCoroutine());
    }

    void OnDestroy()
    {
        StopAnimations();
    }

    public void InitializeNote(float duration, float leadWindowTime, float acceptanceWindow, Transform cameraTransform)
    {
        _duration = duration == -1.0f ? 0.0f : duration;
        _leadWindowTime = leadWindowTime;
        _acceptanceWindow = acceptanceWindow;
        _cameraTransform = cameraTransform;
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
        yield return new WaitForSeconds(_leadWindowTime + _duration + _acceptanceWindow * 0.5f);

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
        // @TODO: Subtract from the progress meter or something

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
        // @TODO: Add to progress meter or something

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
