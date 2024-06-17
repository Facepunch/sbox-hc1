
public abstract class SingletonComponent<T> : Component, IHotloadManaged
	where T : SingletonComponent<T>
{
	public static T Instance { get; private set; }

	protected override void OnAwake()
	{
		if ( Active )
		{
			Instance = (T)this;
		}
	}

	void IHotloadManaged.Destroyed( Dictionary<string, object> state )
	{
		state["IsActive"] = Instance == this;
	}

	void IHotloadManaged.Created( IReadOnlyDictionary<string, object> state )
	{
		if ( state.GetValueOrDefault( "IsActive" ) is true )
		{
			Instance = (T) this;
		}
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
		{
			Instance = null;
		}
	}
}
