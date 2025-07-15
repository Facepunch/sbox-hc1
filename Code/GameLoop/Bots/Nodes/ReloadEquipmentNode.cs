namespace Facepunch;

/// <summary>
/// Checks and handles weapon reloading for bots
/// </summary>
public class ReloadWeaponNode : BaseBehaviorNode
{
	private readonly bool _waitForReload;
	private bool _waiting;

	public ReloadWeaponNode( bool waitForReload = false )
	{
		_waitForReload = waitForReload;
	}

	protected override NodeResult OnEvaluate( BotContext context )
	{
		var weapon = context.Pawn.CurrentEquipment;
		if ( !weapon.IsValid() )
			return NodeResult.Failure;

		var reloadable = weapon.GetComponentInChildren<Reloadable>();
		if ( reloadable == null )
			return NodeResult.Success; // weapon doesn’t need reload

		// If currently reloading
		if ( reloadable.IsReloading )
		{
			if ( _waitForReload )
			{
				// stay running until reload completes
				return NodeResult.Running;
			}
			return NodeResult.Success;
		}

		// If ammo is out, trigger reload
		if ( !reloadable.AmmoComponent.HasAmmo )
		{
			reloadable.StartReload();

			if ( _waitForReload )
			{
				// on next ticks we will sit in the above "IsReloading" path
				_waiting = true;
				return NodeResult.Running;
			}
		}

		// Otherwise we are fine
		_waiting = false;
		return NodeResult.Success;
	}
}
