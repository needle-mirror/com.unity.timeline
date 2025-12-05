namespace UnityEngine.Timeline
{
    static class SelectionExtensions
    {
        public static ObjectId GetObjectId(this Object obj)
        {
#if UNITY_6000_3_OR_NEWER
            return obj.GetEntityId();
#else
            return obj.GetInstanceID();
#endif
        }
    }
}
