using DG.Tweening;
using UnityEngine;

public abstract class Hackable : MonoBehaviour
{
    private NoteManager _noteManager;
    private BeatManager _beatManager;
    [SerializeField] private GameObject _noteManagerPrefab;
    [SerializeField] private GameObject _beatManagerPrefab;
    private MeshRenderer _meshRenderer;
    private Color _originalColor;

    [SerializeField] private int _trackNumber = -1;

    private bool _inHackPhase = false;
    private float _score = 0;
    [SerializeField, Range(0, 100)]
    private int _passPercentage = 80;

    protected virtual void Start()
    {
        if (!_noteManagerPrefab || !_beatManagerPrefab)
        {
            Debug.LogError("Note or beat Managers are not set");
            Destroy(gameObject);
            return;
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

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    OnClicked();
                }
            }
        }

        // @DEBUG
        if (Input.GetButtonDown("Jump"))
        {
            StopHackPhase();
        }
    }

    protected virtual void OnClicked()
    {
        if (_inHackPhase)
        {
            return;
        }

        _inHackPhase = true;
        gameObject.GetComponent<BoxCollider>().enabled = false;

        StartHackPhase();
    }

    protected virtual void StartHackPhase()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        _noteManager = Instantiate(_noteManagerPrefab, pos, Quaternion.identity).GetComponent<NoteManager>();
        _beatManager = Instantiate(_beatManagerPrefab, pos, Quaternion.identity).GetComponent<BeatManager>();

        if (!_noteManager || !_beatManager)
        {
            Debug.LogError("Issues with instantiation of Managers");
        }

        MusicEvent musicEvent = _beatManager.GetMusicData()[_trackNumber];

        _noteManager.Initialise(musicEvent, this);
        _beatManager.StartMusic(_trackNumber);

        // @TODO: Revisit this
        _meshRenderer.material.DOColor(new Color(255.0f, 0.0f, 255.0f), 1.0f)
            .SetEase(Ease.InExpo);
    }

    protected virtual void StopHackPhase()
    {
        _inHackPhase = false;
        gameObject.GetComponent<BoxCollider>().enabled = true; // @DEBUG

        Destroy(_noteManager);
        Destroy(_beatManager);

        // @TODO: Revisit this
        _meshRenderer.material.DOColor(_originalColor, 1.0f)
            .SetEase(Ease.InExpo);
    }

    public void EndHack(float score)
    {
        _score = score;
        Debug.Log($"Total score = {_score}");
        StopHackPhase();
        DetermineSuccess();
    }

    public int PassPercentage
    {
        get => _passPercentage;
        set => _passPercentage = Mathf.Clamp(value, 0, 100);
    }

    private void DetermineSuccess()
    {
        float passRate = PassPercentage / 100.0f;
        Debug.Log($"Score: {_score}, PassRate: {passRate}");
        if (_score < passRate)
        {
            onFailedHack();
        }
        else
        {
            OnSuccessfulHack();
        }
    }

    public abstract void OnSuccessfulHack();
    public abstract void onFailedHack();
}
