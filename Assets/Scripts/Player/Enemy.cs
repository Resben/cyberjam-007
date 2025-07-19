using System.Collections;
using System.Linq;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] private TrailConfigScriptableObject trailConfig;
    [SerializeField] private Transform shootStart;
    private Player _player;
    private Rigidbody _rb;

    private float _lastShot;

    void Start()
    {
        var playerObject = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _player = playerObject != null ? playerObject.GetComponent<Player>() : null;
        if (_player)
            agent.SetPriorityTarget(_player.GetTarget(), 5.0f);
        else
            Debug.LogError("Couldn't find a player");
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        agent.SetSpeed(sprintSpeed);
        agent.state = AgentState.Idle;

        agent.OnPriorityReached.AddListener(() => Shoot());
        _lastShot = Time.time;
    }

    void Update()
    {
        agent.UpdateAgent();
    }

    public Rigidbody GetRigidbody()
    {
        return _rb;
    }

    public void SetRagdoll()
    {
        agent.state = AgentState.Idle;
        _rb.isKinematic = false;
        _rb.freezeRotation = false;
        _rb.constraints = RigidbodyConstraints.None;
    }

    public override void Trigger(string type)
    {
        switch (type)
        {
            case "spawn":
                agent.state = AgentState.Tracking;
                break;
            default:
                break;
        }
    }

    private void Shoot()
    {
        if (Time.time - _lastShot < 1.5f)
            return;

        _lastShot = Time.time;

        float spread = 0.01f;
        Vector3 shootDirection = shootStart.transform.forward + new Vector3(
            Random.Range(spread, spread),
            Random.Range(spread, spread),
            Random.Range(spread, spread)
        );

        shootDirection.Normalize();

        if (Physics.Raycast(shootStart.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("PlayerBody"))
            {
                StartCoroutine(PlayTrail(shootStart.position, hit.point, hit));
            }
        }
        else
        {
            StartCoroutine(PlayTrail(shootStart.position, shootStart.transform.position + (shootDirection * 500f), new RaycastHit()));
        }
    }

    private IEnumerator PlayTrail(Vector3 start, Vector3 end, RaycastHit hit)
    {
        TrailRenderer trail = CreateTrail();
        trail.gameObject.SetActive(true);
        trail.transform.position = start;
        yield return null;

        trail.enabled = true;
        trail.emitting = true;

        float distance = Vector3.Distance(start, end);
        float t = 0f;
        while (t < 1f)
        {
            t += 50 / distance * Time.deltaTime;
            trail.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        trail.transform.position = end;

        if (hit.collider != null)
        {
            Agent entity = hit.collider.GetComponent<Agent>();
            entity?.Hit();
        }
        else
        {
            trail.transform.rotation = Quaternion.LookRotation((end - start).normalized, Vector3.up);
        }

        yield return new WaitForSeconds(0.5f);
        yield return null;
        trail.emitting = false;
        trail.enabled = false;
        trail.gameObject.SetActive(false);
        Destroy(trail);
    }
    
    protected TrailRenderer CreateTrail()
    {
        TrailRenderer trail = new GameObject("Trail").AddComponent<TrailRenderer>();
        trail.colorGradient = trailConfig.color;
        trail.material = trailConfig.material;
        trail.widthCurve = trailConfig.widthCurve;
        trail.time = trailConfig.duration;
        trail.minVertexDistance = trailConfig.minVertedxDistance;

        trail.enabled = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    public override void Hit() { }
}
