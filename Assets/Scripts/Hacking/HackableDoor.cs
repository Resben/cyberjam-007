using UnityEngine;

public class HackableDoor : Hackable
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnSuccessfulHack()
    {
        Debug.Log("Successful Hack on Door");
    }

    public override void OnFailedHack()
    {
        Debug.Log("Failed Hack on Door");
    }
}