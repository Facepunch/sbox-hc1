namespace Facepunch;

public class ScreenShaker : Component
{
	/// <summary>
	/// Get any <see cref="ScreenShaker"/> component on the main camera.
	/// </summary>
	public static ScreenShaker Main
	{
		get
		{
			if ( !Game.ActiveScene.Camera.IsValid() )
				return null;

			return Game.ActiveScene.Camera.GetComponent<ScreenShaker>();
		}
	}
	
	private readonly List<ScreenShake> List = new();

	/// <summary>
	/// Apply any screen shake effects to the specified camera.
	/// </summary>
	/// <param name="camera"></param>
	public void Apply( CameraComponent camera )
	{
		for ( var i = List.Count; i > 0; i-- )
		{
			var entry = List[i - 1];
			var keep = entry.Update( camera );
			if ( keep ) continue;
			List.RemoveAt( i - 1 );
		}
	}
	
	/// <summary>
	/// Add a new screen shake effect to the list.
	/// </summary>
	public void Add( ScreenShake shake )
	{
		List.Add( shake );
	}
}
