namespace Facepunch.Gunsmith;

/// <summary>
/// We'll use this for gunsmith weapons, to designate where we can display or attach something to a weapon.
/// </summary>
public partial class GunsmithAttachment : Component
{
	[Property]
	public GunsmithAttachmentPoint AttachmentPoint { get; set; }

	public void Initialize( GunsmithAttachmentPoint attachmentPoint )
	{
		AttachmentPoint = attachmentPoint;
		OnInitialize();
	}

	protected virtual void OnInitialize()
	{
	}
}
