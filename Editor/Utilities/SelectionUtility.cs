using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Timeline
{
    internal static class SelectionUtility
    {
        public static Object IdToObject(int instanceId)
        {
#if UNITY_6000_3_OR_NEWER
            return EditorUtility.EntityIdToObject(instanceId);
#else
            return EditorUtility.InstanceIDToObject(instanceId);
#endif
        }

        public static IEnumerable<int> selectionIds
        {
            get
            {
#if UNITY_6000_3_OR_NEWER
                return Selection.entityIds.Select(id => (int)id);
#else
                return Selection.instanceIDs;
#endif
            }
        }
    }
}
