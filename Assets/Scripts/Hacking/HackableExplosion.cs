using UnityEngine;

public class HackableExplosion : Hackable
{
    [SerializeField] private Effect effect;

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
        effect.Play();
    }

    public override void OnFailedHack()
    {
        Debug.Log("Failed Hack on Explosion");
    }
}
