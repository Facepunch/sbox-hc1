using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Controls bot aiming at current target with configurable parameters
/// </summary>
public class AimAtTargetNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";
	private readonly float _aimSpeed;
	private readonly float _accuracy;
	private readonly float _maxInaccuracy;

	public AimAtTargetNode( float aimSpeed = 10f, float accuracy = 0.05f, float maxInaccuracy = 50f )
	{
		_aimSpeed = aimSpeed;
		_accuracy = accuracy;
		_maxInaccuracy = maxInaccuracy;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<PlayerPawn>( CURRENT_TARGET_KEY );
		if ( !target.IsValid() || target.HealthComponent.State != LifeState.Alive )
			return NodeResult.Failure;

		var pawn = context.Pawn;
		var targetPos = GetAimPosition( target );

		// Add inaccuracy based on bot's accuracy setting
		var inaccuracy = (1f - _accuracy) * _maxInaccuracy;
		targetPos += Vector3.Random * inaccuracy;

		var direction = (targetPos - pawn.EyePosition).Normal;
		var targetRotation = Rotation.LookAt( direction );

		// Smoothly interpolate to target rotation
		pawn.EyeAngles = pawn.EyeAngles.LerpTo( targetRotation, Time.Delta * _aimSpeed );

		return NodeResult.Success;
	}

	private Vector3 GetAimPosition( PlayerPawn target )
	{
		// Target upper chest/neck area by default
		// TODO: Target hitboxes
		return target.EyePosition - Vector3.Up * 32f;
	}
}
