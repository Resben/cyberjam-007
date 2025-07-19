using UnityEngine;

public class HackableExplosion : Hackable
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnSuccessfulHack()
    {
        effect.Play();
    }

    public override void OnFailedHack()
    {
        Debug.Log("Failed Hack on Explosion");
        effect.Play(); // @Debug
    }
}
