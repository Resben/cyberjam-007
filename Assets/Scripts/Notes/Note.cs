using UnityEngine;
using DG.Tweening;

public class Note : MonoBehaviour
{
    // Properties for the note
    private float _duration { get; set; } = 2.0f;
    private float _graceWindow { get; set; } = 0.5f;
    private float _acceptanceWindow { get; set; } = 0.2f;

    // Properties for Note Animation
    private float _shakeDuration { get; set; } = 0.2f;
    private float _shakeStrength { get; set; } = 0.2f;

    // References to the outer ring and its MeshRenderer
    [SerializeField] private Transform _outerRing;
    [SerializeField] private MeshRenderer _outerRingMeshRenderer;

    // Camera reference
    [SerializeField] private Transform _cameraTransform = null;

    void Awake()
    {
        // Ensure that the outer ring and its MeshRenderer are set
        if (!_outerRing || !_outerRingMeshRenderer || !_cameraTransform)
        {
            Debug.LogError("Outer ring or MeshRenderer references is not set on the note.");
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _outerRingMeshRenderer.material.DOFade(1.0f, _duration * 0.25f).SetEase(Ease.InOutCubic);

        // Start by setting the scale of the outer ring to a larger size
        // This can be done using DOTween to animate the scale of the outer ring
        // Lerp Shrink the outer ring of the note for the duration to reset the scale
        _outerRing.localScale = Vector3.one * 5.0f; // Set initial scale
        // Animate to original scale over duration, to slightly smaller size
        _outerRing.DOScale(Vector3.one * 0.8f, _duration + (0.5f * _graceWindow))
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
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

    void Update()
    {
        // listen for input events, e.g., mouse clicks or touches
        // If the note is clicked or tapped, call OnNoteClicked
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Note clicked: " + hit.transform.name); // Debug log for clicked note
                if (hit.transform == transform) // Check if the clicked object is this note
                {
                    OnNoteClicked(); // Call OnNoteClicked to handle the click
                }
            }
        }   
    }

    // This method is called when the note is clicked or tapped
    public bool OnNoteClicked()
    {
        // Handle the note being clicked, e.g., play a sound based on the hit type (hit, miss, grace hit)
        // You can also use DOTween to animate the note or its outer ring
        // If Click occurs within the acceptance window, you can trigger a successful hit
        // else you can trigger a miss or a grace hit

        transform.DOKill(); // Stop any ongoing animations on the outer ring

        // @DEBUG: For now, we will assume the note is clicked successfully
        OnNoteHit(); // Call OnNoteHit to handle the successful hit

        return true; // Return true if the note was successfully clicked
    }

    // This method is called when the note is missed
    public void OnNoteMissed()
    {
        // Play a Missed sound

        // Subtract from the progress meter or something
        
        transform.DOShakePosition(_shakeDuration, _shakeStrength)
            .OnComplete(() =>
                {
                    // After shaking, destroy the note
                    DestroyNote();
                });
    }

    // This method is called when the note is hit with grace
    public void OnNoteGraceHit()
    {
        // Play a Grace sound

        // Maybe animate phase out or change color
    }

    // This method is called when the note is hit successfully
    public void OnNoteHit()
    {
        // Play a hit sound
        
        // Add to progress meter or something

        // get vector3 between the camera and the note
        Vector3 direction = (_cameraTransform.position - transform.position).normalized;
        transform.DOPunchPosition(direction, _shakeDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
                {
                    // After the punch animation, destroy the note
                    DestroyNote();
                });
    }
}
