# Timeline Preferences

Use the Timeline Preferences to choose the Timeline window settings such as the unit of measurement, whether to display audio waveforms, and edge snap settings.

The Timeline Preferences are found in the Preferences window. The Timeline Preferences are settings that apply to the Timeline window regardless of the selected Timeline asset or Timeline instance.

To open the Timeline Preferences, from the Timeline window, click the Gear button and choose **Preferences Page** from the Timeline Options menu. The Preferences window opens with Timeline selected.

## Time Unit

Select either **Frames**, **Timecode**, or **Seconds** to set the Timeline window to display time in that format. **Timecode** displays the time in seconds with sub-second values displayed in frames.

## Playback Scrolling Mode

Use **Playback Scrolling Mode** to set whether the Content view follows the Timeline Playhead when in [Timeline Playback mode](tl-play-ctrls.md#playbutton).

|**Mode** |**Description** |
|:---|:---|
|**None**|The Content view does not follow the Timeline Playhead.|
|**Pan**|The Content view pans as the Timeline Playhead reaches the edge of the viewable area.|
|**Smooth**|The Content view scrolls while keeping the Timeline Playhead at the center of the Content view and smoothly scrolls the Content view.|

## Show Audio Waveforms

Enable **Show Audio Waveforms** to draw the waveforms for all audio clips on all audio tracks. For example, use an audio waveform as a guide when manually positioning an Audio clip of footsteps with the Animation clip of a humanoid walking. Disable **Show Audio Waveform** to hide audio waveforms. **Show Audio Waveforms** is enabled by default.

## Allow Audio Scrubbing

Enable **Allow Audio Scrubbing** to play audio while dragging the Timeline Playhead. Disable **Allow Audio Scrubbing** to stop playing audio while dragging the Timeline Playhead. When disabled, Timeline only plays audio when in [Timeline Playback mode](tl-play-ctrls.md#playbutton).

## Snap to Frame

Enable **Snap to Frame** to manipulate clips, preview Timeline instances, and to drag and position the Timeline Playhead using frames. Disable **Snap to Frame** to position clips between frames. **Snap to Frame** is enabled by default.

For example, when **Snap to Frame** is disabled and you drag the Timeline Playhead, it moves the playhead between frames. The format of the [Playhead Location](tl-play-ctrls.md#playheadlocation) field is different depending on whether the **Time Unit** is set to **Frames**, **Timecode** or **Seconds**:

* When the **Time Unit** is set to **Frames**, the Playhead Location displays frames and subframes. For example, 8 frames and 34 subframes displays as `8.34`.
* When the **Time Unit** is set to **Timecode**, the Playhead Location displays seconds, frames, and subframes. For example, 6 seconds, 17 frames, and 59 subframes displays as `6:17 [.59]`.
* When the **Time Unit** is set to **Seconds**, the Playhead Location displays seconds. For example, 6.5 seconds displays as `6:50`.

Manipulating clips, previewing Timeline instances, and positioning the playhead at the subframes level is useful when attempting to synchronize animation and effects with audio. Many high-end audio processing software products create audio waveforms with subframe accuracy.

## Edge Snap

Enable the **Edge Snap** option to snap clips when you position, trim, and create blends. When enabled, the start or end of a clip snaps when it is dragged within 10 pixels of the following:
* The Timeline Playhead.
* The start or end of a clip on the same track or other tracks.
* The start or end of the Timeline instance itself.

The start guide or end guide is redrawn in white to indicate when the clip snaps to the edge of another clip or the Timeline Playhead.

Disable **Edge Snap** to create more accurate blends, ease-ins, or ease-outs and when it is not important to have clips snap to the Timeline Playhead, the start and end of other clips, or the start and end of the Timeline instance itself. **Edge Snap** is enabled by default.

## Playback Locked To Frame

Enable **Playback Locked To Frame** to enable frame accurate previewing during playback.
