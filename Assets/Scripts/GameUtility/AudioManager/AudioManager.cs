using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> _activeBGMInstances = new List<EventInstance>();
    private List<EventInstance> _activeSFXInstances = new List<EventInstance>();

    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayOneShot(EventReference soundReference, Vector3 position, bool isBGM)
    {
        EventInstance instance = RuntimeManager.CreateInstance(soundReference);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setVolume(isBGM ? GameManager.Instance.GetVolume() : GameManager.Instance.GetSFXVolume());
        instance.start();
        instance.release();
    }

    public void PlayOneShot(EventReference soundReference, bool isBGM)
    {
        EventInstance instance = RuntimeManager.CreateInstance(soundReference);
        instance.setVolume(isBGM ? GameManager.Instance.GetVolume() : GameManager.Instance.GetSFXVolume());
        instance.start();
        instance.release();
    }

    public EventInstance CreateEventInstance(EventReference soundReference, bool isBGM)
    {
        EventInstance instance = RuntimeManager.CreateInstance(soundReference);
        instance.setVolume(isBGM ? GameManager.Instance.GetVolume() : GameManager.Instance.GetSFXVolume());
        if (isBGM)
            _activeBGMInstances.Add(instance);
        else
            _activeSFXInstances.Add(instance);
        return instance;
    }

    private void CleanUp()
    {
        foreach (var instance in _activeBGMInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
        foreach (var instance in _activeSFXInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }

    public void RemoveInstance(EventInstance instance)
    {
        if (_activeBGMInstances.Contains(instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            _activeBGMInstances.Remove(instance);
        }
        else if (_activeSFXInstances.Contains(instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            _activeSFXInstances.Remove(instance);
        }
    }

    public void OnVolumeChanged(float volume)
    {
        foreach (var instance in _activeBGMInstances)
        {
            instance.setVolume(volume);
        }
    }

    public void OnSFXVolumeChanged(float sfxVolume)
    {
        foreach (var instance in _activeSFXInstances)
        {
            instance.setVolume(sfxVolume);
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
