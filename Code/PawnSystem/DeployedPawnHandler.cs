using Sandbox.Diagnostics;

namespace Facepunch;

/// <summary>
/// A component that is placed on any created Pawn that we want to unpossess and depossess, like a drone.
/// </summary>
public partial class DeployedPawnHandler : Component
{
	[RequireComponent] 
	Client Client { get; set; }

	[Sync] 
	public Pawn Pawn { get; set; }

	public void PossessPlayerPawn()
	{
		Client?.PlayerPawn?.Possess();
	}

	public void PossessDeployedPawn()
	{
		Pawn?.Possess();
	}

	public static DeployedPawnHandler Create( Pawn pawn )
	{
		Assert.True( Networking.IsHost );

		var handler = pawn.Client.GetOrAddComponent<DeployedPawnHandler>();
		handler.Pawn = pawn;

		return handler;
	}

	protected override void OnFixedUpdate()
	{
		if ( Client.IsLocalPlayer )
		{
			if ( !Pawn.IsValid() )
				return;

			if ( Input.Pressed( "Use" ) )
			{
				if ( Client.Pawn == Pawn )
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
