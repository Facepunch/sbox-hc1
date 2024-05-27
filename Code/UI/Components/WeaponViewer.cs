using Sandbox.Html;
using Sandbox.UI;

namespace Facepunch;

public class WeaponViewer : ScenePanel
{
	public SceneModel WeaponModel { get; set; }

	public WeaponDataResource WeaponResource { get; set; }

	public float TimeScale { get; set; } = 1.0f;

	public WeaponViewer()
	{
		World = new SceneWorld();
		new SceneLight( World, Vector3.Left * 50f + Vector3.Up * 150.0f, 512, new Color( 1f, 0.4f, 0.4f ) * 4.0f );
		new SceneLight( World, Vector3.Right * 50f + Vector3.Up * 150.0f, 512, new Color( 0.4f, 0.4f, 1f ) * 4.0f );
		new SceneLight( World, Vector3.Up * 150.0f + Vector3.Backward * 100.0f, 512, new Color( 0.7f, 0.8f, 1 ) * 3.0f );
	}

	public void Update()
	{
		WeaponModel?.Delete();
		WeaponModel = null;

		if ( WeaponResource == null )
		{
			// Couldn't find weapon definition?
			return;
		}

		SetModel( WeaponResource.WorldModel );

		var bounds = WeaponModel.Bounds;
		var middle = (bounds.Mins + bounds.Maxs) * 0.5f;

		var size = 0f;
		size = MathF.Max( size, MathF.Abs( bounds.Mins.x ) + MathF.Abs( bounds.Maxs.x ) );
		size = MathF.Max( size, MathF.Abs( bounds.Mins.y ) + MathF.Abs( bounds.Maxs.y ) );
		size = MathF.Max( size, MathF.Abs( bounds.Mins.z ) + MathF.Abs( bounds.Maxs.z ) );

		Camera.Position = Vector3.Right * -middle.y + Vector3.Up * middle.z + Vector3.Backward * (40f + (size * 2f));
		Camera.FieldOfView = 23;
		Camera.ZNear = 5;
		Camera.ZFar = 15000;
		Camera.AmbientLightColor = Color.Gray * 0.5f;

		//We should be using the map fog but use this for now. - louie
		World.GradientFog.Enabled = true;
		World.GradientFog.Color = new Color( 0.03f, 0.11f, 0.19f );
		World.GradientFog.MaximumOpacity = 0.28f;
		World.GradientFog.StartHeight = 0;
		World.GradientFog.EndHeight = 2000;
		World.GradientFog.DistanceFalloffExponent = 2;
		World.GradientFog.VerticalFalloffExponent = 0;
		World.GradientFog.StartDistance = 500;
		World.GradientFog.EndDistance = 1000;

		//foreach ( var att in Weapon.Attachments.Select( x => WeaponAttachment.Get( x ) ).OrderBy( x => x.Priority ) )
		//{
		//	att?.SetupSceneModel( WeaponModel );
		//}
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		WeaponModel?.Delete();
		WeaponModel = null;

		World?.Delete();
		World = null;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( firstTime )
		{
			Update();
		}
	}

	protected float MouseWidthNormal;
	protected float MouseHeightNormal;
	public bool NoRotation { get; set; } = true;
	bool FirstUpdate = false;

	public bool FlipYaw { get; set; } = false;
	public float ViewYaw => FlipYaw ? -90 : 90;

	public override void Tick()
	{
		if ( WeaponModel is null ) return;
		if ( !IsVisible ) return;

		if ( (NoRotation && !FirstUpdate) || !NoRotation )
		{
			WeaponModel.Update( Time.Delta * TimeScale );
		}

		if ( NoRotation )
		{
			WeaponModel.Rotation = Rotation.From( 0, ViewYaw, 0 );
		}
		else
		{
			var mdW = MousePosition.x / Screen.Width;
			var mdH = MousePosition.y / Screen.Height;

			MouseWidthNormal = MouseWidthNormal.LerpTo( mdW, Time.Delta * 2f );
			MouseHeightNormal = MouseHeightNormal.LerpTo( mdH, Time.Delta * 2f );

			WeaponModel.Rotation = WeaponModel.Rotation.Angles()
				.WithYaw( ViewYaw + ((MouseWidthNormal - 0.5f) * 50f) )
				.WithRoll( 0 + ((MouseHeightNormal - 0.5f) * -10f) )
				.ToRotation();
		}
	}

	internal void SetModel( Model model )
	{
		try
		{
			if ( WeaponModel != null )
			{
				WeaponModel.Delete();
				WeaponModel = null;
			}

			WeaponModel = new SceneModel( World, model, Transform.Zero.WithRotation( Rotation.From( 0, 90, 0 ) ) );
			WeaponModel.Update( 0.1f );
		}
		catch ( Exception e )
		{
			Log.Warning( e );
		}
	}

	internal void SetModel( string modelName ) => SetModel( Model.Load( modelName ) );
}

