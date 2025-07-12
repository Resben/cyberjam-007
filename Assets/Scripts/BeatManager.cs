using System.Collections.Generic;
using System.IO;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

[System.Serializable]
public struct BeatNote
{
    public string name;
    public float time;
    public float duration;
}

[System.Serializable]
public struct BeatData
{
    public TempoInfo tempo;
    public List<BeatNote> tags;
}

[System.Serializable]
public struct TempoInfo
{
    public string tempo;
    public int numerator;
    public int denominator;
}

public struct MusicEvent
{
    public string name;
    public EventInstance reference;
    public BeatData data;
}

public class BeatManager : MonoBehaviour
{
    [SerializeField] private List<EventReference> music;

    private readonly List<MusicEvent> musicData = new();

    void Awake()
    {
        if (music == null || music.Count == 0)
        {
            Debug.LogError("No music events assigned in BeatManager.");
            DestroyImmediate(gameObject);
        }

        foreach (var musicEvent in music)
        {
            string eventName = musicEvent.Path[(musicEvent.Path.LastIndexOf('/') + 1)..];
            string path = Path.Combine(Application.streamingAssetsPath, eventName + ".json");
            string jsonText = File.ReadAllText(path);
            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError($"No JSON data found for {eventName} at {path}");
                continue;
            }

            BeatData beatData = JsonUtility.FromJson<BeatData>(jsonText);
            if (beatData.tags == null || beatData.tags.Count == 0)
            {
                Debug.LogWarning($"No tags found in the beat data for {eventName}.");
                continue;
            }

            MusicEvent musicEventInstance = new MusicEvent
            {
                name = eventName,
                reference = RuntimeManager.CreateInstance(musicEvent),
                data = beatData
            };

            musicData.Add(musicEventInstance);
        }
    }

    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        foreach (var eventReference in music)
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            instance.setVolume(0.5f);
            instance.start();
        }
    }

    public void StopMusic()
    {
        foreach (var eventReference in music)
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            instance.setVolume(0.5f);
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public List<MusicEvent> GetMusicData()
    {
        return musicData;
    }
}