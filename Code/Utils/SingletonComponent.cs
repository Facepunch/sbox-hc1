
public abstract class SingletonComponent<T> : Component
	where T : SingletonComponent<T>
{
	public static T Instance { get; private set; }

	protected override void OnAwake()
	{
		Instance = (T) this;
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
		{
			Instance = null;
		}
	}
}
