using UnityEngine;
using DG.Tweening;
using System.Collections;

[System.Serializable]
public struct NoteProperties
{
    public float startTime;

    // Properties for the note
    public float duration;
    public float leadWindowTime;
    public float acceptanceWindow;
    public float outerRingStartingScale;

    // Properties for Note Animation
    public float shakeDuration;
    public float shakeStrength;

    public bool isHandled;
}

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

    private NoteProperties _noteProperties;

    void Awake()
    {
        if (!_outerRing || !_outerRingMeshRenderer)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            Destroy(gameObject);
            return;
        }

        _noteProperties.outerRingStartingScale = 5.0f; // Initial scale of the outer ring
        _noteProperties.shakeDuration = 0.2f;
        _noteProperties.shakeStrength = 0.2f;
        _noteProperties.isHandled = false;
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

        _noteProperties.startTime = Time.time;

        // animate alpha channels to opaque over note's lifetime
        _outerRingMeshRenderer.material.DOFade(1.0f, _noteProperties.leadWindowTime + _noteProperties.duration * 0.25f)
            .SetEase(Ease.InOutCubic);

        _outerRing.localScale = Vector3.one * _noteProperties.outerRingStartingScale;
        _outerRing.DOScale(Vector3.one, _noteProperties.leadWindowTime + _noteProperties.duration + 0.25f * _noteProperties.acceptanceWindow)
            .SetEase(Ease.InCubic);

        StartCoroutine(LifespanCoroutine());
    }

    void OnDestroy()
    {
        StopAnimations();
    }

    public void InitializeNote(NoteManager noteManager, float duration, float leadWindowTime, float acceptanceWindow, Transform cameraTransform)
    {
        _noteProperties.duration = duration == -1.0f ? 0.0f : duration;
        _noteProperties.leadWindowTime = leadWindowTime;
        _noteProperties.acceptanceWindow = acceptanceWindow;
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
        yield return new WaitForSecondsRealtime(_noteProperties.leadWindowTime + _noteProperties.duration + _noteProperties.acceptanceWindow * 0.5f);

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
        float correctTimeWindowStart = _noteProperties.startTime + _noteProperties.leadWindowTime - _noteProperties.acceptanceWindow;
        float correctTimeWindowEnd = _noteProperties.startTime + _noteProperties.leadWindowTime + _noteProperties.duration + _noteProperties.acceptanceWindow;
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
        if (_noteProperties.isHandled)
        {
            return;
        }

        _noteProperties.isHandled = true;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        _noteManager.AddFailPoint();

        _audioSource.volume = 0.2f;
        _audioSource.PlayOneShot(_failSound);

        transform.DOShakePosition(_noteProperties.shakeDuration, _noteProperties.shakeStrength)
            .OnComplete(() =>
                {
                    Destroy(gameObject);
                });
    }

    private void OnNoteSuccess()
    {
        if (_noteProperties.isHandled)
        {
            return;
        }

        _noteProperties.isHandled = true;
        _noteManager.AddSuccessPoint();

        _audioSource.volume = 1.0f;
        _audioSource.PlayOneShot(_successSound);

        // Punch towards the camera
        Vector3 direction = (_cameraTransform.position - transform.position).normalized;
        transform.DOPunchPosition(direction, _noteProperties.shakeDuration)
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
