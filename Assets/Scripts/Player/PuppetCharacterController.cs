using KinematicCharacterController;
using UnityEngine;

public enum InputType
{
    On, Off
}

public enum Stance
{
    Stand
}

public struct CharacterState
{
    public Stance Stance;
    public bool Grounded;
    public Vector3 Velocity;
    // Not raw acceleration, excludes gravity and other forces
    // User movement driven acceleration
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Vector2 Move; // Currently only on the z axis
    public InputType Sprint;
}

public class PuppetCharacterController : MonoBehaviour, ICharacterController
{
    [Header("Character Components")]
    [SerializeField] private Transform cameraTarget;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6.5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float airSpeed = 6.5f;
    [SerializeField] private float airAcceleration = 6.5f;
    [SerializeField] private float gravity = -50f;

    private KinematicCharacterMotor motor;

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    private Vector3 _requestedMovement;
    private bool _requestedSprint;

    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;

        motor = GetComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _lastState = _state;

        // Always move forward along Z-axis (ignore input)
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

        // Still respond to sprint input if needed
        _requestedSprint = input.Sprint switch
        {
            InputType.On => true,
            InputType.Off => false,
            _ => _requestedSprint
        };
        
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;

        // Grounded movement
        if (motor.GroundingStatus.IsStableOnGround)
        {
            var groundedMovement = motor.GetDirectionTangentToSurface(_requestedMovement, motor.GroundingStatus.GroundNormal) * _requestedMovement.magnitude;

            // Calculate speed and response based on Stance
            var speed = _requestedSprint ? sprintSpeed : walkSpeed;
            var response = walkResponse;

            // Apply response to the current velocity
            var targetVelocity = groundedMovement * speed;
            var moveVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime) // Remain framerate independent
            );

            _state.Acceleration = (moveVelocity - currentVelocity) / deltaTime;
            currentVelocity = moveVelocity;
        }
        // Air movement
        else
        {
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                // Calculate air movement based on the requested movement same as regular movement but using the Up
                var planarMovement = Vector3.ProjectOnPlane(_requestedMovement, motor.CharacterUp) * _requestedMovement.magnitude;
                var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                var movementForce = planarMovement * airAcceleration * deltaTime;

                var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                movementForce = targetPlanarVelocity - currentPlanarVelocity;

                currentVelocity += movementForce;
            }

            var effectiveGravity = gravity;
            currentVelocity += deltaTime * effectiveGravity * motor.CharacterUp;
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {}

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;

        _lastState = _tempState;
    }


    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public Transform GetCameraTarget() => cameraTarget;
    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);

        if (killVelocity)
        {
            motor.BaseVelocity = Vector3.zero;
        }
    }

    public Vector3 GetPosition()
    {
        return motor.TransientPosition;
    }
}
