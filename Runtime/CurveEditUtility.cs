using System;

namespace UnityEngine.Timeline
{
    // Utility class for editing animation clips from serialized properties
    static class CurveEditUtility
    {
        // Creates an opposing blend curve that matches the given curve to make sure the result is normalized
        public static AnimationCurve CreateMatchingCurve(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;

            for (var i = 0; i != keys.Length; i++)
            {
                if (!Single.IsPositiveInfinity(keys[i].inTangent))
                    keys[i].inTangent = -keys[i].inTangent;
                if (!Single.IsPositiveInfinity(keys[i].outTangent))
                    keys[i].outTangent = -keys[i].outTangent;
                keys[i].value = 1.0f - keys[i].value;
            }
            return new AnimationCurve(keys);
        }
    }
}
