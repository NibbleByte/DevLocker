using UnityEngine;

namespace DevLocker.Tools
{
	public class InspectorCopyableAttribute : PropertyAttribute { }

	// Inherit from this class, so your class can be copyable in the inspector.
	// Sorry it cannot be done with interfaces, Unity doesn't pick that up. :(
	public abstract class InspectorCopyableBase { }
}
