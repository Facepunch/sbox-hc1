namespace Facepunch;

public record UseResult
{
	public bool CanUse { get; set; } = false;
	public string Reason { get; set; }

	// How on god's green earth is this legal
	public static implicit operator UseResult( bool boolean ) => new UseResult() { CanUse = boolean };
	public static implicit operator UseResult( string reason ) => new UseResult() { CanUse = false, Reason = reason };
}

public interface IUse : IValid
{
	public UseResult CanUse( PlayerPawn player );
	public void OnUse( PlayerPawn player );
}
