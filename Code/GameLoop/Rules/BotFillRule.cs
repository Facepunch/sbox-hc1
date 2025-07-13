using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Maintains a minimum player count by filling empty slots with bots.
/// Bots are removed as real players join.
/// </summary>
public sealed class BotFillRule : Component,
	IGameEventHandler<PlayerJoinedEvent>,
	IGameEventHandler<PlayerDisconnectedEvent>,
	IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// Minimum number of players desired in the game
	/// </summary>
	[Property] public int MinPlayerCount { get; set; } = 10;

	/// <summary>
	/// Only add bots to this team, if specified
	/// </summary>
	[Property] public Team TargetTeam { get; set; }

	private List<Client> _addedBots = new();

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		// Initial player count check when entering state
		UpdatePlayerCount();
	}

	void IGameEventHandler<PlayerJoinedEvent>.OnGameEvent( PlayerJoinedEvent eventArgs )
	{
		if ( eventArgs.Player.IsBot )
			return;

		// Real player joined, potentially remove a bot
		RemoveBot();

		// Update in case we need more bots
		UpdatePlayerCount();
	}

	void IGameEventHandler<PlayerDisconnectedEvent>.OnGameEvent( PlayerDisconnectedEvent eventArgs )
	{
		// Don't react to bot disconnects - shouldn't happen anyway
		if ( eventArgs.Player.IsBot )
			return;

		// Real player left, check if we need to add bots
		UpdatePlayerCount();
	}

	private void UpdatePlayerCount()
	{
		if ( !Networking.IsHost )
			return;

		var realPlayerCount = GameUtils.AllPlayers.Count( x => !x.IsBot );
		var botsNeeded = MinPlayerCount - realPlayerCount;

		// Add bots if we're below minimum
		while ( botsNeeded > 0 )
		{
			var bot = AddBot();
			if ( bot is null )
				break;

			_addedBots.Add( bot );
			botsNeeded--;
		}
	}

	private Client AddBot()
	{
		var botManager = BotManager.Instance;
		if ( !botManager.IsValid() )
			return null;

		// Create bot
		botManager.AddBot();

		// Get the last added bot
		var bot = GameUtils.AllPlayers.LastOrDefault( x => x.IsBot );
		if ( bot is null )
			return null;

		// Assign team if specified
		if ( TargetTeam != Team.Unassigned )
		{
			bot.Team = TargetTeam;
		}

		return bot;
	}

	private void RemoveBot()
	{
		// Remove the last added bot
		var lastBot = _addedBots.LastOrDefault();
		if ( lastBot.IsValid() )
		{
			_addedBots.Remove( lastBot );
			lastBot.Kick( "Making room for player" );
		}
	}

	protected override void OnDisabled()
	{
		// Clean up any bots we added when disabled
		foreach ( var bot in _addedBots.ToArray() )
		{
			if ( bot.IsValid() )
			{
				bot.Kick( "Rule disabled" );
			}
		}
		_addedBots.Clear();
	}
}
