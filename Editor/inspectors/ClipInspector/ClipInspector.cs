using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace UnityEditor.Timeline
{
    [CustomEditor(typeof(EditorClip)), CanEditMultipleObjects]
    class ClipInspector : Editor
    {
        internal static class Styles
        {
            public const string stylesheetPath = DirectorStyles.stylesheetsPath + "ClipInspector.uss";

            public static readonly GUIContent StartName = L10n.TextContent("Start", "The start time of the clip");
            public static readonly GUIContent DurationName = L10n.TextContent("Duration", "The length of the clip");
            public static readonly GUIContent EndName = L10n.TextContent("End", "The end time of the clip");
            public static readonly GUIContent EaseInDurationName = L10n.TextContent("Ease In Duration", "The length of the ease in");
            public static readonly GUIContent BlendInDurationName = L10n.TextContent("Blend In Duration", "The length of the blend in");
            public static readonly GUIContent EaseOutDurationName = L10n.TextContent("Ease Out Duration", "The length of the ease out");
            public static readonly GUIContent BlendOutDurationName = L10n.TextContent("Blend Out Duration", "The length of the blend out");
            public static readonly GUIContent ClipInName = L10n.TextContent("Clip In", "Start the clip at this local time");
            public static readonly GUIContent TimeScaleName = L10n.TextContent("Speed Multiplier", "Time scale of the playback speed");
            public static readonly GUIContent PreExtrapolateLabel = L10n.TextContent("Pre-Extrapolate", "Extrapolation used prior to the first clip");
            public static readonly GUIContent PostExtrapolateLabel = L10n.TextContent("Post-Extrapolate", "Extrapolation used after a clip ends");
            public static readonly GUIContent BlendInCurveName = L10n.TextContent("In", "Blend In Curve");
            public static readonly GUIContent BlendOutCurveName = L10n.TextContent("Out", "Blend Out Curve");
            public static readonly GUIContent PreviewTitle = L10n.TextContent("Curve Editor");
            public static readonly GUIContent ClipTimingTitle = L10n.TextContent("Clip Timing");
            public static readonly GUIContent AnimationExtrapolationTitle = L10n.TextContent("Animation Extrapolation");
            public static readonly GUIContent BlendCurvesTitle = L10n.TextContent("Blend Curves");
            public static readonly GUIContent GroupTimingTitle = L10n.TextContent("Multiple Clip Timing");
            public static readonly GUIContent MultipleClipsSelectedIncompatibleCapabilitiesWarning = L10n.TextContent("Multiple clips selected. Only common properties are shown.");
            public static readonly GUIContent MultipleSelectionTitle = L10n.TextContent("Timeline Clips");
            public static readonly GUIContent MultipleClipStartName = L10n.TextContent("Start", "The start time of the clip group");
            public static readonly GUIContent MultipleClipEndName = L10n.TextContent("End", "The end time of the clip group");
            public static readonly GUIContent TimelineClipFG = DirectorStyles.IconContent("TimelineClipFG");
            public static readonly GUIContent TimelineClipBG = DirectorStyles.IconContent("TimelineClipBG");
        }

        class EditorClipSelection : ICurvesOwnerInspectorWrapper
        {
            public EditorClip editorClip { get; }
            public TimelineClip clip => editorClip == null ? null : editorClip.clip;
            public SerializedObject serializedPlayableAsset { get; }
            public ICurvesOwner curvesOwner => clip;

            public int lastCurveVersion { get; set; }
            public double lastEvalTime { get; set; }

            public EditorClipSelection(EditorClip anEditorClip)
            {
                editorClip = anEditorClip;
                lastCurveVersion = -1;
                lastEvalTime = -1;

                var so = new SerializedObject(editorClip);
                SerializedProperty playableAssetProperty = so.FindProperty("m_Clip.m_Asset");
                if (playableAssetProperty != null)
                {
                    var asset = playableAssetProperty.objectReferenceValue as UnityEngine.Playables.PlayableAsset;
                    if (asset != null)
                        serializedPlayableAsset = new SerializedObject(asset);
                }
            }

            public double ToLocalTime(double time)
            {
                return clip?.ToLocalTime(time) ?? time;
            }
        }

        class CustomPropertiesElement : VisualElement
        {
            const string k_Name = "clip-inspector-custom-properties";
            const string k_ClassName = "clip-inspector-custom-properties";
            const string k_FoldoutClassName = "clip-inspector-custom-properties__foldout";
            const string k_InspectorElementClassName = "clip-inspector-custom-properties__inspector";

            public event Action OnPropertyChanged;

            static StyleSheet s_StyleSheet;
            readonly VisualElement m_Container;

            public CustomPropertiesElement(Editor editor, string title)
            {
                name = k_Name;

                if (s_StyleSheet == null)
                    s_StyleSheet = DirectorStyles.LoadStyleSheet(Styles.stylesheetPath);

                styleSheets.Add(s_StyleSheet);
                AddToClassList(k_ClassName);

                var foldout = new Foldout { text = title };
                foldout.AddToClassList(k_FoldoutClassName);
                Add(foldout);

                m_Container = new VisualElement();
                foldout.Add(m_Container);

#if UNITY_2021_2_OR_NEWER
                var inspectorElement = new InspectorElement(editor);
                inspectorElement.AddToClassList(k_InspectorElementClassName);
                inspectorElement.TrackSerializedObjectValue(editor.serializedObject, _ => OnPropertyChanged?.Invoke());

                // when an ExposedReference is changed, its ID does not change and does not trigger a property change
                // add a callback on the context object (which is usually the Playable Director) when the exposed reference property table is modified
                using SerializedProperty property = TimelineInspectorUtility.FindExposedReferenceTableFrom(editor.m_Context);
                if (property != null)
                    this.TrackPropertyValue(property, _ => OnPropertyChanged?.Invoke());

                m_Container.Add(inspectorElement);
#else
                m_Container.Add(new IMGUIContainer(() =>
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        editor.OnInspectorGUI();
                        if (scope.changed)
                            OnPropertyChanged?.Invoke();
                    }
                }));
#endif
            }

            public void UpdateLockState(bool locked)
            {
                bool enabled = !locked;
                if (m_Container.enabledSelf != enabled)
                    m_Container.SetEnabled(enabled);
            }
        }

        enum PreviewCurveState
        {
            None = 0,
            MixIn = 1,
            MixOut = 2
        }

        const double k_TimeScaleSensitivity = 0.003;
        const int k_TopMargin = 5;
        const double k_MixMinimum = 0.0;
        const string k_CommonPropertiesName = "clip-inspector-common-properties";

        SerializedProperty m_DisplayNameProperty;
        SerializedProperty m_BlendInDurationProperty;
        SerializedProperty m_BlendOutDurationProperty;
        SerializedProperty m_EaseInDurationProperty;
        SerializedProperty m_EaseOutDurationProperty;
        SerializedProperty m_ClipInProperty;
        SerializedProperty m_TimeScaleProperty;
        SerializedProperty m_PostExtrapolationModeProperty;
        SerializedProperty m_PreExtrapolationModeProperty;
        SerializedProperty m_PostExtrapolationTimeProperty;
        SerializedProperty m_PreExtrapolationTimeProperty;
        SerializedProperty m_MixInCurveProperty;
        SerializedProperty m_MixOutCurveProperty;
        SerializedProperty m_BlendInCurveModeProperty;
        SerializedProperty m_BlendOutCurveModeProperty;

        void InitializeProperties()
        {
            m_DisplayNameProperty = serializedObject.FindProperty("m_Clip.m_DisplayName");
            m_BlendInDurationProperty = serializedObject.FindProperty("m_Clip.m_BlendInDuration");
            m_BlendOutDurationProperty = serializedObject.FindProperty("m_Clip.m_BlendOutDuration");
            m_EaseInDurationProperty = serializedObject.FindProperty("m_Clip.m_EaseInDuration");
            m_EaseOutDurationProperty = serializedObject.FindProperty("m_Clip.m_EaseOutDuration");
            m_ClipInProperty = serializedObject.FindProperty("m_Clip.m_ClipIn");
            m_TimeScaleProperty = serializedObject.FindProperty("m_Clip.m_TimeScale");
            m_PostExtrapolationModeProperty = serializedObject.FindProperty("m_Clip.m_PostExtrapolationMode");
            m_PreExtrapolationModeProperty = serializedObject.FindProperty("m_Clip.m_PreExtrapolationMode");
            m_PostExtrapolationTimeProperty = serializedObject.FindProperty("m_Clip.m_PostExtrapolationTime");
            m_PreExtrapolationTimeProperty = serializedObject.FindProperty("m_Clip.m_PreExtrapolationTime");
            m_MixInCurveProperty = serializedObject.FindProperty("m_Clip.m_MixInCurve");
            m_MixOutCurveProperty = serializedObject.FindProperty("m_Clip.m_MixOutCurve");
            m_BlendInCurveModeProperty = serializedObject.FindProperty("m_Clip.m_BlendInCurveMode");
            m_BlendOutCurveModeProperty = serializedObject.FindProperty("m_Clip.m_BlendOutCurveMode");
        }

        TimelineAsset m_TimelineAsset;
        List<EditorClipSelection> m_SelectionCache;
        Editor m_SelectedPlayableAssetsInspector;
        ClipInspectorCurveEditor m_ClipCurveEditor;
        CurvePresetLibrary m_CurvePresets;
        string m_MultiselectionHeaderTitle;
        ClipInspectorSelectionInfo m_SelectionInfo;
        PreviewCurveState m_PreviewCurveState;
        CustomPropertiesElement m_CustomPropertiesElement;

        bool hasMultipleSelection
        {
            get { return targets.Length > 1; }
        }

        float currentFrameRate
        {
            get { return m_TimelineAsset != null ? (float)m_TimelineAsset.editorSettings.frameRate : (float)TimelineAsset.EditorSettings.kDefaultFrameRate; }
        }

        bool selectionHasIncompatibleCapabilities
        {
            get
            {
                return !(m_SelectionInfo.supportsBlending
                    && m_SelectionInfo.supportsClipIn
                    && m_SelectionInfo.supportsExtrapolation
                    && m_SelectionInfo.supportsSpeedMultiplier);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            if (m_SelectionInfo == null)
                return root;

            root.Add(new IMGUIContainer(DrawCommonProperties) { name = k_CommonPropertiesName });

            UnityEngine.Object[] selectedAssets = m_SelectionCache.Select(e => e.clip.asset).ToArray();

            if (m_SelectionInfo.selectedAssetTypesAreHomogeneous)
            {
                TimelineInspectorUtility.GetInspectorForObjects(selectedAssets, ref m_SelectedPlayableAssetsInspector);
                if (CanShowPlayableAssetInspector())
                {
                    string title = PlayableAssetSectionTitle();
                    m_CustomPropertiesElement = new CustomPropertiesElement(m_SelectedPlayableAssetsInspector, title);
                    m_CustomPropertiesElement.OnPropertyChanged += OnCustomInspectorChanged;
                    m_CustomPropertiesElement.UpdateLockState(IsLocked());
                    root.Add(m_CustomPropertiesElement);
                }
            }

            return root;
        }

        public override bool RequiresConstantRepaint()
        {
            return base.RequiresConstantRepaint() || (m_SelectedPlayableAssetsInspector != null && m_SelectedPlayableAssetsInspector.RequiresConstantRepaint());
        }

        internal override void OnHeaderTitleGUI(Rect titleRect, string header)
        {
            if (hasMultipleSelection)
            {
                base.OnHeaderTitleGUI(titleRect, m_MultiselectionHeaderTitle);
                return;
            }

            if (m_DisplayNameProperty != null)
            {
                using (new EditorGUI.DisabledScope(!IsEnabled()))
                {
                    serializedObject.Update();
                    if (IsLocked())
                    {
                        base.OnHeaderTitleGUI(titleRect, m_DisplayNameProperty.stringValue);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.DelayedTextField(titleRect, m_DisplayNameProperty, GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                        {
                            ApplyModifiedProperties();
                            TimelineWindow.RepaintIfEditingTimelineAsset(m_TimelineAsset);
                        }
                    }
                }
            }
        }

        internal override Rect DrawHeaderHelpAndSettingsGUI(Rect r)
        {
            using (new EditorGUI.DisabledScope(IsLocked()))
            {
                var helpSize = EditorStyles.iconButton.CalcSize(EditorGUI.GUIContents.helpIcon);
                // Show Editor Header Items.
                return EditorGUIUtility.DrawEditorHeaderItems(new Rect(r.xMax - helpSize.x, r.y + k_TopMargin, helpSize.x, helpSize.y), targets);
            }
        }

        internal override void OnHeaderIconGUI(Rect iconRect)
        {
            using (new EditorGUI.DisabledScope(IsLocked()))
            {
                var bgColor = Color.white;
                if (!EditorGUIUtility.isProSkin)
                    bgColor.a = 0.55f;
                using (new GUIColorOverride(bgColor))
                {
                    GUI.Label(iconRect, Styles.TimelineClipBG);
                }

                var fgColor = Color.white;
                if (m_SelectionInfo != null && m_SelectionInfo.uniqueParentTracks.Count == 1)
                    fgColor = TrackResourceCache.GetTrackColor(m_SelectionInfo.uniqueParentTracks.First());

                using (new GUIColorOverride(fgColor))
                {
                    GUI.Label(iconRect, Styles.TimelineClipFG);
                }
            }
        }

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            m_ClipCurveEditor = new ClipInspectorCurveEditor();

            m_SelectionCache = new List<EditorClipSelection>();
            var selectedClips = new List<TimelineClip>();
            foreach (var editorClipObject in targets)
            {
                var editorClip = editorClipObject as EditorClip;
                if (editorClip != null)
                {
                    //all selected clips should have the same TimelineAsset
                    if (!IsTimelineAssetValidForEditorClip(editorClip))
                    {
                        m_SelectionCache.Clear();
                        return;
                    }
                    m_SelectionCache.Add(new EditorClipSelection(editorClip));
                    selectedClips.Add(editorClip.clip);
                }
            }

            InitializeProperties();
            m_SelectionInfo = new ClipInspectorSelectionInfo(selectedClips);
            m_MultiselectionHeaderTitle = m_SelectionCache.Count + " " + Styles.MultipleSelectionTitle.text;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            DestroyImmediate(m_SelectedPlayableAssetsInspector);
        }

        void DrawClipProperties()
        {
            IEnumerable<EditorClipSelection> dirtyEditorClipSelection = m_SelectionCache.Where(s => s.editorClip.GetHashCode() != s.editorClip.lastHash);
            UnselectCurves();

            EditorGUI.BeginChangeCheck();

            //Group Selection
            if (hasMultipleSelection)
            {
                GUILayout.Label(Styles.GroupTimingTitle);
                EditorGUI.indentLevel++;
                DrawGroupSelectionProperties();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            //Draw clip timing
            GUILayout.Label(Styles.ClipTimingTitle);

            if (hasMultipleSelection && selectionHasIncompatibleCapabilities)
            {
                GUILayout.Label(Styles.MultipleClipsSelectedIncompatibleCapabilitiesWarning, EditorStyles.helpBox);
            }

            EditorGUI.indentLevel++;

            if (!m_SelectionInfo.containsAtLeastTwoClipsOnTheSameTrack)
            {
                DrawStartTimeField();
                DrawEndTimeField();
            }

            if (!hasMultipleSelection)
            {
                DrawDurationProperty();
            }

            if (m_SelectionInfo.supportsBlending)
            {
                EditorGUILayout.Space();
                DrawBlendingProperties();
            }

            if (m_SelectionInfo.supportsClipIn)
            {
                EditorGUILayout.Space();
                DrawClipInProperty();
            }

            if (!hasMultipleSelection && m_SelectionInfo.supportsSpeedMultiplier)
            {
                EditorGUILayout.Space();
                DrawTimeScale();
            }

            EditorGUI.indentLevel--;

            var hasDirtyEditorClips = false;
            foreach (EditorClipSelection editorClipSelection in dirtyEditorClipSelection)
            {
                EditorUtility.SetDirty(editorClipSelection.editorClip);
                hasDirtyEditorClips = true;
            }

            //Re-evaluate the graph in case of a change in properties
            var propertiesHaveChanged = false;
            if (EditorGUI.EndChangeCheck() || hasDirtyEditorClips)
            {
                if (TimelineEditor.state != null && TimelineWindow.IsEditingTimelineAsset(m_TimelineAsset))
                {
                    TimelineEditor.state.Evaluate();
                    TimelineEditor.window.Repaint();
                }
                propertiesHaveChanged = true;
            }

            //Draw Animation Extrapolation
            if (m_SelectionInfo.supportsExtrapolation)
            {
                EditorGUILayout.Space();
                GUILayout.Label(Styles.AnimationExtrapolationTitle);
                EditorGUI.indentLevel++;
                DrawExtrapolationOptions();
                EditorGUI.indentLevel--;
            }

            //Blend curves
            if (m_SelectionInfo.supportsBlending)
            {
                EditorGUILayout.Space();
                GUILayout.Label(Styles.BlendCurvesTitle);
                EditorGUI.indentLevel++;
                DrawBlendOptions();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            if (propertiesHaveChanged)
            {
                foreach (EditorClipSelection item in m_SelectionCache)
                    item.editorClip.lastHash = item.editorClip.GetHashCode();
                m_SelectionInfo.Update();
            }
        }

        void DrawCommonProperties()
        {
            if (TimelineEditor.window == null || m_TimelineAsset == null)
                return;

            using (new EditorGUI.DisabledScope(IsLocked()))
            using (new LabelWidthScope(0f)) //reset label width to prevent state pollution by unrelated IMGUI code
            {
                EditMode.HandleModeClutch();

                serializedObject.Update();
                DrawClipProperties();
                ApplyModifiedProperties();
            }

            //the playable asset needs to be refresh on each frame
            UpdatePlayableAsset();
        }

        void UpdatePlayableAsset()
        {
            PreparePlayableAssets();
            m_CustomPropertiesElement?.UpdateLockState(IsLocked());
        }

        internal override bool IsEnabled()
        {
            if (!TimelineUtility.IsCurrentSequenceValid() || IsCurrentSequenceReadOnly())
                return false;

            if (m_TimelineAsset != TimelineEditor.state.editSequence.asset)
                return false;
            return base.IsEnabled();
        }

        void DrawTimeScale()
        {
            var inputEvent = InputEvent.None;
            var newEndTime = m_SelectionInfo.end;
            var oldTimeScale = m_TimeScaleProperty.doubleValue;

            EditorGUI.BeginChangeCheck();
            var newTimeScale = TimelineInspectorUtility.DelayedAndDraggableDoubleField(Styles.TimeScaleName, oldTimeScale, ref inputEvent, k_TimeScaleSensitivity);

            if (EditorGUI.EndChangeCheck())
            {
                newTimeScale = newTimeScale.Clamp(TimelineClip.kTimeScaleMin, TimelineClip.kTimeScaleMax);
                newEndTime = m_SelectionInfo.start + (m_SelectionInfo.duration * oldTimeScale / newTimeScale);
            }
            EditMode.inputHandler.ProcessTrim(inputEvent, newEndTime, true);
        }

        void DrawStartTimeField()
        {
            var inputEvent = InputEvent.None;
            var newStart = TimelineInspectorUtility.TimeFieldUsingTimeReference(Styles.StartName, m_SelectionInfo.multipleClipStart, false, m_SelectionInfo.hasMultipleStartValues, currentFrameRate, 0.0, TimelineClip.kMaxTimeValue, ref inputEvent);

            if (inputEvent.InputHasBegun() && m_SelectionInfo.hasMultipleStartValues)
            {
                var items = ItemsUtils.ToItems(m_SelectionInfo.clips);
                EditMode.inputHandler.SetValueForEdge(items, AttractedEdge.Left, newStart); //if the field has multiple values, set the same start on all selected clips
                m_SelectionInfo.Update(); //clips could have moved relative to each other, recalculate
            }

            EditMode.inputHandler.ProcessMove(inputEvent, newStart);
        }

        void DrawEndTimeField()
        {
            var inputEvent = InputEvent.None;
            var newEndTime = TimelineInspectorUtility.TimeFieldUsingTimeReference(Styles.EndName, m_SelectionInfo.multipleClipEnd, false, m_SelectionInfo.hasMultipleEndValues, currentFrameRate, 0, TimelineClip.kMaxTimeValue, ref inputEvent);

            if (inputEvent.InputHasBegun() && m_SelectionInfo.hasMultipleEndValues)
            {
                var items = ItemsUtils.ToItems(m_SelectionInfo.clips);
                EditMode.inputHandler.SetValueForEdge(items, AttractedEdge.Right, newEndTime); //if the field has multiple value, set the same end on all selected clips
                m_SelectionInfo.Update(); //clips could have moved relative to each other, recalculate
            }

            var newStartValue = m_SelectionInfo.multipleClipStart + (newEndTime - m_SelectionInfo.multipleClipEnd);
            EditMode.inputHandler.ProcessMove(inputEvent, newStartValue);
        }

        void DrawExtrapolationOptions()
        {
            // PreExtrapolation
            var preExtrapolationTime = m_PreExtrapolationTimeProperty.doubleValue;
            bool hasPreExtrap = preExtrapolationTime > 0.0;
            if (hasPreExtrap)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_PreExtrapolationModeProperty, Styles.PreExtrapolateLabel);
                using (new GUIMixedValueScope(m_PreExtrapolationTimeProperty.hasMultipleDifferentValues))
                    EditorGUILayout.DoubleField(preExtrapolationTime, EditorStyles.label);
                EditorGUILayout.EndHorizontal();
            }

            // PostExtrapolation
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_PostExtrapolationModeProperty, Styles.PostExtrapolateLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyModifiedProperties();
                    //recalculate Extrapolation times to update next clip pre-extrapolation
                    foreach (var track in m_SelectionInfo.uniqueParentTracks)
                        Extrapolation.CalculateExtrapolationTimes(track);

                    TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
                }

                using (new GUIMixedValueScope(m_PostExtrapolationTimeProperty.hasMultipleDifferentValues))
                    EditorGUILayout.DoubleField(m_PostExtrapolationTimeProperty.doubleValue, EditorStyles.label);
                EditorGUILayout.EndHorizontal();
            }
        }

        void OnDestroy()
        {
            DestroyImmediate(m_SelectedPlayableAssetsInspector);
        }

        public override GUIContent GetPreviewTitle()
        {
            return Styles.PreviewTitle;
        }

        public override bool HasPreviewGUI()
        {
            return m_PreviewCurveState != PreviewCurveState.None;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (m_PreviewCurveState != PreviewCurveState.None && m_ClipCurveEditor != null)
            {
                SetCurveEditorTrackHead();
                m_ClipCurveEditor.OnGUI(r, m_CurvePresets);
            }
        }

        void SetCurveEditorTrackHead()
        {
            if (TimelineEditor.window == null || TimelineEditor.state == null)
                return;

            if (hasMultipleSelection)
                return;

            var editorClip = target as EditorClip;
            if (editorClip == null)
                return;

            var director = TimelineEditor.state.editSequence.director;

            if (director == null)
                return;

            m_ClipCurveEditor.trackTime = ClipInspectorCurveEditor.kDisableTrackTime;
        }

        void UnselectCurves()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (m_ClipCurveEditor != null)
                    m_ClipCurveEditor.SetUpdateCurveCallback(null);
                m_PreviewCurveState = PreviewCurveState.None;
            }
        }

        // Callback when the mixin/mixout properties are clicked on
        void OnMixCurveSelected(string title, CurvePresetLibrary library, SerializedProperty curveSelected, bool easeIn)
        {
            m_PreviewCurveState = easeIn ? PreviewCurveState.MixIn : PreviewCurveState.MixOut;

            m_CurvePresets = library;
            var animationCurve = curveSelected.animationCurveValue;
            m_ClipCurveEditor.headerString = title;
            m_ClipCurveEditor.SetCurve(animationCurve);
            m_ClipCurveEditor.SetSelected(animationCurve);
            if (easeIn)
                m_ClipCurveEditor.SetUpdateCurveCallback(MixInCurveUpdated);
            else
                m_ClipCurveEditor.SetUpdateCurveCallback(MixOutCurveUpdated);
            Repaint();
        }

        // callback when the mix property is updated
        void MixInCurveUpdated(AnimationCurve curve, EditorCurveBinding binding)
        {
            curve.keys = CurveEditUtility.SanitizeCurveKeys(curve.keys, true);
            m_MixInCurveProperty.animationCurveValue = curve;
            ApplyModifiedProperties();
            var editorClip = target as EditorClip;
            if (editorClip != null)
                editorClip.lastHash = editorClip.GetHashCode();
            RefreshCurves();
        }

        void MixOutCurveUpdated(AnimationCurve curve, EditorCurveBinding binding)
        {
            curve.keys = CurveEditUtility.SanitizeCurveKeys(curve.keys, false);
            m_MixOutCurveProperty.animationCurveValue = curve;
            ApplyModifiedProperties();
            var editorClip = target as EditorClip;
            if (editorClip != null)
                editorClip.lastHash = editorClip.GetHashCode();
            RefreshCurves();
        }

        void RefreshCurves()
        {
            AnimationCurvePreviewCache.ClearCache();
            TimelineWindow.RepaintIfEditingTimelineAsset(m_TimelineAsset);
            Repaint();
        }

        void DrawBlendCurve(GUIContent title, SerializedProperty modeProperty, SerializedProperty curveProperty, Action<SerializedProperty> onCurveClick)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(modeProperty, title);
            if (hasMultipleSelection)
            {
                GUILayout.FlexibleSpace();
            }
            else
            {
                using (new EditorGUI.DisabledScope(modeProperty.intValue != (int)TimelineClip.BlendCurveMode.Manual))
                {
                    ClipInspectorCurveEditor.CurveField(GUIContent.none, curveProperty, onCurveClick);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void OnCustomInspectorChanged()
        {
            MarkClipsDirty();
            if (TimelineWindow.IsEditingTimelineAsset(m_TimelineAsset) && TimelineEditor.state != null)
            {
                //the playable asset editor can control how it handles property changes
                if (m_SelectedPlayableAssetsInspector is IInspectorChangeHandler inspectorChangeHandler)
                    inspectorChangeHandler.OnPlayableAssetChangedInInspector();
                else //refresh the timeline by default
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        void PreparePlayableAssets()
        {
            if (m_SelectedPlayableAssetsInspector != null)
            {
                if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
                {
                    foreach (EditorClipSelection selectedItem in m_SelectionCache)
                        CurvesOwnerInspectorHelper.PreparePlayableAsset(selectedItem);
                }
            }
        }

        void ApplyModifiedProperties()
        {
            // case 926861 - we need to force the track to be dirty since modifying the clip does not
            //  automatically mark the track asset as dirty
            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (var obj in serializedObject.targetObjects)
                {
                    var editorClip = obj as EditorClip;
                    if (editorClip != null && editorClip.clip != null && editorClip.clip.GetParentTrack() != null)
                    {
                        editorClip.clip.MarkDirty();
                        EditorUtility.SetDirty(editorClip.clip.GetParentTrack());
                    }
                }
            }
        }

        void MarkClipsDirty()
        {
            foreach (var obj in targets)
            {
                var editorClip = obj as EditorClip;
                if (editorClip != null && editorClip.clip != null)
                {
                    editorClip.clip.MarkDirty();
                }
            }
        }

        string PlayableAssetSectionTitle()
        {
            var firstSelectedClipAsset = m_SelectionCache.Any() ? m_SelectionCache.First().clip.asset : null;
            return firstSelectedClipAsset != null
                ? ObjectNames.NicifyVariableName(firstSelectedClipAsset.GetType().Name)
                : string.Empty;
        }

        bool IsTimelineAssetValidForEditorClip(EditorClip editorClip)
        {
            var trackAsset = editorClip.clip.GetParentTrack();
            if (trackAsset == null)
                return false;

            var clipTimelineAsset = trackAsset.timelineAsset;
            if (m_TimelineAsset == null)
                m_TimelineAsset = clipTimelineAsset;
            else if (clipTimelineAsset != m_TimelineAsset)
            {
                m_TimelineAsset = null;
                return false;
            }
            return true;
        }

        bool CanShowPlayableAssetInspector()
        {
            if (m_SelectedPlayableAssetsInspector != null)
            {
                bool isValidMultipleSelection = m_SelectedPlayableAssetsInspector.canEditMultipleObjects &&
                    m_SelectionInfo.selectedAssetTypesAreHomogeneous;
                return !hasMultipleSelection || isValidMultipleSelection;
            }

            return false;
        }

        void DrawDurationProperty()
        {
            var minDuration = 1.0 / 30.0;
            if (currentFrameRate > float.Epsilon)
            {
                minDuration = 1.0 / currentFrameRate;
            }

            var inputEvent = InputEvent.None;
            var newDuration = TimelineInspectorUtility.DurationFieldUsingTimeReference(
                Styles.DurationName, m_SelectionInfo.start, m_SelectionInfo.end, false, m_SelectionInfo.hasMultipleDurationValues, currentFrameRate, minDuration, TimelineClip.kMaxTimeValue, ref inputEvent);
            EditMode.inputHandler.ProcessTrim(inputEvent, m_SelectionInfo.start + newDuration, false);
        }

        void DrawBlendingProperties()
        {
            var inputEvent = InputEvent.None;
            double blendMax;
            GUIContent label;

            var useBlendIn = m_SelectionInfo.hasBlendIn;
            SerializedProperty currentMixInProperty;
            if (!useBlendIn)
            {
                currentMixInProperty = m_EaseInDurationProperty;
                var blendOutStart = m_SelectionInfo.duration - m_BlendOutDurationProperty.doubleValue;
                blendMax = Math.Min(Math.Max(k_MixMinimum, m_SelectionInfo.maxMixIn), blendOutStart);
                label = Styles.EaseInDurationName;
            }
            else
            {
                currentMixInProperty = m_BlendInDurationProperty;
                blendMax = TimelineClip.kMaxTimeValue;
                label = Styles.BlendInDurationName;
            }
            if (blendMax > TimeUtility.kTimeEpsilon)
                TimelineInspectorUtility.TimeField(currentMixInProperty, label, useBlendIn, currentFrameRate, k_MixMinimum,
                    blendMax, ref inputEvent);


            var useBlendOut = m_SelectionInfo.hasBlendOut;
            SerializedProperty currentMixOutProperty;
            if (!useBlendOut)
            {
                currentMixOutProperty = m_EaseOutDurationProperty;
                var blendInEnd = m_SelectionInfo.duration - m_BlendInDurationProperty.doubleValue;
                blendMax = Math.Min(Math.Max(k_MixMinimum, m_SelectionInfo.maxMixOut), blendInEnd);
                label = Styles.EaseOutDurationName;
            }
            else
            {
                currentMixOutProperty = m_BlendOutDurationProperty;
                blendMax = TimelineClip.kMaxTimeValue;
                label = Styles.BlendOutDurationName;
            }
            if (blendMax > TimeUtility.kTimeEpsilon)
                TimelineInspectorUtility.TimeField(currentMixOutProperty, label, useBlendOut, currentFrameRate,
                    k_MixMinimum, blendMax, ref inputEvent);
        }

        void DrawClipInProperty()
        {
            var action = InputEvent.None;
            TimelineInspectorUtility.TimeField(m_ClipInProperty, Styles.ClipInName, false, currentFrameRate, 0, TimelineClip.kMaxTimeValue, ref action);
        }

        void DrawBlendOptions()
        {
            EditorGUI.BeginChangeCheck();

            DrawBlendCurve(Styles.BlendInCurveName, m_BlendInCurveModeProperty, m_MixInCurveProperty, x => OnMixCurveSelected("Blend In", BuiltInPresets.blendInPresets, x, true));
            DrawBlendCurve(Styles.BlendOutCurveName, m_BlendOutCurveModeProperty, m_MixOutCurveProperty, x => OnMixCurveSelected("Blend Out", BuiltInPresets.blendOutPresets, x, false));

            if (EditorGUI.EndChangeCheck())
                TimelineWindow.RepaintIfEditingTimelineAsset(m_TimelineAsset);
        }

        void DrawGroupSelectionProperties()
        {
            var inputEvent = InputEvent.None;
            var newStartTime = TimelineInspectorUtility.TimeField(Styles.MultipleClipStartName, m_SelectionInfo.multipleClipStart, false, false, currentFrameRate, 0, TimelineClip.kMaxTimeValue, ref inputEvent);
            EditMode.inputHandler.ProcessMove(inputEvent, newStartTime);

            inputEvent = InputEvent.None;
            var newEndTime = TimelineInspectorUtility.TimeField(Styles.MultipleClipEndName, m_SelectionInfo.multipleClipEnd, false, false, currentFrameRate, 0, TimelineClip.kMaxTimeValue, ref inputEvent);
            var newStartValue = newStartTime + (newEndTime - m_SelectionInfo.multipleClipEnd);
            EditMode.inputHandler.ProcessMove(inputEvent, newStartValue);
        }

        bool IsLocked()
        {
            if (!TimelineUtility.IsCurrentSequenceValid() || IsCurrentSequenceReadOnly())
                return true;

            return targets.OfType<EditorClip>().Any(t => t.clip.GetParentTrack() != null && t.clip.GetParentTrack().lockedInHierarchy);
        }

        static bool IsCurrentSequenceReadOnly()
        {
            return TimelineEditor.state.editSequence.isReadOnly;
        }

        void OnUndoRedoPerformed()
        {
            if (m_PreviewCurveState == PreviewCurveState.None)
                return;

            // if an undo is performed the curves need to be updated in the curve editor, as the reference to them is no longer valid
            // case 978673
            if (m_ClipCurveEditor != null)
            {
                serializedObject.Update();
                m_ClipCurveEditor.SetCurve(m_PreviewCurveState == PreviewCurveState.MixIn ? m_MixInCurveProperty.animationCurveValue : m_MixOutCurveProperty.animationCurveValue);
            }
        }
    }
}
