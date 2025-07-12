using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PuppetCharacterController puppetCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

    private PlayerInputActions _inputActions;
    private CharacterInput _characterInput;

    void Start()
    {
        _characterInput = new CharacterInput
        {
            Move = new Vector2(0, 1),
            Sprint = InputType.On
        };

        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        puppetCharacter.Initialize();

        playerCamera.Initialize(puppetCharacter.GetPosition());
        cameraSpring.Initialize();
        cameraLean.Initialize();
        StartCoroutine(KillVelocity());
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;

        var cameraInput = new CameraInput
        {
            Look = input.Look.ReadValue<Vector2>(),
            Position = puppetCharacter.GetPosition(),
            CharacterState = puppetCharacter.GetState()
        };

        playerCamera.UpdateInput(cameraInput);

        puppetCharacter.UpdateInput(_characterInput);
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = puppetCharacter.GetCameraTarget();
        var state = puppetCharacter.GetState();

        playerCamera.UpdateCamera(deltaTime);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, state.Acceleration, cameraTarget.up);
    }

    public void Teleport(Vector3 position)
    {
        puppetCharacter.SetPosition(position);
    }

    public Vector3 GetPosition()
    {
        return puppetCharacter.GetPosition();
    }

    public Stance GetStance()
    {
        return puppetCharacter.GetState().Stance;
    }

    // Test to kill velocity every 10 seconds
    // See how the camera behaves
    private IEnumerator KillVelocity()
    {
        while (true)
        {
            _characterInput.Move = new Vector2(0, 1);
            yield return new WaitForSeconds(7.5f);

            _characterInput.Move = Vector2.zero;
            yield return new WaitForSeconds(2.5f);
        }
    }
}
