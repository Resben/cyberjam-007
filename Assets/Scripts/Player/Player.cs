using UnityEngine;

public class Player : Entity
{
    [Header("Components")]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private Target target;

    private PlayerInputActions _inputActions;

    void Start()
    {
        agent.OnEndReached.AddListener(() => Debug.Log("YOU WON!"));

        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = GameManager.Instance.inputActions;

        playerCamera.Initialize(agent.transform.position);
        cameraSpring.Initialize();
        cameraLean.Initialize();
    }

    void Update()
    {
        agent.UpdateAgent();

        var input = _inputActions.Gameplay;

        var cameraInput = new CameraInput
        {
            Look = input.Look.ReadValue<Vector2>(),
            Position = agent.transform.position,
        };

        playerCamera.UpdateInput(cameraInput);
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        // var cameraTarget = agent.transform.position;

        playerCamera.UpdateCamera(deltaTime);
        // cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        // cameraLean.UpdateLean(deltaTime, state.Acceleration, cameraTarget.up);
    }

    public void Teleport(Vector3 position)
    {
        agent.Warp(position);
    }

    public Vector3 GetPosition()
    {
        return agent.transform.position;
    }

    public Target GetTarget()
    {
        return target;
    }
}
