using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Checks and handles weapon reloading for bots
/// </summary>
public class ReloadWeaponNode : BaseBehaviorNode
{
	private readonly bool _waitForReload;

	public ReloadWeaponNode( bool waitForReload = false )
	{
		_waitForReload = waitForReload;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		var weapon = context.Pawn.CurrentEquipment;
		if ( !weapon.IsValid() )
			return NodeResult.Failure;

		var reloadable = weapon.GetComponentInChildren<Reloadable>();
		if ( reloadable == null )
			return NodeResult.Success; // Not a reloadable weapon, that's fine

		// Already reloading
		if ( reloadable.IsReloading )
		{
			return _waitForReload
				? NodeResult.Running
				: NodeResult.Success;
		}

		// Need to reload
		if ( !reloadable.AmmoComponent.HasAmmo )
		{
			reloadable.StartReload();

			// If we want to wait for reload to complete
			if ( _waitForReload )
			{
				while ( !token.IsCancellationRequested && reloadable.IsReloading )
				{
					await context.Task.FixedUpdate();
				}
			}
		}

		// No need to reload.
		return NodeResult.Success;
	}
}
