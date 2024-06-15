namespace Facepunch;

/// <summary>
/// A crappy debug overlay system, using gameobjects and components to show overlays with lifetime.
/// </summary>
public static class DebugOverlay
{
	private static GameObject CreateObject( Vector3 start )
	{
		var scene = Game.ActiveScene;
		using var _ = scene.Push();

		var go = new GameObject();
		go.Transform.Position = start;
		return go;
	}

	public static DebugLine Line( Vector3 start, Vector3 end, Color color, float lifetime = 5 )
	{
		var go = CreateObject( start );
		var line = go.Components.Create<DebugLine>();
		line.StartPos = start;
		line.EndPos = end;
		line.Color = color;

		var dest = go.Components.Create<TimedDestroyComponent>();
		dest.Time = lifetime;

		return line;
	}

	public static DebugSphere Sphere( Vector3 start, float radius, Color color, float lifetime = 5 )
	{
		var scene = Game.ActiveScene;
		using var _ = scene.Push();

		var go = new GameObject();
		go.Transform.Position = start;
		var sphere = go.Components.Create<DebugSphere>();
		sphere.Size = radius;
		sphere.Color = color;

		var dest = go.Components.Create<TimedDestroyComponent>();
		dest.Time = lifetime;

		return sphere;
	}
}
