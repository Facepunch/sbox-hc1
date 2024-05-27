namespace Facepunch;

/// <summary>
/// A utility component that lets you have tags update based on conditions,
/// similarly to how we have Panel.BindClass
/// </summary>
public partial class TagBinder : Component
{
	/// <summary>
	/// A cache of binds.
	/// </summary>
	private Dictionary<string, Func<bool>> binds = new();

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		foreach ( var kv in binds )
		{
			GameObject.Tags.Set( kv.Key, kv.Value() );
		}
	}

	/// <summary>
	/// Do we have a bind already?
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	private bool HasExistingBind( string tag )
	{
		return binds.ContainsKey( tag );
	}

	/// <summary>
	/// Bind a tag to a specific condition. Must not exist.
	/// </summary>
	/// <param name="tag"></param>
	/// <param name="predicate"></param>
	/// <returns></returns>
	public bool BindTag( string tag, Func<bool> predicate )
	{
		if ( HasExistingBind( tag ) )
		{
			return false;
		}

		binds.Add( tag, predicate );

		return true;
	}
}
