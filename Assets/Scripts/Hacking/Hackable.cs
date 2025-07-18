using DG.Tweening;
using UnityEngine;

public abstract class Hackable : MonoBehaviour
{
    [SerializeField] protected Effect effect;
    [SerializeField] private GameObject _hackingManagerObj;
    private HackingManager _hackingManager;
    private MeshRenderer _meshRenderer;
    private Color _originalColor;

    [SerializeField] private int _trackNumber = -1;

    [SerializeField, Range(0, 100)]
    private int _passPercentage = 80;

    private bool _isHacking = false;

    protected virtual void Start()
    {
        if (!_hackingManagerObj)
        {
            _hackingManagerObj = GameObject.FindGameObjectWithTag("HackingManager");
            if (!_hackingManagerObj)
            {
                Debug.LogError("Hacking Manager GameObject has not been placed");
                Destroy(gameObject);
                return;
            }
        }

        if (!_hackingManager)
        {
            _hackingManager = _hackingManagerObj.GetComponent<HackingManager>();
        }

        if (_trackNumber == -1)
        {
            Debug.LogError("Music Track is not selected!!");
            Destroy(gameObject);
            return;
        }

        _meshRenderer = GetComponent<MeshRenderer>();
        if (!_meshRenderer)
        {
            Debug.LogError("Error getting mesh renderer");
            Destroy(gameObject);
            return;
        }

        _originalColor = _meshRenderer.material.color;
    }

    public virtual void OnClicked()
    {
        StartHackPhase();
    }

    protected virtual void StartHackPhase()
    {
        if (_isHacking) return;

        _hackingManager.CreateHackingSession(this);
    }

    protected virtual void StopHackPhase()
    {
        if (!_isHacking) return;

        _hackingManager.DestroyHackingSession();
    }

    public virtual void StartHack()
    {
        OnStartHackPhase();
    }

     public void EndHack()
    {        
        OnStopHackPhase();
    }

    protected virtual void OnStartHackPhase()
    {
        _isHacking = true;
        gameObject.GetComponent<Collider>().enabled = false;

        // @TODO: Revisit this
        _meshRenderer.material.DOColor(new Color(255.0f, 0.0f, 255.0f), 1.0f)
            .SetEase(Ease.InExpo);
    }

    protected virtual void OnStopHackPhase()
    {
        _isHacking = false;
        gameObject.GetComponent<Collider>().enabled = true;

        // @TODO: Revisit this
        _meshRenderer.material.DOColor(_originalColor, 1.0f)
            .SetEase(Ease.InExpo);
    }

    public int PassPercentage
    {
        get => _passPercentage;
        set => _passPercentage = Mathf.Clamp(value, 0, 100);
    }

    public int GetTrackNumber() => _trackNumber;
    public abstract void OnSuccessfulHack();
    public abstract void OnFailedHack();
}
