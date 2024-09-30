using Sandbox.Diagnostics;

namespace Facepunch;

public partial class DeployedPawnHandler : Component
{
	[RequireComponent] PlayerState PlayerState { get; set; }
	[Sync] public Pawn Pawn { get; set; }

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

		var handler = pawn.PlayerState.GetOrAddComponent<DeployedPawnHandler>();
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
