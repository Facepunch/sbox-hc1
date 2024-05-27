namespace Facepunch
{
	public partial class PlayerBody : Component
	{
		[Property] public GameObject RightHand { get; set; }

		/// <summary>
		/// Move a weapon to the player's right hand
		/// </summary>
		/// <param name="weapon"></param>
		public void MoveWeapon( Weapon weapon )
		{
			weapon.GameObject.SetParent( RightHand, false );
		}
	}
}
