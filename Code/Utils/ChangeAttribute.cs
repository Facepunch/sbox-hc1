namespace Facepunch;

[CodeGenerator( CodeGeneratorFlags.Instance | CodeGeneratorFlags.WrapPropertySet, "Facepunch.ChangeAttribute.OnPropertySet" )]
public class ChangeAttribute : Attribute
{
	public string Callback { get; set; }

	public ChangeAttribute( string callbackName )
	{
		Callback = callbackName;
	}
	
	internal static void OnPropertySet<T>( WrappedPropertySet<T> p )
	{
		var attribute = p.Attributes.OfType<ChangeAttribute>().FirstOrDefault();
		var type = TypeLibrary.GetType( p.TypeName );
		var method = type.GetMethod( attribute.Callback );
		var property = TypeLibrary.GetMemberByIdent( p.MemberIdent ) as PropertyDescription;
		var oldValue = property.GetValue( p.Object );
		var isTheSame = p.Value.Equals( oldValue );
		
		p.Setter( p.Value );

		if ( isTheSame )
			return;
		
		try
		{
			method.Invoke( p.Object, new[] { oldValue, p.Value } );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
}
