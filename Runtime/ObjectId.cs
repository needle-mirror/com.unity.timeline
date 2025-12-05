using System;

namespace UnityEngine.Timeline
{
    [Serializable]
    struct ObjectId : IEquatable<ObjectId>, IComparable<ObjectId>
    {
        public static readonly ObjectId InvalidId = new ObjectId(-1);
        public static readonly ObjectId DefaultId = new ObjectId(0);

        [SerializeField]
        private int m_Data;

        internal ObjectId(int data) => m_Data = data;

#if UNITY_6000_3_OR_NEWER
        public static implicit operator ObjectId(EntityId entityId)
        {
            return new ObjectId() { m_Data = entityId };
        }

        public static implicit operator EntityId(ObjectId objectId) => objectId.m_Data;
#else
        public static implicit operator ObjectId(int instanceId)
        {
            return new ObjectId() { m_Data = instanceId };
        }

        public static implicit operator int(ObjectId objectId) => objectId.m_Data;
#endif
        public override bool Equals(object obj) => obj is ObjectId other && Equals(other);

        public bool Equals(ObjectId other) => m_Data == other.m_Data;

        public int CompareTo(ObjectId other) => this.m_Data.CompareTo(other.m_Data);

        public static bool operator ==(ObjectId left, ObjectId right) => left.Equals(right);

        public static bool operator !=(ObjectId left, ObjectId right) => !left.Equals(right);

        public static bool operator <(ObjectId left, ObjectId right) => left.m_Data < right.m_Data;

        public static bool operator >(ObjectId left, ObjectId right) => left.m_Data > right.m_Data;

        public static bool operator <=(ObjectId left, ObjectId right) => left.m_Data <= right.m_Data;

        public static bool operator >=(ObjectId left, ObjectId right) => left.m_Data >= right.m_Data;

        public override int GetHashCode()
        {
            return m_Data;
        }
    }
}
