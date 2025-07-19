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

public struct HackingBeat
{
    public int trackNumber;
    public List<BeatNote> beatNotes;
}

public class BeatManager : MonoBehaviour
{
    [SerializeField] private List<EventReference> musicFile;

    private readonly List<MusicEvent> musicData = new();

    private float musicVolume;

    private List<List<BeatNote>> _beats = new();

    void Awake()
    {
        if (musicFile == null || musicFile.Count == 0)
        {
            Debug.LogError("No music events assigned in BeatManager.");
            DestroyImmediate(gameObject);
        }

        musicVolume = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetVolume();

        foreach (var musicEvent in musicFile)
        {
            BeatData beatData = JsonUtility.FromJson<BeatData>(GetJson());
            if (beatData.tags == null || beatData.tags.Count == 0)
            {
                Debug.LogWarning($"No tags found in the beat data");
                continue;
            }

            var instance = RuntimeManager.CreateInstance(musicEvent);
            EventDescription description;
            instance.getDescription(out description);
            int size = 0;
            description.getLength(out size);

            MusicEvent musicEventInstance = new MusicEvent
            {
                name = "Main MX",
                reference = instance,
                data = beatData,
                length = size
            };

            musicEventInstance.reference.setVolume(musicVolume);

            musicData.Add(musicEventInstance);

        }

        ReadBeats();
    }

    void Start()
    {
        StartMusic(1);
        // ImmediatelyStopMusic(1);
    }

    void OnDestroy()
    {
        for (int i = 0; i < musicData.Count; i++)
        {
            DestroyMusic(i);
        }
    }

    public void StartMusic(int index) => musicData[index].reference.start();
    public void SetParameterByName(int index, string name, float value) => musicData[index].reference.setParameterByName(name, value);
    public void SetParameterByNameWithLabel(int index, string name, string value) => musicData[index].reference.setParameterByNameWithLabel(name, value);
    public void PauseMusic(int index) => musicData[index].reference.setPaused(true);
    public void UnPauseMusic(int index) => musicData[index].reference.setPaused(false);
    public void FadeStopMusic(int index) => musicData[index].reference.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    public void ImmediatelyStopMusic(int index) => musicData[index].reference.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    public void ReleaseMusic(int index) => musicData[index].reference.release();
    public void DestroyMusic(int index)
    {
        //ImmediatelyStopMusic(index);
        ReleaseMusic(index);
    }

    public void PlayMXState(int index, float state) => SetParameterByName(index, "MX State", state);
    public void PlayHackingBeat(int index, float state) => SetParameterByName(index, "Hacking Beat", state);

    private void ReadBeats()
    {
        // @TODO: make a better parser
        // running out of time and I cbf <3
        // ps i hate myself for this, don't look at it pleeeassseee
        BeatNote beat1Start = new();
        BeatNote beat2Start = new();
        BeatNote beat3Start = new();
        BeatNote beat4Start = new();

        List<BeatNote> beat1 = new();
        List<BeatNote> beat2 = new();
        List<BeatNote> beat3 = new();
        List<BeatNote> beat4 = new();

        foreach (var beatNote in musicData[1].data.tags)
        {
            if (beatNote.name == "Beat 1 Start")
                beat1Start = beatNote;
            else if (beatNote.name == "Beat 2 Start")
                beat2Start = beatNote;
            else if (beatNote.name == "Beat 3 Start")
                beat3Start = beatNote;
            else if (beatNote.name == "Beat 4 Start")
            {
                beat4Start = beatNote;
                break;
            }
        }

        foreach (var beatNote in musicData[1].data.tags)
        {
            if (beatNote.name == "Beat 1")
                beat1.Add(new BeatNote
                {
                    name = beatNote.name,
                    time = beatNote.time - beat1Start.time,
                    duration = beatNote.duration
                });
            else if (beatNote.name == "Beat 2")
                beat2.Add(new BeatNote
                {
                    name = beatNote.name,
                    time = beatNote.time - beat2Start.time,
                    duration = beatNote.duration
                });
            else if (beatNote.name == "Beat 3")
                beat3.Add(new BeatNote
                {
                    name = beatNote.name,
                    time = beatNote.time - beat3Start.time,
                    duration = beatNote.duration
                });
            else if (beatNote.name == "Beat 4")
                beat4.Add(new BeatNote
                {
                    name = beatNote.name,
                    time = beatNote.time - beat4Start.time,
                    duration = beatNote.duration
                });
        }

        _beats.Add(beat1);
        _beats.Add(beat2);
        _beats.Add(beat3);
        _beats.Add(beat4);
    }

    public HackingBeat GetRandomBeat()
    {
        int index = Random.Range(1, 4);
        HackingBeat HackingBeat = new HackingBeat { trackNumber = index, beatNotes = _beats[index] };
        return HackingBeat;
    }

    public string GetJson()
    {
        return @"
        {
            ""tempo"": {
                ""tempo"": 185,
                ""numerator"": 4,
                ""denominator"": 4
            },
            ""tags"": [
                {
                ""name"": ""Beat 1 Start"",
                ""time"": 204.972959,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 1"",
                ""time"": 207.5675625,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 1"",
                ""time"": 210.16216666666668,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 1"",
                ""time"": 212.75675,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 1"",
                ""time"": 215.35008333333334,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 2 Start"",
                ""time"": 215.32392331,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 2"",
                ""time"": 217.9459375,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 2"",
                ""time"": 220.56795169059322,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 2"",
                ""time"": 223.12508049571534,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 2"",
                ""time"": 225.72972916666666,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 3 Start"",
                ""time"": 228.0093524359867,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 3"",
                ""time"": 230.5937676765293,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 3"",
                ""time"": 233.1781829170719,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 3"",
                ""time"": 235.7859865308321,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 3"",
                ""time"": 238.38599402018642,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 4 Start"",
                ""time"": 238.05855682008877,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 4"",
                ""time"": 240.65076816008877,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 4"",
                ""time"": 243.24297952503724,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 4"",
                ""time"": 245.83783333333332,
                ""duration"": -1
                },
                {
                ""name"": ""Beat 4"",
                ""time"": 248.4324375,
                ""duration"": -1
                }
            ]
            } 
        ";
    }
}