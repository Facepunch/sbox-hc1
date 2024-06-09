namespace Facepunch;

public partial record struct DamageEvent
{
	/// <summary>
	/// Who took damage?
	/// </summary>
	public Component Victim { get; set; }

	/// <summary>
	/// Who was the attacker?
	/// </summary>
	public Component Attacker { get; set; }

	/// <summary>
	/// What caused this damage? Can be a weapon, grenade, etc.
	/// </summary>
	public Component Inflictor { get; set; }

	/// <summary>
	/// How much damage?
	/// </summary>
	public float Damage { get; set; }

	/// <summary>
	/// The point of the damage. Normally where you were hit.
	/// </summary>
	public Vector3 Position { get; set; }

	/// <summary>
	/// The force of the damage.
	/// </summary>
	public Vector3 Force { get; set; }

	/// <summary>
	/// What hitboxes did we hit?
	/// </summary>
	public string Hitboxes { get; set; }

	/// <summary>
	/// Extra data that we can pass around. Like if it's a blind-shot, mid-air shot, through smoke shot, etc.
	/// </summary>
	public string Tags { get; set; }

	/// <summary>
	/// Generate a damage event from a bunch of data.
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="damage"></param>
	/// <param name="inflictor"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="hitboxes"></param>
	/// <param name="tags"></param>
	/// <returns></returns>
	public static DamageEvent From(
		Component attacker,
		float damage,
		Component inflictor = null,
		Vector3 position = default,
		Vector3 force = default,
		string hitboxes = "",
		string tags = "" )
	{
		return new DamageEvent()
		{
			Attacker = attacker,
			Damage = damage,
			Inflictor = inflictor,
			Position = position,
			Force = force,
			Hitboxes = hitboxes,
			Tags = tags
		};
	}

	public override string ToString()
	{
		return $"\"{Attacker}\" - \"{Victim}\" with \"{Inflictor}\" ({Damage} damage)";
	}
}
