namespace Facepunch.Gunsmith;

/// <summary>
/// We'll use this for gunsmith weapons, to designate where we can display or attach something to a weapon.
/// </summary>
public partial class GunsmithAttachmentPoint : Component
{
	[Property]
	public GunsmithPartType Category { get; set; }
}
