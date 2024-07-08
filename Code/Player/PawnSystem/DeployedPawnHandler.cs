using Sandbox.Diagnostics;
using System.Text.Json.Serialization;

namespace Facepunch;

public partial class DeployedPawnHandler : Component
{
	[RequireComponent] PlayerState PlayerState { get; set; }

	[Property] public Pawn Pawn
	{
		get => Scene?.Directory.FindComponentByGuid( pawnGuid ) as Pawn;
		private set => pawnGuid = value.Id;
	}

	[Sync, JsonIgnore] private Guid pawnGuid { get; set; } = Guid.Empty;

	public void PossessPlayerPawn()
	{
		PlayerState?.PlayerPawn?.Possess();
	}

	public void PossessDeployedPawn()
	{
		Pawn?.Possess();
	}

	public static DeployedPawnHandler Create( Pawn pawn )
	{
		Assert.True( Networking.IsHost );

		var handler = pawn.PlayerState.Components.GetOrCreate<DeployedPawnHandler>();
		handler.Pawn = pawn;

		return handler;
	}

	protected override void OnFixedUpdate()
	{
		if ( PlayerState.IsLocalPlayer )
		{
			if ( !Pawn.IsValid() )
				return;

			if ( Input.Pressed( "Use" ) )
			{
				if ( PlayerState.Pawn == Pawn )
				{
					PossessPlayerPawn();
				}
				else
				{
					PossessDeployedPawn();
				}
			}
		}
	}
}
