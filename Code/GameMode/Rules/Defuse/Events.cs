using Sandbox.Events;

namespace Facepunch;

public record BombPlantedEvent( PlayerPawn Planter, GameObject Bomb, BombSite BombSite ) : IGameEvent;
public record BombDefusedEvent( PlayerPawn Defuser, GameObject Bomb, BombSite BombSite ) : IGameEvent;
public record BombDetonatedEvent( GameObject Bomb, BombSite BombSite ) : IGameEvent;
public record BombDroppedEvent : IGameEvent;
public record BombPickedUpEvent : IGameEvent;

[Title( "Bomb Planted Event" )]
public class BombPlantedEventComponent : GameEventComponent<BombPlantedEvent> { }

[Title( "Bomb Defused Event" )]
public class BombDefusedEventComponent : GameEventComponent<BombDefusedEvent> { }

[Title( "Bomb Detonated Event" )]
public class BombDetonatedEventComponent : GameEventComponent<BombDetonatedEvent> { }
