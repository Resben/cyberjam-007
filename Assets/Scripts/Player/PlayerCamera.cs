using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
    public Vector3 Position;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minFalloff = 0.15f;
    [Range(0.01f, 1f)][SerializeField] private float fallOffViewPercentage = 0.7f;
    [SerializeField] private float maxYaw = 30f;
    [SerializeField] private float maxPitch = 30f;
    [SerializeField] private Transform CameraPitch;
    [SerializeField] private Transform CameraYaw;
    private float _yaw;
    private float _pitch;
    private CameraInput _requestedInput;
    private Vector3 offset;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;   
    }

    public void Initialize(Vector3 puppetPosition)
    {
        offset = transform.position - puppetPosition;
    }
    public void UpdateCamera(float deltaTime)
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

        Vector3 desiredPosition = _requestedInput.Position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 5f * Time.deltaTime);
    }

    public void UpdateInput(CameraInput input)
    {
        _requestedInput = input;
    }
}
