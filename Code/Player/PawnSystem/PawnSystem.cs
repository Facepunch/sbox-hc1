namespace Facepunch;

public interface IPawn : IValid
{
	/// <summary>
	/// What team does this pawn belong to?
	/// </summary>
	public Team Team => Team.Unassigned;

	/// <summary>
	/// An accessor for health component if we have one.
	/// </summary>
	public HealthComponent HealthComponent => GameObject.Components.Get<HealthComponent>();

	/// <summary>
	/// Are we possessing this pawn right now? (Clientside)
	/// </summary>
	public bool IsPossessed => Game.ActiveScene.GetSystem<PawnSystem>().Viewer == this;

	/// <summary>
	/// Is this pawn locally controlled by us?
	/// </summary>
	public bool IsLocallyControlled => IsPossessed && !IsProxy;

	/// <summary>
	/// What's our name?
	/// </summary>
	public string DisplayName { get; }

	/// <summary>
	/// What's the pawn's eye angles?
	/// </summary>
	public Angles EyeAngles { get; }

	/// <summary>
	/// The pawn's camera. Has to have one.
	/// </summary>
	public CameraComponent Camera { get; }

	/// <summary>
	/// Who's the owner?
	/// </summary>
	public ulong SteamId { get; set; }

	/// <summary>
	/// The pawn's gameobject
	/// </summary>
	public GameObject GameObject { get; }

	/// <summary>
	/// Do we have network rights over this pawn?
	/// </summary>
	public bool IsProxy { get; }

	/// <summary>
	/// Possess the pawn.
	/// </summary>
	public void Possess()
	{
		Game.ActiveScene.GetSystem<PawnSystem>()
			.Possess( this );
	}

	/// <summary>
	/// De possesses the pawn.
	/// </summary>
	public void DePossess()
	{
		Game.ActiveScene.GetSystem<PawnSystem>()
			.DePossess( this );
	}

	public void OnPossess();
	public void OnDePossess();
}

/// <summary>
/// The system that holds data about what pawn we're looking through the eyes of.
/// If we don't have network authority, we'll try to spectate that pawn.
/// </summary>
public partial class PawnSystem : GameObjectSystem
{
	public IPawn Viewer { get; private set; }

	/// <summary>
	/// Tries to possess a pawn. If you don't own it, spectate!
	/// </summary>
	public void Possess( IPawn pawn )
	{
		DePossess( Viewer );
		Viewer = pawn;
		pawn?.OnPossess();

		// Valid and we own it?
		if ( pawn.IsValid() && !pawn.IsProxy )
		{
			pawn.SteamId = Connection.Local.SteamId;
		}

		GameUtils.LocalPlayer.PlayerState.NotifyPossessed( pawn );
	}

	public void DePossess( IPawn pawn )
	{
		pawn?.OnDePossess();

		// Valid and we own it?
		if ( pawn.IsValid() && !pawn.IsProxy )
		{
			pawn.SteamId = 0;
		}

		Viewer = null;
	}

	public PawnSystem( Scene scene ) : base( scene )
	{
	}
}
