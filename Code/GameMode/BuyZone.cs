﻿
namespace Facepunch;

internal class BuyZone : Component, IMinimapIcon, IMinimapVolume
{
	[Property]
	public TeamDefinition Team { get; set; }

	public Color Color => Team.GetColor().WithAlpha( 0.1f );
	public Color LineColor => Team.GetColor().WithAlpha( 0.5f );

	public Vector3 Size => Components.Get<BoxCollider>().Scale;

	string IMinimapIcon.IconPath => Team.GetIconPath();
	Angles IMinimapVolume.Angles => GameObject.Transform.Rotation.Angles();

	int IMinimapIcon.IconOrder => 15;

	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( Pawn viewer ) => true;
}
