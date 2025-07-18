namespace Facepunch;

[Title( "PIP Scope" ), Group( "Weapon Components" )]
public class PictureInPictureScope : WeaponInputAction
{
	[Property] public SoundEvent ZoomSound { get; set; }
	[Property] public SoundEvent UnzoomSound { get; set; }

	public bool IsZooming { get; set; } = false;

	protected override void OnInputDown()
	{
		if ( !IsZooming )
		{
			StartZoom();
		}
		else
		{
			EndZoom();
		}
	}

	public void StartZoom()
	{
		IsZooming = true;
		Equipment.SetTag( "aiming", true );
		Equipment.SetTag( "scoped", true );
	}

	public void EndZoom()
	{
		IsZooming = false;
		Equipment.SetTag( "aiming", false );
		Equipment.SetTag( "scoped", false );
	}

	protected virtual bool CanAim()
	{
		if ( Tags.Has( "reloading" ) ) return false;
		return true;
	}

	protected override void OnDisabled()
	{
		EndZoom();
	}

	protected override void OnParentChanged( GameObject oldParent, GameObject newParent )
	{
		base.OnParentChanged( oldParent, newParent );
		EndZoom();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !CanAim() )
		{
			EndZoom();
		}
	}
};
