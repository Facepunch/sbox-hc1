namespace Facepunch;

/// <summary>
/// This is spawned clientside when a ping happens.
/// </summary>
public partial class WorldPingComponent : Component, IMarkerObject, ICustomMinimapIcon
{
	/// <summary>
	/// Who's the owner?
	/// </summary>
	public PlayerState Owner { get; set; }

	private Vector3 MarkerPosition
	{
		get
		{
			if ( Receiver.IsValid() )
				return Receiver.Position;

			return Transform.Position + Vector3.Up * 32f;
		}
	}

	private string MarkerIcon
	{
		get
		{
			if ( Receiver.IsValid() && !string.IsNullOrEmpty( Receiver.Icon ) )
				return Receiver.Icon;

			return "ui/minimaps/player_icon.png";
		}
	}

	private string DisplayText
	{
		get
		{
			if ( Receiver.IsValid() && Receiver.Text != null )
				return Receiver.Text;

			return Owner?.Network.OwnerConnection?.DisplayName ?? "";
		}
	}

	private Color MarkerColor
	{
		get
		{
			if ( Receiver.IsValid() && Receiver.Color.HasValue )
			{
				return Receiver.Color.Value;
			}

			return Color.White;
		}
	}

	// IMarkerObject
	string IMarkerObject.MarkerIcon => MarkerIcon;
	string IMarkerObject.DisplayText => DisplayText;
	Vector3 IMarkerObject.MarkerPosition => MarkerPosition;
	string IMarkerObject.MarkerStyles => $"background-tint:{MarkerColor};";
	int IMarkerObject.IconSize => Receiver.IsValid() ? 16 : 32;

	// IMinimapIcon
	string IMinimapIcon.IconPath => MarkerIcon;
	Vector3 IMinimapElement.WorldPosition => MarkerPosition;

	// ICustomMinimapIcon
	string ICustomMinimapIcon.CustomStyle => $"background-tint:{MarkerColor}";

	/// <summary>
	/// 
	/// </summary>
	public IPingReceiver Receiver { get; private set; }

	/// <summary>
	/// Are we pinging a world object that might move?
	/// </summary>
	public Component Target
	{
		set
		{
			if ( !value.IsValid() )
				return;
			
			var receiver = (value as IPingReceiver) ?? value.GameObject.Root.Components.Get<IPingReceiver>();
			if ( receiver.IsValid() )
			{
				Receiver = receiver;
			}
		}
	}

	// IMinimapElement
	bool IMinimapElement.IsVisible( Pawn viewer ) => true;

	/// <summary>
	/// Triggers the ping to tear off in X seconds.
	/// </summary>
	/// <param name="lifetime"></param>
	public void Trigger( float lifetime = 15f )
	{
		var destroy = Components.Create<TimedDestroyComponent>();
		destroy.Time = lifetime;

		Receiver?.OnPing();
	}

	protected override void OnUpdate()
	{
		if ( !Receiver.IsValid() )
			return;

		if ( !Receiver.ShouldShow() )
		{
			GameObject.Destroy();
		}
	}
}
