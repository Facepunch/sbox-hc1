namespace Facepunch;

public partial class PlayerState : Component.INetworkListener
{
	RealTimeSince TimeSinceDisconnected { get; set; }
	float DisconnectCleanupTime => 60f;

	void INetworkListener.OnDisconnected( Connection channel )
	{
		if ( Connection == channel )
		{
			TimeSinceDisconnected = 0;
		}
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy ) return;
		if ( IsConnected ) return;

		if ( TimeSinceDisconnected > DisconnectCleanupTime )
		{
			GameObject.Destroy();
		}
	}
}
