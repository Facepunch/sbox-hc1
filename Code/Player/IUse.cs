namespace Facepunch;

public interface IUse : IValid
{
	public bool CanUse( PlayerController player );
	public bool OnUse( PlayerController player );
}
