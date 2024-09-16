
namespace Facepunch;

public static class CollectionExtensions
{
	public static TValue GetValueOrDefault<TKey, TValue>( this NetDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default )
	{
		return dict.TryGetValue( key, out var value ) ? value : defaultValue;
	}
}

