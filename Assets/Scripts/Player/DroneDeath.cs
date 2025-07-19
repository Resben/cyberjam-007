using UnityEngine;

public class DroneDeath : Effect
{
    [SerializeField] private ParticleSystem _destroyedDrone;
    [SerializeField] private ParticleSystem _destroyBlast;
    [SerializeField] private ParticleSystem _exhaust;
    [SerializeField] private MeshRenderer _droneMesh;
    [SerializeField] private MeshRenderer _lampMesh;
    [SerializeField] private Light _spotLight;

    public override void Play()
    {
        _exhaust.Stop();
        _spotLight.enabled = false;
        _droneMesh.enabled = false;
        _lampMesh.enabled = false;
        _destroyedDrone.Play();
        _destroyBlast.Play();
    }
}
