
using Facepunch;

public record BombPlantedEvent( PlayerController Planter, GameObject Bomb, BombSite BombSite );

public interface IBombDefusedListener
{
	public void OnBombDefused( PlayerController defuser, GameObject bomb, BombSite bombSite ) { }
}

public interface IBombDetonatedListener
{
	public void OnBombDetonated( GameObject bomb, BombSite bombSite ) { }
}

public interface IBombDroppedListener
{
	public void OnBombDropped() { }
	public void OnBombPickedUp() { }
}
