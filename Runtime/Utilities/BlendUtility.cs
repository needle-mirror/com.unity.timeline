using System;

namespace UnityEngine.Timeline
{
    static class BlendUtility
    {
        static readonly double kMinOverlapTime = TimeUtility.kTimeEpsilon * 1000;

        static bool Overlaps(TimelineClip blendOut, TimelineClip blendIn)
        {
            if (blendIn == blendOut)
                return false;

            if (Math.Abs(blendIn.start - blendOut.start) < TimeUtility.kTimeEpsilon)
            {
                return blendIn.duration >= blendOut.duration;
            }

            return blendIn.start >= blendOut.start && blendIn.start < blendOut.end;
        }

        public static void ComputeBlendsFromOverlaps(TimelineClip[] clips)
        {
            foreach (var clip in clips)
            {
                clip.blendInDuration = -1;
                clip.blendOutDuration = -1;
            }

            Array.Sort(clips, (c1, c2) =>
                Math.Abs(c1.start - c2.start) < TimeUtility.kTimeEpsilon ? c1.duration.CompareTo(c2.duration) : c1.start.CompareTo(c2.start));

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (!clip.SupportsBlending())
                    continue;
                var blendIn = clip;
                TimelineClip blendOut = null;

                var blendOutCandidate = clips[Math.Max(i - 1, 0)];
                if (Overlaps(blendOutCandidate, blendIn))
                    blendOut = blendOutCandidate;

                if (blendOut != null)
                {
                    UpdateClipIntersection(blendOut, blendIn);
                }
            }
        }

        static void UpdateClipIntersection(TimelineClip blendOutClip, TimelineClip blendInClip)
        {
            if (!blendOutClip.SupportsBlending() || !blendInClip.SupportsBlending())
                return;

            if (blendInClip.start - blendOutClip.start < blendOutClip.duration - blendInClip.duration)
                return;

            double duration = Math.Max(0, blendOutClip.start + blendOutClip.duration - blendInClip.start);
            duration = duration <= kMinOverlapTime ? 0 : duration;
            blendOutClip.blendOutDuration = duration;
            blendInClip.blendInDuration = duration;

            var blendInMode = blendInClip.blendInCurveMode;
            var blendOutMode = blendOutClip.blendOutCurveMode;

            if (blendInMode == TimelineClip.BlendCurveMode.Manual && blendOutMode == TimelineClip.BlendCurveMode.Auto)
            {
                blendOutClip.mixOutCurve = CurveEditUtility.CreateMatchingCurve(blendInClip.mixInCurve);
            }
            else if (blendInMode == TimelineClip.BlendCurveMode.Auto && blendOutMode == TimelineClip.BlendCurveMode.Manual)
            {
                blendInClip.mixInCurve = CurveEditUtility.CreateMatchingCurve(blendOutClip.mixOutCurve);
            }
            else if (blendInMode == TimelineClip.BlendCurveMode.Auto && blendOutMode == TimelineClip.BlendCurveMode.Auto)
            {
                blendInClip.mixInCurve = null; // resets to default curves
                blendOutClip.mixOutCurve = null;
            }
        }
    }
}
