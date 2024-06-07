namespace Facepunch;

public interface IUse : IValid
{
	public bool CanUse( PlayerController player );
	public void OnUse( PlayerController player );
}
