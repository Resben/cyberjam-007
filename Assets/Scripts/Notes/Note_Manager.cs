using System.Linq;
using UnityEngine;

// This class is responsible for managing the notes in the game.
// It will load note data from a JSON file and spawn notes in the game world.
public class Note_Manager : MonoBehaviour
{
    //Note data
    [SerializeField] private string _noteDataFileName;
    [SerializeField] private string _noteDataFilePath;

    // Note prefab to instantiate
    [SerializeField] private GameObject _notePrefab;

    // Terminal window properties
    [SerializeField] private Transform _terminalWindowTransform;
    [SerializeField] private float _windowHeight;
    [SerializeField] private float _windowWidth;

    [SerializeField] private Transform _cameraTransform;
    
    private GameObject[] _notes;

    void Awake()
    {
        // Load the note data from the JSON file, such as name, duration and timestamp.
    }

    void Start()
    {
        // Know when to spawn notes based on the loaded data
        // Instantiate notes using the _notePrefab, position them randomly within a certain range,
        // and set their properties like duration, grace window, and acceptance window.
        // On each entry in the note data, spawn a note at the specified position and rotation.

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
                note.GetComponent<Note>().DestroyNote(); // Call DestroyNote to clean up the note
            }
        }
        _notes = null; // Clear the notes array
    }

    void SpawnNote(Vector3 position, float duration, float graceWindow, float acceptanceWindow)
    {
        // Instantiate a note at the specified position and rotation
        GameObject note = Instantiate(_notePrefab, position, Quaternion.identity);
        note.transform.SetParent(transform); // Set the parent to this manager for organization
        _notes.Append(note); // Add the note to the notes array

        // Set a random position for the note
        note.transform.position = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(1f, 3f),
            Random.Range(-5f, 5f)
        );

        // Pass duration, grace window, acceptance window, into its script
        Note noteScript = note.GetComponent<Note>();
        if (!noteScript)
        {
            Debug.LogError("Note script not found on the note prefab.");
            return; // Exit if the Note script is not found
        }

        noteScript.InitializeNote(duration, graceWindow, acceptanceWindow, _cameraTransform);
    }

   
}
