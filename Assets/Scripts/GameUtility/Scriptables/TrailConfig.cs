using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Sound Rebel Scriptables/Trail Config", order = 0)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material material;
    public AnimationCurve widthCurve;
    public float duration = 0.5f;
    public float minVertedxDistance = 0.1f;
    public Gradient color;

    public float missDistance = 100f;
    public float simulationSpeed = 1f;
}