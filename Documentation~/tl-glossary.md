# Timeline glossary

This topic provides an alphabetical list of the terminology used throughout the Timeline documentation.

**animatable property**: A property belonging to a GameObject, or belonging to a component added to a GameObject, that can have a different value over time.

**animation**: The result of adding two keyframes with different values, at two different times, for the same animatable property. The basic definition is a value that changes over time.

**animation curve**: The curve drawn between keyframes set for the same animatable property, at different times. The position of the tangents and the selected interpolation mode for each keyframe determines the shape of the animation curve.

**binding** or **Track binding**: Refers to the link between Timeline asset tracks and the GameObjects in the scene. When you link a GameObject to a track, the track animates the GameObject. Bindings are stored as part of the Timeline instance.

**blend** and **blend area**: The area where two Animation clips, Audio clips, or Control clips overlap. The overlap creates a transition that is referred to as a **blend**. The duration of the overlap is referred to as the **blend area**. The blend area sets the duration of the transition.

**Blend In curve**: In a blend between two Animation clips, Audio clips, or Control clips, there are two blend curves. The blend curve for the incoming clip is referred to as the **Blend In** curve.

**Blend Out curve**: In a blend between two Animation clips, Audio clips, or Control clips, there are two blend curves. The blend curve for the out-going clip is referred to as the **Blend Out** curve.

**clip**: A generic term that refers to any clip within the Content view of the Timeline window.

**Content view**: The area in the Timeline window where you add, position, and manipulate clips and markers.

**Curves view**: The area in the Timeline window that displays the animation curves for Infinite clips, or for Animation clips that have been converted from Infinite clips. The Curves view also displays animation curves for Audio Track properties. The Curves view is similar to [Curves mode](https://docs.unity3d.com/Manual/animeditor-AnimationCurves.html) in the Animation window.

**gap extrapolation**: How an Animation track approximates animation data in the gaps before and after an Animation clip.

**field**: A generic term that describes an editable box where the user clicks and types-in a value.

**incoming clip:** The second clip in a blend between two clips. The first clip, the out-going clip, transitions to the second clip, the **incoming clip**.

**Infinite clip**: A special animation clip that contains basic keyframe animation recorded directly to an Animation track within the Timeline window. An Infinite clip cannot be positioned, trimmed, or split because it does not have a defined duration: it spans the entirety of an Animation track.

**interpolation**: The estimation of values between keyframes that determine the shape of an animation curve.

**interpolation mode**: The interpolation algorithm that draws the animation curve between keyframes. The interpolation mode also joins or breaks left and right tangents. Interpolation mode is also referred to as **tangent mode** and **tangent type**.

**keyframe**: The value of an animatable property set at a specific point in time. Setting at least two keyframes for the same property creates an animation.

**marker**: A generic term that refers to any marker within the Content view of the Timeline window.

**out-going clip**: The first clip in a blend between two clips. The first clip, the **out-going clip**, transitions to the second clip, the incoming clip.

**Playhead Location field**: The field that expresses the location of the Timeline Playhead in frames, timecode, or seconds depending on the Timeline Settings.

**property**: A generic term for the editable fields, toggles, checkboxes, or menus that comprise a component. An editable field is also referred to as a **field**.

**tangent**: One of two handles that control the shape of the animation curve before and after a keyframe. Tangents appear when a keyframe is selected in the Curves view.

**tangent mode** or **tangent type**: The selected interpolation mode used by the left tangent, right tangent, or both tangents.

**Timeline** or **Unity's Timeline**: Generic terms that refer to all features, windows, editors, and components related to creating, modifying, or reusing cut-scenes, cinematic, and gameplay sequences.

**Timeline asset**: Refers to the tracks, clips, and recorded keyframe animation that comprise a cut-scene, cinematic, gameplay sequence, or other effect created with the Timeline window. A Timeline asset does not include bindings to the GameObjects animated by the Timeline asset. The bindings to scene GameObjects are stored in the Timeline instance. A Timeline asset is project-based.

**Timeline instance**: Refers to the link between a Timeline asset and the GameObjects that the Timeline asset animates in the scene. You create a Timeline instance by associating a Timeline asset to a GameObject through a Playable Director component. A Timeline instance is scene-based.

**Timeline Playback Controls**: The row of buttons, toggles, and fields in the Timeline window that controls playback of the Timeline instance. The Timeline Playback Controls affect the location of the Timeline Playhead.

**Timeline Playback mode**: The mode that previews the Timeline instance in the Timeline window. Timeline Playback mode does not support audio playback. Timeline Playback mode is a simulation of Play mode in the Unity.

**Timeline Playhead**: The white marker and line that indicates the exact point in time being previewed in the Timeline window.

**Timeline Selector**: The name of the menu in the Timeline window that selects the Timeline instance to be previewed or modified.

**Timeline window**: The official name of the window where you create, modify, and preview a Timeline instance. Modifications to a Timeline instance also affect the Timeline asset.

**track**: A generic term that refers to any track within the Track list of the Timeline window.

**Track group**: A container in the Track list for organizing a collection of tracks.

**Track sub-group**: A Track group nested within another Track group. A Track group can have multiple nested sub-groups.

**Track list**: The area in the Timeline window where you add, group, and modify tracks. Each track is represented by a Track Header.
