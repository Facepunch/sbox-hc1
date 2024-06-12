using Facepunch;

public record BombPlantedEvent( PlayerController Planter, GameObject Bomb, BombSite BombSite );
public record BombDefusedEvent( PlayerController Defuser, GameObject Bomb, BombSite BombSite );
public record BombDetonatedEvent( GameObject Bomb, BombSite BombSite );
public record BombDroppedEvent;
public record BombPickedUpEvent;
