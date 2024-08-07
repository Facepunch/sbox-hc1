namespace Facepunch;

public interface IMinimapElement : IValid
{
	public Vector3 WorldPosition { get; }
	public bool IsVisible( Pawn viewer );
}

// Icons
public interface IMinimapIcon : IMinimapElement
{
	public string IconPath { get; }
	public int IconOrder => 22;
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
	public Angles Angles { get; }
}

// Labels
public interface IMinimapLabel : IMinimapElement
{
	public string Label { get; }
	public Color LabelColor { get; }
}
