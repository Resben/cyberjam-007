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
    public int length;
}

public class BeatManager : MonoBehaviour
{
    [SerializeField] private List<EventReference> musicFile;

    private readonly List<MusicEvent> musicData = new();

    [SerializeField] private float musicVolume = 1.0f;

    void Awake()
    {
        if (musicFile == null || musicFile.Count == 0)
        {
            Debug.LogError("No music events assigned in BeatManager.");
            DestroyImmediate(gameObject);
        }

        foreach (var musicEvent in musicFile)
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

            var instance = RuntimeManager.CreateInstance(musicEvent);
            EventDescription description;
            instance.getDescription(out description);
            int size = 0;
            description.getLength(out size);

            MusicEvent musicEventInstance = new MusicEvent
            {
                name = eventName,
                reference = instance,
                data = beatData,
                length = size
            };
            

            musicEventInstance.reference.setVolume(musicVolume);

            musicData.Add(musicEventInstance);
        }
    }

    void Start()
    {

    }

    void OnDestroy()
    {
        for (int i = 0; i < musicData.Count; i++)
        {
            DestroyMusic(i);
        }
    }

    public void StartMusic(int index) => musicData[index].reference.start();
    public void PauseMusic(int index) => musicData[index].reference.setPaused(true);
    public void UnPauseMusic(int index) => musicData[index].reference.setPaused(false);
    public void FadeStopMusic(int index) => musicData[index].reference.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    public void ImmediateStopMusic(int index) => musicData[index].reference.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    public void ReleaseMusic(int index) => musicData[index].reference.release();
    public void DestroyMusic(int index)
    {
        ImmediateStopMusic(index);
        ReleaseMusic(index);
    }

    public List<MusicEvent> GetMusicData()
    {
        return musicData;
    }
}