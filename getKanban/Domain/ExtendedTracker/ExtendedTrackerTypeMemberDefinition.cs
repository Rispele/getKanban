using System.Reflection;

namespace Domain.ExtendedTracker;

public class ExtendedTrackerTypeMemberDefinition
{
	private FieldInfo? Field { get; }
	private PropertyInfo? Property { get; }

	public string Name => Field?.Name ?? Property?.Name ?? string.Empty;

	public ExtendedTrackerTypeMemberDefinition(FieldInfo? field, PropertyInfo? property)
	{
		if (!((field == null) ^ (property == null)))
		{
			throw new ArgumentException("Field and property cannot be null or notnull together");
		}

		Field = field;
		Property = property;
	}

	public object? Value(object obj)
	{
		return Field?.GetValue(obj) ?? Property?.GetValue(obj);
	}
}