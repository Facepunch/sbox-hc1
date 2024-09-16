namespace Facepunch;

/// <summary>
/// For properties that contain an <see cref="IValid"/>, wrap the getter to return null if
/// the stored value isn't valid.
/// </summary>
[CodeGenerator( CodeGeneratorFlags.Instance | CodeGeneratorFlags.WrapPropertyGet, "Facepunch.ValidOrNullAttribute.OnPropertyGet", 20 )]
[AttributeUsage( AttributeTargets.Property )]
public sealed class ValidOrNullAttribute : Attribute
{
	internal static T OnPropertyGet<T>( WrappedPropertyGet<T> p )
	{
		return p.Value is IValid { IsValid: false } ? default : p.Value;
	}
}
