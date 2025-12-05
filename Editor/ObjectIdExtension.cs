using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    static class ObjectIdExtension
    {
        public static Object IdToObject(this ObjectId id)
        {
#if UNITY_6000_3_OR_NEWER
            return EditorUtility.EntityIdToObject(id);
#else
            return EditorUtility.InstanceIDToObject(id);
#endif
        }

        public static List<ObjectId> GetNewSelection(ObjectId id,
            List<ObjectId> allInstanceIDs, List<ObjectId> selectedIDs, ObjectId lastClickedId, bool keepMultiSelection, bool useShiftAsActionKey, bool allowMultiSelection)
        {
#if UNITY_6000_3_OR_NEWER
            return InternalEditorUtility.GetNewSelection(id, allInstanceIDs.ConvertAll(i => (EntityId)i), selectedIDs.ConvertAll(i => (EntityId)i), lastClickedId, keepMultiSelection, useShiftAsActionKey, allowMultiSelection)
                .ConvertAll(i => (ObjectId)i);
#else
            return InternalEditorUtility.GetNewSelection(id, allInstanceIDs.ConvertAll(i => (int)i), selectedIDs.ConvertAll(i => (int)i), lastClickedId, keepMultiSelection, useShiftAsActionKey, allowMultiSelection)
               .ConvertAll(i => (ObjectId)i);
#endif
        }

        public static IEnumerable<ObjectId> selectedIds
        {
            get
            {
#if UNITY_6000_3_OR_NEWER
                return Selection.entityIds.Select(id => (ObjectId)id);
#else
                return Selection.instanceIDs.Select(id => (ObjectId)id);
#endif
            }
        }
    }
}
