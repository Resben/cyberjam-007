using UnityEngine;

public class Door : Hackable
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
    
    public override void OnSuccessfulHack()
    {
        Debug.Log("Successful Hack");
    }

    public override void onFailedHack()
    {
        Debug.Log("Failed Hack");
    }
}