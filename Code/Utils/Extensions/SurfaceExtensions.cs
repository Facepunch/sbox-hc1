public static class SurfaceExtensions
{
	/// <summary>
	/// Gets the blunt impact prefab for the given surface.
	/// </summary>
	/// <param name="surface"></param>
	/// <returns></returns>
	public static GameObject GetBluntImpact( this Surface surface )
	{
		return surface.PrefabCollection.BluntImpact ?? surface.GetBaseSurface()?.PrefabCollection.BluntImpact;
	}

	/// <summary>
	/// Gets the blunt impact prefab for the given surface.
	/// </summary>
	/// <param name="surface"></param>
	/// <returns></returns>
	public static GameObject GetBulletImpact( this Surface surface )
	{
		return surface.PrefabCollection.BulletImpact ?? surface.GetBaseSurface()?.PrefabCollection.BulletImpact;
	}
}
