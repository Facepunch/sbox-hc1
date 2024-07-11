using System.Text.Json.Serialization;

namespace Facepunch;

public abstract class Pawn : Component, IRespawnable
{
	private static Pawn Current { get; set; }

	/// <summary>
	/// The player state ID
	/// </summary>
	[HostSync] private PlayerState playerState { get; set; }

	/// <summary>
	/// This pawn's PlayerState
	/// </summary>
	[Property, JsonIgnore, ReadOnly, Group( "Data" )] public PlayerState PlayerState
	{
		get
		{
			if ( !playerState.IsValid() ) return PlayerState.Local;
			return playerState;
		}
		set => playerState = value;
	}

	/// <summary>
	/// The tags of the last spawn point of this pawn.
	/// </summary>
	[HostSync]
	public NetList<string> SpawnPointTags { get; private set; } = new();

	/// <summary>
	/// What team does this pawn belong to?
	/// </summary>
	public virtual Team Team
	{
		get => PlayerState.Team;
		set => PlayerState.Team = value;
	}

	public virtual string NameType { get; } = "Pawn";

	/// <summary>
	/// An accessor for health component if we have one.
	/// </summary>
	[Property] public virtual HealthComponent HealthComponent { get; set; }

	/// <summary>
	/// Are we possessing this pawn right now? (Clientside)
	/// </summary>
	public bool IsPossessed => Current == this;

	/// <summary>
	/// Is this pawn locally controlled by us?
	/// </summary>
	public virtual bool IsLocallyControlled => IsPossessed && !IsProxy;

	/// <summary>
	/// What's our name?
	/// </summary>
	public virtual string DisplayName { get; }

	/// <summary>
	/// What's the pawn's eye angles?
	/// </summary>
	public virtual Angles EyeAngles { get; set; }

	/// <summary>
	/// The pawn's camera. Has to have one.
	/// </summary>
	public virtual CameraComponent Camera { get; }

	/// <summary>
	/// Who's the owner?
	/// </summary>
	[Sync] public ulong SteamId { get; set; }

	/// <summary>
	/// Possess the pawn.
	/// </summary>
	public void Possess() => Possess(this);
	public static void Possess( Pawn pawn )
	{
		if ( pawn.IsPossessed )
			return;

		DePossess( Current );
		Current = pawn;
		pawn?.OnPossess();

		// Valid and we own it?
		if ( pawn.IsValid() && !pawn.IsProxy )
		{
			pawn.SteamId = Connection.Local.SteamId;
		}

		PlayerState.OnPossess( pawn );
	}

	/// <summary>
	/// De possesses the pawn.
	/// </summary>
	public void DePossess() => DePossess( this );
	public static void DePossess( Pawn pawn )
	{ 
		bool wasPossessed = pawn.IsValid() && pawn.IsPossessed;
		Current = null;

		if ( wasPossessed )
		{
			pawn?.OnDePossess();

			// Valid and we own it?
			if ( pawn.IsValid() && !pawn.IsProxy )
			{
				pawn.SteamId = 0;
			}
		}
	}

	public virtual void OnPossess() { }
	public virtual void OnDePossess() { }

	public virtual void OnRespawn() { }
	public virtual void OnKill( DamageInfo damageInfo ) { }
}
