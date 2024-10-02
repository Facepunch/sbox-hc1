namespace Facepunch.Gunsmith;

/// <summary>
/// We'll use this for gunsmith weapons, to designate where we can display or attach something to a weapon.
/// </summary>
public partial class GunsmithAttachmentPoint : Component
{
	[Property]
	public GunsmithPartType Category { get; set; }

	[Property]
	public GameObject CurrentGameObject { get; set; }

	[Property]
	public GunsmithPart CurrentPart { get; set; }

	/// <summary>
	/// Set the gunsmith part to this part
	/// </summary>
	/// <param name="part"></param>
	public void SetPart( GunsmithPart part )
	{
		if ( CurrentGameObject.IsValid() )
		{
			CurrentGameObject.Destroy();
		}

		CurrentPart = part;

		if ( CurrentPart is null )
		{
			CurrentGameObject = null;
			return;
		}

		CurrentGameObject = CurrentPart.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = GameObject,
			StartEnabled = true,
		} );
	}
}
