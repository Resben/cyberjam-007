using System.Collections.Generic;
using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
    public Vector3 Position;
    public CharacterState CharacterState;
}

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform CameraPitch;
    [SerializeField] private Transform CameraYaw;

    [Space]

    [Header("Camera Input")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minFalloff = 0.15f;
    [Range(0.01f, 1f)][SerializeField] private float fallOffViewPercentage = 0.7f;
    [SerializeField] private float maxYaw = 30f;
    [SerializeField] private float maxPitch = 30f;

    [Space]

    [Header("Camera Dyanmics")]
    [SerializeField] private float cameraFollowResponse = 0.005f;


    private CameraInput _requestedInput;
    private float _yaw;
    private float _pitch;
    private Vector3 _offset;
    private Vector3 _cameraVelocity;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    public void Initialize(Vector3 puppetPosition)
    {
        _offset = transform.position - puppetPosition;
    }

    public void UpdateCamera(float deltaTime)
    {
        UpdateLook();
        UpdatePosition(deltaTime);
    }

    public void UpdateInput(CameraInput input)
    {
        _requestedInput = input;
    }

    public void UpdatePosition(float deltaTime)
    {
        Vector3 targetPosition = _requestedInput.Position + _offset;
        Vector3 currentCameraPosition = transform.position;

        Vector3 desiredVelocity = (targetPosition - currentCameraPosition) * cameraFollowResponse;
        Vector3 newVelocity = Vector3.Lerp(
            _cameraVelocity,
            desiredVelocity,
            1f - Mathf.Exp(-cameraFollowResponse * deltaTime)
        );

        transform.position += newVelocity * deltaTime;
        _cameraVelocity = newVelocity;
    }

    public void UpdateLook()
    {
        // Calculate the current position inside the circle/ellipse
        // 0 represents the center closer to 1 is closer to the edge
        float normYawBefore = _yaw / maxYaw;
        float normPitchBefore = _pitch / maxPitch;
        float ellipseRadiusSqBefore = normYawBefore * normYawBefore + normPitchBefore * normPitchBefore;
        float ellipseRadiusBefore = Mathf.Sqrt(ellipseRadiusSqBefore);

        // Apply elliptical falloff based on distance from center
        // This might need a lot of fine tuning to different curve types
        float falloff = 1f / (1f + Mathf.Exp(6f * (ellipseRadiusBefore - fallOffViewPercentage)));
        falloff = Mathf.Max(falloff, minFalloff);

        // Apply input and falloff
        _yaw += _requestedInput.Look.x * sensitivity * falloff;
        _pitch -= _requestedInput.Look.y * sensitivity * falloff;

        // Clamp yaw/pitch to the ellipse boundary
        float normYaw = _yaw / maxYaw;
        float normPitch = _pitch / maxPitch;
        float ellipseRadiusSq = normYaw * normYaw + normPitch * normPitch;

        if (ellipseRadiusSq > 1f)
        {
            float scale = 1f / Mathf.Sqrt(ellipseRadiusSq);
            _yaw = normYaw * scale * maxYaw;
            _pitch = normPitch * scale * maxPitch;
        }

        // Apply rotation to dedicated transforms
        CameraYaw.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        CameraPitch.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
