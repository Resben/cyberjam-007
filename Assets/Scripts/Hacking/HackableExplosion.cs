using UnityEngine;

public class HackableExplosion : Hackable
{
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void OnSuccessfulHack()
    {
        Debug.Log("Successful Hack on Explosion");
    }

    public override void OnFailedHack()
    {
        Debug.Log("Failed Hack on Explosion");
    }
}
