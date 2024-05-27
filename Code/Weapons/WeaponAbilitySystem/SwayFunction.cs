namespace Facepunch;

/// <summary>
/// Implements a very basic weapon sway mechanic, when aiming down the sights.
/// </summary>
public partial class SwayFunction : WeaponFunction
{
	internal Angles Current { get; private set; }

	private float SwayScale
	{
		get
		{
			var sc = 0.5f;
			var velLen = Weapon.PlayerController.CharacterController.Velocity.Length.Remap( 0, 600, 0, 1 );
			return sc + ( velLen * 15f );
		}
	}

	protected override void OnUpdate()
	{
		if ( Weapon.Tags.Has( "aiming" ) )
		{
			var horizontalSway = MathF.Sin( Time.Now * SwayScale ) * SwayScale * Time.Delta;
			var verticalSway = MathF.Cos( Time.Now + 0.5f * SwayScale ) * SwayScale * Time.Delta;
			var angles = new Angles( -verticalSway, horizontalSway, 0 );
			Current = angles;
		}

		Current = Current.LerpTo( Angles.Zero, Time.Delta * 10f );
	}
}
