using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float strength = 0.075f;

    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVelocity;
    public void Initialize() {}

    public void UpdateLean(float deltaTime, Vector3 acceleration, Vector3 up)
    {
        // Get the target camera position based on planar acceleration
        // and determine the damping factor based if we are in attack or decay phase
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decayDamping;

        // Smoothly damp the acceleration vector towards the planar acceleration
        // saving the current velocity for the next frame
        _dampedAcceleration = Vector3.SmoothDamp(_dampedAcceleration, planarAcceleration, ref _dampedAccelerationVelocity, damping, float.PositiveInfinity, deltaTime);

        // Axis to lean the camera towards is perpendicular to the damped acceleration
        // Rotate the camera around the lean axis
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
        transform.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * strength, leanAxis) * transform.rotation;
    }
}
