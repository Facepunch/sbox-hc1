namespace Gunfight;

[Title( "Surface" ), Icon( "do_not_step" )]
public sealed class SurfaceComponent : Component
{
	[Property] public Surface Surface { get; set; }
}

public static partial class GameObjectExtensions
{
	public static Surface GetSurface( this GameObject self )
	{
		// Try to find a surface component in either the current gameobject or its parent
		var surfaceComponent = self.Components.Get<SurfaceComponent>( FindMode.EverythingInSelfAndParent );

		if ( !surfaceComponent.IsValid() )
		{
			return null;
		}

		return surfaceComponent.Surface;
	}
}
