using System.Collections.Generic;
using UnityEngine;

public interface ITrigger
{
    public void Trigger(string type);
}

public class TriggerVolume : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> triggersGameObjects;
    [SerializeField] private List<string> tagTriggers;
    [SerializeField] private List<string> type;

    private readonly List<ITrigger> _triggers = new();

    private void Awake()
    {
        _triggers.Clear();
        foreach (var behaviour in triggersGameObjects)
        {
            if (behaviour is ITrigger trigger)
                _triggers.Add(trigger);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (var tag in tagTriggers)
        {
            if (other.CompareTag(tag))
            {
                type.ForEach(t => _triggers.ForEach(trigger => trigger.Trigger(t)));
            }
        }
    }
}
