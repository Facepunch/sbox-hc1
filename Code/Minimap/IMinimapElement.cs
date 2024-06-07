namespace Facepunch;

public enum MinimapIconType
{
	None,
	DroppedC4,
	Buyzone,
	Player,
	PlayerDead,
	PlayerEnemy,
	PlayerEnemyMissing
}

public interface IMinimapElement : IValid
{
	public Vector3 WorldPosition { get; }

	public bool IsVisible( PlayerController viewer );
}

// Icons
public interface IMinimapIcon : IMinimapElement
{
	public MinimapIconType IconType { get; }
}

public interface ICustomMinimapIcon : IMinimapIcon
{
	public string CustomStyle { get; }
}

public interface IDirectionalMinimapIcon : ICustomMinimapIcon
{
	public bool EnableDirectional { get; }
	public Angles Direction { get; }
}

// Volumes
public interface IMinimapVolume : IMinimapElement
{
	public Color Color { get; }
	public Color LineColor { get; }
	public Vector3 Size { get; }
}

// Labels
public interface IMinimapLabel : IMinimapElement
{
	public string Label { get; }
	public Color LabelColor { get; }
}

public static class MinimapExtensionMethods
{
	public static string GetClass( this MinimapIconType type )
	{
		return type switch
		{
			MinimapIconType.DroppedC4 => "c4",
			MinimapIconType.Buyzone => "buyzone",
			MinimapIconType.Player => "",
			MinimapIconType.PlayerDead => "dead",
			MinimapIconType.PlayerEnemy => "enemy",
			MinimapIconType.PlayerEnemyMissing => "enemy-missing",
			_ => "",
		};
	}

}
