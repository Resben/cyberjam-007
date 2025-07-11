using System.Collections;
using UnityEngine;

// This class is responsible for managing the notes in the game.
// It will load note data from a JSON file and spawn notes in the game world.
public class Note_Manager : MonoBehaviour
{
    // Note prefab to instantiate
    [SerializeField] private GameObject _notePrefab;

    // Terminal window properties
    //private Transform _terminalWindowTransform; // more ideal to have a panel transform passed in
    [SerializeField] private float _windowHeight;
    [SerializeField] private float _windowWidth;

    private Transform _cameraTransform;

    // Properties for the notes, This should have level difficulty in mind
    [SerializeField] private float _acceptanceWindow; // Time window for accepting a note hit
    [SerializeField] private float _leadWindowTime; // Spawn time before note marker happens

    void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Camera Transform is not set");
            Destroy(gameObject);
        }

        SpawnNotes();
    }

    void OnDestroy()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");

        foreach (GameObject note in notes)
        {
            Destroy(note);
        }
    }

    void SpawnNotes()
    {
        BeatManager beatManager = GetComponent<BeatManager>();
        if (!beatManager)
        {
            Debug.LogError("BeatManager component not found on Note_Manager.");
            return;
        }

        foreach (MusicEvent musicEvent in beatManager.GetMusicData())
        {
            if (musicEvent.data.tags == null || musicEvent.data.tags.Count == 0)
            {
                Debug.LogWarning($"No tags found for music event: {musicEvent.name}");
                continue;
            }

            foreach (BeatNote beatNote in musicEvent.data.tags)
            {
                StartCoroutine(SpawnNoteCoroutine(beatNote.time, GetRandomPosition(), beatNote.duration, _leadWindowTime, _acceptanceWindow));
            }
        }
    }

    private IEnumerator SpawnNoteCoroutine(float spawnTime, Vector3 position, float duration, float leadWindowTime, float acceptanceWindow)
    {
        yield return new WaitForSeconds(spawnTime - _leadWindowTime); // Wait for the note's time before spawning


        GameObject noteObj = Instantiate(_notePrefab, position, Quaternion.identity);
        if (!noteObj)
        {
            Debug.LogError("Failed to instantiate note prefab.");
            yield break;
        }

        noteObj.transform.position = position;
        Note note = noteObj.GetComponent<Note>();
        if (!note)
        {
            Debug.LogError("Note script not found on the note prefab.");
            yield break;
        }

        note.InitializeNote(duration, leadWindowTime, acceptanceWindow, _cameraTransform);

        yield return null;
    }
    
    Vector3 GetRandomPosition()
    {
        float x = Random.Range(- _windowWidth * 0.5f, _windowWidth * 0.5f);
        float y = Random.Range(- _windowHeight * 0.5f, _windowHeight * 0.5f);
        return new Vector3(x, y, -20f); // Fixed Z position for testing
    }
}
