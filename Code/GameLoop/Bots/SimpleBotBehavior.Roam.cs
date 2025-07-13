using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

public partial class SimpleBotBehavior
{
	private async Task<bool> Roam( CancellationToken token )
	{
		if ( FindAndUpdateTarget() != null )
			return false;

		while ( !token.IsCancellationRequested )
		{
			bool result = await RunParallel(
				token,
				CheckForTarget,
				UpdateRotationToMatchMovement,
				token => RunSelector( token, GoSomewhereRandom )
			);

			if ( !result )
				return true;
		}

		return false;
	}

	private async Task<bool> GoSomewhereRandom( CancellationToken token )
	{
		var randomPoint = Scene.NavMesh.GetRandomPoint();
		if ( !randomPoint.HasValue )
			return false;

		TimeSince timeSinceMoveStart = 0;
		const float MoveDuration = 10f;

		MeshAgent.MoveTo( randomPoint.Value );

		while ( timeSinceMoveStart < MoveDuration && !token.IsCancellationRequested )
		{
			MeshAgent.MoveTo( randomPoint.Value );
			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	private async Task<bool> CheckForTarget( CancellationToken token )
	{
		while ( !token.IsCancellationRequested )
		{
			_currentTarget = FindAndUpdateTarget();

			if ( _currentTarget.IsValid() )
			{
				return false;
			}

			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}

	private async Task<bool> UpdateRotationToMatchMovement( CancellationToken token )
	{
		while ( !token.IsCancellationRequested )
		{
			var aimPos = Player.AimRay.Position;
			var currentAimTarget = MeshAgent.GetLookAhead( 30.0f ).WithZ( aimPos.z );
			var dir = currentAimTarget - aimPos;

			if ( dir.LengthSquared > 0.01f * 0.01f )
			{
				var rotation = Rotation.LookAt( dir );
				Pawn.EyeAngles = Pawn.EyeAngles.LerpTo( rotation, 0.1f );
			}

			await Task.FixedUpdate();
		}

		return !token.IsCancellationRequested;
	}
}
