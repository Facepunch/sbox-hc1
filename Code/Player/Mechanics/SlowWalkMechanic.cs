namespace Facepunch;

public partial class SlowWalkMechanic : BasePlayerControllerMechanic
{
    [Property, Group( "Config" )] public float SlowWalkSpeed { get; set; } = 70f;

    public override bool ShouldBecomeActive()
    {
        return Input.Down( "Run" ) && !HasAnyTag( "crouch" );
    }

    public override IEnumerable<string> GetTags()
    {
        yield return "slow_walk";
    }

    public override float? GetSpeed()
    {
        return SlowWalkSpeed;
    }
}
