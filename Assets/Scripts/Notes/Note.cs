using UnityEngine;
using DG.Tweening;

public class Note : MonoBehaviour
{
    // Properties for the note
    public float _duration { get; set; } = 2.0f;
    public float _graceWindow { get; set; } = 0.5f;
    public float _acceptanceWindow { get; set; } = 0.2f;

    // Properties for Note Animation
    public float _shakeDuration { get; set; } = 0.2f;
    public float _shakeStrength { get; set; } = 0.2f;

    // References to the outer ring and its MeshRenderer
    [SerializeField] private Transform _outerRing;
    [SerializeField] private MeshRenderer _outerRingMeshRenderer;

    // Camera reference
    public Transform _cameraTransform { get; set; } = null;

    void Awake()
    {
        // Initialize the note, set up references, etc.
        // This is called when the script instance is being loaded
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // Ensure that the outer ring and its MeshRenderer are set
        if (!_outerRing || !_outerRingMeshRenderer || !meshRenderer)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            return;
        }

        // @TODO: I dont like this, but it works for now
        // Set the outer ring's material to be transparent initially 
        // and possibly the note's material as well
        _outerRingMeshRenderer.material.DOFade(0.0f, 0.0f).SetEase(Ease.InSine);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // Ensure that the outer ring and its MeshRenderer are set
        if (!_outerRing || !_outerRingMeshRenderer || !meshRenderer)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            DestroyNote();
        }

        _outerRingMeshRenderer.material.DOFade(1.0f, _duration).SetEase(Ease.InOutCubic);

        // Start by setting the scale of the outer ring to a larger size
        // This can be done using DOTween to animate the scale of the outer ring
        // Lerp Shrink the outer ring of the note for the duration to reset the scale
        _outerRing.localScale = Vector3.one * 5.0f; // Set initial scale
        // Animate to original scale over duration, to slightly smaller size
        _outerRing.DOScale(Vector3.one * 0.8f, _duration + (0.5f * _graceWindow)).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            OnNoteMissed(); // Call OnNoteMissed when the animation completes
        });
    }

    private void OnDestroy()
    {
        // Clean up the note, e.g., remove it from the scene, reset properties, etc.
        // This can be called when the note is missed or hit successfully
        if (_outerRing)
        {
            _outerRing.DOKill(); // Stop any ongoing animations on the outer ring
        }
        Destroy(gameObject); // Destroy the note GameObject
    }

    public void DestroyNote()
    {
        OnDestroy(); // Call OnDestroy to clean up the note
    }

    public void InitializeNote(float duration, float graceWindow, float acceptanceWindow, Transform cameraTransform)
    {
        // Initialize the note with the given parameters
        _duration = duration;
        _graceWindow = graceWindow;
        _acceptanceWindow = acceptanceWindow;
        _cameraTransform = cameraTransform;
    }

    // This method is called when the note is clicked or tapped
    public bool OnNoteClicked()
    {
        // Handle the note being clicked, e.g., play a sound based on the hit type (hit, miss, grace hit)
        // You can also use DOTween to animate the note or its outer ring
        // If Click occurs within the acceptance window, you can trigger a successful hit
        // else you can trigger a miss or a grace hit

        // @DEBUG: For now, we will assume the note is clicked successfully
        OnNoteHit(); // Call OnNoteHit to handle the successful hit

        return true; // Return true if the note was successfully clicked
    }

    // This method is called when the note is missed
    public void OnNoteMissed()
    {
        // Handle the note being missed, e.g., play a miss sound, trigger a miss animation, etc.
        // You can also use DOTween to animate the note or its outer ring to indicate a miss
        transform.DOShakePosition(_shakeDuration, _shakeStrength).OnComplete(() =>
        {
            // After shaking, destroy the note
            DestroyNote();
        });
    }

    // This method is called when the note is hit with grace
    public void OnNoteGraceHit()
    {
        // Handle the note being hit with grace, e.g., play a grace hit sound, trigger a grace hit animation, etc.
        // You can also use DOTween to animate the note or its outer ring to indicate a grace hit
    }

    // This method is called when the note is hit successfully
    public void OnNoteHit()
    {
        // Handle the note being hit successfully, e.g., play a hit sound, trigger a hit animation, etc.
        // You can also use DOTween to animate the note or its outer ring to indicate a successful hit

        // get vector3 between the camera and the note
        Vector3 direction = (_cameraTransform.position - transform.position).normalized;
        transform.DOPunchPosition(direction, _shakeDuration);

        // Play a hit sound

        // Add to progress meter or something

        DestroyNote();
    }
}
