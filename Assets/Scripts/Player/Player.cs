using UnityEngine;

public class Player : Entity
{
    [Header("Components")]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private Target target;
    [SerializeField] private Animator animator;

    private PlayerInputActions _inputActions;
    private bool _allowUpdate = false;

    void Start()
    {
        //agent.OnEndReached.AddListener(() => Debug.Log("YOU WON!"));

        Cursor.lockState = CursorLockMode.None;

        _inputActions = GameManager.Instance.inputActions;

        agent.SetSpeed(walkSpeed);

        playerCamera.Initialize(agent.transform.position);
        cameraSpring.Initialize();
        cameraLean.Initialize();

        GameManager.Instance.GameResumed.AddListener(() => GameResumed());
        GameManager.Instance.GamePaused.AddListener(() => GamePaused());
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

    private void GameResumed()
    {
        _allowUpdate = true;
    }

    private void GamePaused()
    {
        _allowUpdate = false;
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

    public void SetAnimationState(string state, bool isOn)
    {
        animator.SetBool(state, isOn);
    }

    public override void Trigger(string type)
    {
        switch (type)
        {
            case "start":
                agent.SetSpeed(sprintSpeed);
                break;
        }
    }

    public override void Hit()
    {
        GameManager.Instance.currentLevelManager.LoseCondition();
    }
}
