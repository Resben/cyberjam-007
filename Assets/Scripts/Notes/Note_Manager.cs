using System.Collections;
using System.Linq;
using UnityEngine;

// This class is responsible for managing the notes in the game.
// It will load note data from a JSON file and spawn notes in the game world.
public class Note_Manager : MonoBehaviour
{
    // Note prefab to instantiate
    [SerializeField] private GameObject _notePrefab;

    // Terminal window properties
    [SerializeField] private Transform _terminalWindowTransform;
    [SerializeField] private float _windowHeight = 10.0f; // Height of the terminal window
    [SerializeField] private float _windowWidth = 20.0f; // Width of the terminal window

    [SerializeField] private Transform _cameraTransform;

    // Properties for the notes, This should have level difficulty in mind
    [SerializeField] private float _acceptanceWindow = 0.2f;
    [SerializeField] private float _leadWindowTime = 2.0f; // Time before the note occurs

    private GameObject[] _notes;

    void Awake()
    {
        // Load the note data from the JSON file, such as name, duration and timestamp.
        BeatManager beatManager = GetComponent<BeatManager>();
        if (!beatManager)
        {
            Debug.LogError("BeatManager component not found on Note_Manager.");
            return; // Exit if BeatManager is not found
        }
    }

    void Start()
    {
        // Know when to spawn notes based on the loaded data
        // Instantiate notes using the _notePrefab, position them randomly within a certain range,
        // and set their properties like duration, grace window, and acceptance window.
        // On each entry in the note data, spawn a note at the specified position and rotation.

        StartCoroutine(SpawnNotesCoroutine());

        // After full track has been played, clean up the notes
        // Destroy();
    }

    public void DestroyManager()
    {
        OnDestroy(); // Call OnDestroy to clean up the notes
    }

    private void OnDestroy()
    {
        if (_notes == null)
        {
            return; // If notes are already null, do nothing
        }

        // Clean up notes when the manager is destroyed
        foreach (GameObject note in _notes)
        {
            if (note)
            {
                Destroy(note); // Call DestroyNote to clean up the note
            }
        }
        _notes = null; // Clear the notes array
    }

    IEnumerator SpawnNotesCoroutine()
    {
        // @DEBUG: Wait for a short duration before starting to spawn notes
        //yield return new WaitForSeconds(1f);

        BeatManager beatManager = GetComponent<BeatManager>();

        if (!beatManager)
        {
            Debug.LogError("BeatManager component not found on Note_Manager.");
            yield break; // Exit if BeatManager is not found
        }

        // @DEBUG: remove while loop when done testing
        while (true)
        {
            // Loop through the notes and spawn them at their respective positions
            foreach (MusicEvent musicEvent in beatManager.GetMusicData())
            {
                if (musicEvent.data.tags == null || musicEvent.data.tags.Count == 0)
                {
                    Debug.LogWarning($"No tags found for music event: {musicEvent.name}");
                    continue; // Skip if no tags are found
                }

                foreach (BeatNote beatNote in musicEvent.data.tags)
                {
                    Debug.LogWarning($"Spawning note: {beatNote.name} at time: {beatNote.time} with duration: {beatNote.duration}");
                    // Calculate the position based on the note's time and other properties
                    Vector3 notePosition = new Vector3(
                        Random.Range(-_windowWidth / 2, _windowWidth / 2),
                        Random.Range(1f, _windowHeight - 1f),
                        -20f // Fixed Z position for simplicity
                    );

                    yield return new WaitForSeconds(beatNote.time - _leadWindowTime); // Wait for the note's time before spawning

                    SpawnNote(notePosition, beatNote.duration, _leadWindowTime, _acceptanceWindow);
                }
            }
        }
        
    }

    void SpawnNote(Vector3 position, float duration, float leadWindowTime, float acceptanceWindow)
    {
        // Instantiate a note at the specified position and rotation
        GameObject note = Instantiate(_notePrefab, position, Quaternion.identity);
        if (!note)
        {
            Debug.LogError("Failed to instantiate note prefab.");
            return; // Exit if the note prefab could not be instantiated
        }

        /* 
        * Need to work on this later
        if (_notes == null)
        {
            _notes = new GameObject[0]; // Initialize the notes array if it is null
        }

        _notes.Append(note); // Add the note to the notes array
        */

        // Set a random position on screen for the note
        note.transform.position = GetRandomPosition();

        // Pass duration, grace window, acceptance window, into its script
        Note noteScript = note.GetComponent<Note>();
        if (!noteScript)
        {
            Debug.LogError("Note script not found on the note prefab.");
            return; // Exit if the Note script is not found
        }

        noteScript.InitializeNote(duration, leadWindowTime, acceptanceWindow, _cameraTransform);
    }
    
    Vector3 GetRandomPosition()
    {
        // Generate a random position within the terminal window bounds
        float x = Random.Range(-_windowWidth / 2, _windowWidth / 2);
        float y = Random.Range(1f, _windowHeight - 1f);
        return new Vector3(x, y, -20f); // Fixed Z position for simplicity
    }

   
}
