using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public partial class TimelinePlayable
    {
        readonly Dictionary<AnimationTrack, List<ITimelineEvaluateCallback>> m_EvaluateCallbacks = new Dictionary<AnimationTrack, List<ITimelineEvaluateCallback>>();
        readonly List<ITimelineEvaluateCallback> m_AlwaysEvaluateCallbacks = new List<ITimelineEvaluateCallback>();
        readonly HashSet<ITimelineEvaluateCallback> m_ForceEvaluateNextEvaluate = new HashSet<ITimelineEvaluateCallback>();
        readonly HashSet<ITimelineEvaluateCallback> m_InvokedThisFrame = new HashSet<ITimelineEvaluateCallback>();
        readonly HashSet<AnimationTrack> m_ActiveTracksToEvaluateCache = new HashSet<AnimationTrack>();

        readonly struct TrackCacheManager : IDisposable
        {
            public readonly HashSet<AnimationTrack> trackCache;

            public TrackCacheManager(HashSet<AnimationTrack> cache, IReadOnlyList<RuntimeElement> activeRuntimeElements)
            {
                trackCache = cache;
                GetTrackAssetsFromRuntimeElements(activeRuntimeElements);
            }

            public void Dispose()
            {
                trackCache.Clear();
            }

            void GetTrackAssetsFromRuntimeElements(IReadOnlyList<RuntimeElement> activeRuntimeElements)
            {
                for (int index = 0; index < activeRuntimeElements.Count; index++)
                {
                    if (activeRuntimeElements[index] is RuntimeClip rc)
                    {
                        if (rc.clip?.GetParentTrack() is AnimationTrack asset)
                        {
                            trackCache.Add(asset);
                        }
                    }
                }
            }
        }

        void AddPlayableOutputCallbacks(AnimationTrack track, PlayableOutput playableOutput)
        {
            AddOutputWeightProcessor(track, (AnimationPlayableOutput)playableOutput);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AddPreviewUpdateCallback(track, (AnimationPlayableOutput)playableOutput);
#endif
        }

        void AddOutputWeightProcessor(AnimationTrack track, AnimationPlayableOutput animOutput)
        {
            var processor = new AnimationOutputWeightProcessor(animOutput);
            if (track.inClipMode)
                AddEvaluateCallback(track, processor);
            else
                m_AlwaysEvaluateCallbacks.Add(processor);

            m_ForceEvaluateNextEvaluate.Add(processor);
        }

#if UNITY_EDITOR
        void AddPreviewUpdateCallback(AnimationTrack track, AnimationPlayableOutput animOutput)
        {
            var callback = new AnimationPreviewUpdateCallback(animOutput);
            if (track.inClipMode)
                AddEvaluateCallback(track, callback);
            else
                m_AlwaysEvaluateCallbacks.Add(callback);

            m_ForceEvaluateNextEvaluate.Add(callback);
        }
#endif

        void AddEvaluateCallback(AnimationTrack track, ITimelineEvaluateCallback callback)
        {
            if (m_EvaluateCallbacks.TryGetValue(track, out var list))
            {
                list.Add(callback);
            }
            else
            {
                m_EvaluateCallbacks[track] = new List<ITimelineEvaluateCallback> { callback };
            }
        }

        void InvokeOutputCallbacks(IReadOnlyList<RuntimeElement> activeRuntimeElements)
        {
            foreach (var callback in m_ForceEvaluateNextEvaluate)
            {
                callback.Evaluate();
                m_InvokedThisFrame.Add(callback);
            }

            m_ForceEvaluateNextEvaluate.Clear();

            if (activeRuntimeElements.Count > 0)
            {
                using (var activeTracksCache = new TrackCacheManager(m_ActiveTracksToEvaluateCache, activeRuntimeElements))
                {
                    foreach (AnimationTrack asset in activeTracksCache.trackCache)
                    {
                        if (TryGetCallbackList(asset, out var callbacks))
                        {
                            foreach (ITimelineEvaluateCallback callback in callbacks)
                            {
                                if (m_InvokedThisFrame.Contains(callback)) // prevent double invocation
                                    continue;

                                callback.Evaluate();
                                m_InvokedThisFrame.Add(callback);
                                m_ForceEvaluateNextEvaluate.Add(callback);
                            }
                        }
                    }
                }
            }
            else // evaluate all callbacks if there are no active clips
            {
                foreach (List<ITimelineEvaluateCallback> callbacks in m_EvaluateCallbacks.Values)
                {
                    foreach (ITimelineEvaluateCallback callback in callbacks)
                    {
                        if (m_InvokedThisFrame.Contains(callback)) // prevent double invocation
                            continue;

                        callback.Evaluate();
                    }
                }
            }

            foreach (var callback in m_AlwaysEvaluateCallbacks)
            {
                callback.Evaluate();
            }

            m_InvokedThisFrame.Clear();
        }

        bool TryGetCallbackList(AnimationTrack track, out List<ITimelineEvaluateCallback> list)
        {
            if (track == null)
            {
                list = null;
                return false;
            }

            if (m_EvaluateCallbacks.TryGetValue(track, out list))
                return true;

            return TryGetCallbackList(track.parent as AnimationTrack, out list);
        }
    }
}
