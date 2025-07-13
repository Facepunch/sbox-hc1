using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Controls weapon firing with configurable burst patterns
/// </summary>
public class ShootTargetNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";
	private readonly RangedFloat _burstRange;
	private readonly float _burstBreakTime;

	private int _burstCount;
	private int _targetBurstSize;
	private TimeSince _timeSinceLastShot;

	public ShootTargetNode( RangedFloat? burstRange = null, float burstBreakTime = 0.5f )
	{
		_burstRange = burstRange ?? new RangedFloat( 2, 5 );
		_burstBreakTime = burstBreakTime;
		_timeSinceLastShot = 0;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<PlayerPawn>( CURRENT_TARGET_KEY );
		if ( !target.IsValid() || target.HealthComponent.State != LifeState.Alive )
			return NodeResult.Failure;

		var pawn = context.Pawn;
		var weapon = pawn.CurrentEquipment;

		if ( !weapon.IsValid() || weapon.GetComponentInChildren<Shootable>() is not { } shootable )
			return NodeResult.Failure;

		// Check if target is visible
		var trace = pawn.Scene.Trace.Ray( pawn.EyePosition, target.EyePosition + Vector3.Down * 16f )
			.IgnoreGameObjectHierarchy( pawn.GameObject.Root )
			.Run();

		if ( !trace.Hit || trace.GameObject.Root != target.GameObject )
		{
			return NodeResult.Failure;
		}

		// Start new burst if needed
		if ( _burstCount == 0 && _timeSinceLastShot > _burstBreakTime )
		{
			_targetBurstSize = _burstRange.GetValue().CeilToInt();
			_burstCount = 0;
		}

		// Handle shooting
		if ( _burstCount < _targetBurstSize && shootable.CanShoot() )
		{
			// Get angle to target
			var toTarget = target.WorldPosition - pawn.WorldPosition;
			var dot = Vector3.Dot( toTarget.Normal, pawn.EyeAngles.Forward );

			// Only shoot if aim is reasonable
			if ( dot > 0.85f ) // ~30 degree cone
			{
				shootable.Shoot();
				_burstCount++;
				_timeSinceLastShot = 0;
			}
		}
		else if ( _burstCount >= _targetBurstSize )
		{
			_burstCount = 0;
			_targetBurstSize = 0;
		}

		return NodeResult.Running;
	}
}
