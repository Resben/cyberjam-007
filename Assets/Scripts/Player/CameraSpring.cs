using UnityEngine;
using UnityEngine.UIElements;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)][SerializeField] private float halfLife = 0.075f;
    [SerializeField] private float frequency = 18f;
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    private Vector3 _springPosition;
    private Vector3 _springVelocity;

    public void Initialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }

    /// @Todo document so I know how this actually works
    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        transform.localPosition = Vector3.zero;

        Spring(ref _springPosition, ref _springVelocity, transform.position, halfLife, frequency, deltaTime);

        var localSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up);

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
        transform.localPosition = localSpringPosition * linearDisplacement;
    }

    // https://allenchou.net/2015/04/game-math-more-on-numeric-springing/
    // halfLife: The time it takes for the spring to lose half of its energy. A value of 2 would indicate that the spring will lose half of its energy in 2 seconds.
    // frequency: The frequency of the spring oscillation. A value of 1 would indicate that the spring oscillates once per second.
    private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}
