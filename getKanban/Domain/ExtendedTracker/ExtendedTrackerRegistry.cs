using System.Reflection;
using Domain.Attributes;

namespace Domain.ExtendedTracker;

public class ExtendedTrackerRegistry
{
	private readonly Assembly assembly;

	private readonly Dictionary<Type, ExtendedTrackerTypeDefinition> extendedTrackerTypeDefinitions;

	public ExtendedTrackerRegistry(Assembly assembly)
	{
		this.assembly = assembly;

		extendedTrackerTypeDefinitions = assembly
			.GetTypes()
			.Select(BuildTypeDefinition)
			.Where(definition => definition.Bindings.Length > 0)
			.ToDictionary(definition => definition.Type);
	}

	public bool TryGetTrackedMemberName(object entity, out IEnumerable<string>? memberNames)
	{
		memberNames = null;

		var entityType = entity.GetType();
		if (!extendedTrackerTypeDefinitions.TryGetValue(entityType, out var extendedTrackerTypeDefinition))
		{
			return false;
		}

		memberNames = extendedTrackerTypeDefinition.Bindings
			.Where(binding => binding.IsUpdated(entity))
			.Select(binding => binding.TrackingName);

		return true;
	}

	private static ExtendedTrackerTypeDefinition BuildTypeDefinition(Type type)
	{
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		var trackers = ExtractTrackObjects<TrackerAttribute>(properties, fields)
			.ToDictionary(t => t.bindingTag, t => t.memberDefinition);

		var bindings = ExtractTrackObjects<TrackingAttribute>(properties, fields)
			.Where(t => trackers.ContainsKey(t.bindingTag))
			.Select(t => new ExtendedTrackerTrackBinding(trackers[t.bindingTag], t.memberDefinition))
			.ToArray();

		return new ExtendedTrackerTypeDefinition(type, bindings);
	}

	private static IEnumerable<(ExtendedTrackerTypeMemberDefinition memberDefinition, string bindingTag)>
		ExtractTrackObjects<TAttribute>(PropertyInfo[] properties, FieldInfo[] fields)
		where TAttribute : TrackAttribute
	{
		var propertyObjects = ExtractTrackObjects<PropertyInfo, TAttribute>(properties);
		var fieldObjects = ExtractTrackObjects<FieldInfo, TAttribute>(fields);

		return propertyObjects
			.Select(p => (new ExtendedTrackerTypeMemberDefinition(null, p.member), p.bindingTag))
			.Concat(fieldObjects.Select(t => (new ExtendedTrackerTypeMemberDefinition(t.member, null), t.bindingTag)));
	}

	private static IEnumerable<(TMember member, string bindingTag)> ExtractTrackObjects<TMember, TAttribute>(
		TMember[] members)
		where TMember : MemberInfo
		where TAttribute : TrackAttribute
	{
		return members
			.Select(member => (member, tag: member.GetCustomAttribute<TAttribute>()?.TrackTag))
			.Where(t => t.tag != null)
			.Select(t => (tracker: t.member, t.tag!));
	}
}