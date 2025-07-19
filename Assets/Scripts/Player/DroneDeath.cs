using FMODUnity;
using UnityEngine;

public class DroneDeath : Effect
{
    [SerializeField] private ParticleSystem _destroyedDrone;
    [SerializeField] private ParticleSystem _destroyBlast;
    [SerializeField] private ParticleSystem _exhaust;
    [SerializeField] private MeshRenderer _droneMesh;
    [SerializeField] private MeshRenderer _lampMesh;
    [SerializeField] private Light _spotLight;
    [SerializeField] private EventReference _soundExplosion;

    public override void Play()
    {

        var gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        var volume = gameManager.GetVolume();
        var instance = RuntimeManager.CreateInstance(_soundExplosion);
        instance.setVolume(volume);
        RuntimeManager.AttachInstanceToGameObject(instance, gameObject, GetComponent<Rigidbody>());
        instance.start();

        _exhaust.Stop();
        _spotLight.enabled = false;
        _droneMesh.enabled = false;
        _lampMesh.enabled = false;
        _destroyedDrone.Play();
        _destroyBlast.Play();
    }
}
