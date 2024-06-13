using Sandbox.Diagnostics;

namespace Facepunch;

public class PlayerState : Component
{
	/// <summary>
	/// The controller for this player
	/// </summary>
	[RequireComponent] public PlayerController Player { get; private set; }

	/// <summary>
	/// The pawn this player is currently in possession of (networked if it's networked)
	/// </summary>
	public IPawn CurrentPawn { get; private set; }
	[HostSync] private Guid pawnGuid { get; set; } = Guid.Empty;
	// todo: this should be an engine feature?

	/// <summary>
	/// The Player we're currently in the view of (clientside)
	/// </summary>
	public static PlayerState CurrentView { get; private set; }

	/// <summary>
	/// Are we in the view of this player (clientside)
	/// </summary>
	public bool IsViewer => CurrentView == this;

	/// <summary>
	/// Is this the local player for this client
	/// </summary>
	public bool IsLocalPlayer => !Player.IsProxy && !Player.IsBot;

	protected override void OnAwake()
	{
		CurrentPawn = Scene.Directory.FindComponentByGuid( pawnGuid ) as IPawn;
	}

	/// <summary>
	/// Called from client when we've taken possession of a pawn.
	/// </summary>
	public void NotifyPossessed( IPawn pawn )
	{
		Assert.True(!IsProxy);
		NotifyPossessed((pawn as Component).Id);
	}

	[Broadcast]
	private void NotifyPossessed( Guid guid )
	{
		if ( Networking.IsHost )
			pawnGuid = guid;

		// todo: this should be an engine feature?
		CurrentPawn = Scene.Directory.FindComponentByGuid( guid ) as IPawn;

		if ( IsViewer && IsProxy )
		{
			Possess();
		}
	}

	public void Possess()
	{
		CurrentView = this;

		Assert.True( Player.IsValid(), "PlayerState has no Player!" );

		if ( CurrentPawn is null || IsLocalPlayer )
		{
			// Local player - always assume the controller
			(Player as IPawn).Possess();
		}
		else
		{
			// A remote player is possessing this player (spectating)
			// So enter the latest known pawn this player has possessed
			CurrentPawn.Possess();
		}
	}
}
