namespace Facepunch;

public interface IUse : IValid
{
	public bool CanUse( PlayerPawn player );
	public void OnUse( PlayerPawn player );
}
