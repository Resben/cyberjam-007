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

    void Start()
    {
        foreach (var musicEvent in music)
        {
            string eventName = musicEvent.Path[(musicEvent.Path.LastIndexOf('/') + 1)..];
            string path = Path.Combine(Application.streamingAssetsPath, "Beats/" + eventName + ".json");
            string jsonText = File.ReadAllText(path);

            BeatData beatData = JsonUtility.FromJson<BeatData>(jsonText);
            MusicEvent musicEventInstance = new MusicEvent
            {
                name = eventName,
                reference = RuntimeManager.CreateInstance(musicEvent),
                data = beatData
            };

            musicData.Add(musicEventInstance);
        }
    }
}