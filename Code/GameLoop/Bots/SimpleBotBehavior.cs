using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

public partial class SimpleBotBehavior : Component
{
	public Client Client => Pawn.Client;
	public Pawn Pawn => GetComponentInParent<Pawn>();
	public PlayerPawn Player => Pawn as PlayerPawn;
	public BotPlayerController BotController => GetComponentInParent<BotPlayerController>( true );
	public NavMeshAgent MeshAgent => BotController?.MeshAgent;

	private void SyncNavAgentWithPhysics()
	{
		MeshAgent.MaxSpeed = Player.GetWishSpeed();
		Player.WishVelocity = MeshAgent.WishVelocity;

		// Handle desync between agent and physics
		if ( WorldPosition.WithZ( 0 ).DistanceSquared( MeshAgent.AgentPosition.WithZ( 0 ) ) > MeshAgent.Radius * MeshAgent.Radius )
		{
			MeshAgent.SetAgentPosition( WorldPosition );
		}
		if ( MathF.Abs( WorldPosition.z - MeshAgent.AgentPosition.z ) > MeshAgent.Height * MeshAgent.Height )
		{
			MeshAgent.SetAgentPosition( WorldPosition );
		}
	}

	/// <summary>
	/// Executes tasks in sequence until one returns true.
	/// </summary>
	private async Task<bool> RunSelector( CancellationToken token, params Func<CancellationToken, Task<bool>>[] tasks )
	{
		foreach ( var task in tasks )
		{
			if ( token.IsCancellationRequested )
				return false;

			if ( await task( token ) )
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Executes tasks in parallel with different cancellation/success modes.
	/// </summary>
	private async Task<bool> RunParallel( CancellationToken externalToken, params Func<CancellationToken, Task<bool>>[] tasks )
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource( externalToken );
		var token = cts.Token;

		var taskList = tasks.Select( task => task( token ) ).ToList();

		// Await the first task to complete
		var firstCompletedTask = await Task.WhenAny( taskList );

		// Cancel the other tasks
		cts.Cancel();

		try
		{
			// Get the result of the first completed task
			bool result = await firstCompletedTask;

			// This ensures that all task exceptions are observed
			await Task.WhenAll( taskList );

			return result;
		}
		catch ( OperationCanceledException )
		{
			// Handle task cancellation
			return firstCompletedTask.Result;
		}
		catch ( Exception )
		{
			// Handle any exceptions from the tasks
			return firstCompletedTask.Result;
		}
	}

	private async void RunBehaviour()
	{
		if ( MeshAgent == null || IsProxy )
			return;

		while ( Scene.NavMesh.IsGenerating )
		{
			await Task.DelaySeconds( 1 );
		}

		while ( this.IsValid() && Enabled )
		{
			try
			{
				using var timeoutCts = new CancellationTokenSource( TimeSpan.FromSeconds( 30 ) );
				using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
					GameObject.EnabledToken,
					timeoutCts.Token
				);

				await RunSelector(
					combinedCts.Token,
					Combat,
					Roam
				);

				await Task.DelayRealtimeSeconds( 0.1f );
			}
			catch ( OperationCanceledException )
			{
				Log.Info( "Bot behavior cancelled or timed out" );
			}
			catch ( Exception ex )
			{
				Log.Error( $"Error in bot behavior: {ex}" );
			}
		}
	}

	bool HasTicked = false;

	internal void Tick()
	{
		SyncNavAgentWithPhysics();
		FindEnemies();

		// TODO: this fucking sucks
		if ( !HasTicked )
		{
			RunBehaviour();
			HasTicked = true;
		}
	}
}
