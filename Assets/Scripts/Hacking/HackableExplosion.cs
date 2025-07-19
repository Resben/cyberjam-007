using UnityEngine;

public class HackableExplosion : Hackable
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnSuccessfulHack()
    {
        Debug.Log("Success");
        effect.Play();
    }

    public override void OnFailedHack() {}
}
