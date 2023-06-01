# Timeline assets and instances

Use the **Timeline window** to create cut-scenes, cinematics, and gameplay sequences by visually arranging tracks and clips linked to GameObjects in your Scene.

![](images/tl-cinematic-example.png)

_A cinematic sequence in the Timeline window._

For each cut-scene, cinematic, or gameplay sequence, the Timeline window saves the following:

* **Timeline asset**: Stores the tracks, clips, and recorded animations without links to the specific GameObjects being animated. The Timeline asset is saved to the Project.
* **Timeline instance**: Stores links to the specific GameObjects being animated or affected by the Timeline asset. These links, referred to as **bindings**, are saved to the Scene.

## Timeline asset

The Timeline window saves track and clip definitions as a **Timeline asset**.

If you record keyframe animation while creating your cinematic, cut-scene, or gameplay sequence, the Timeline window saves the recorded clips as children of the Timeline asset.

![](images/spec-tl-overview-asset.png)

_The Timeline asset saves tracks and clips (A). Timeline window saves recorded clips (B) as children of the Timeline asset._

## Timeline instance

To animate a GameObject in your Scene with a Timeline asset, you must create a **Timeline instance**. A **Timeline instance** associates a Timeline asset with the GameObject in the Scene through a [Playable Director](playable-director.md) component.

When you select a GameObject in a Scene that has a Playable Director component, the Timeline instance displays in the Timeline window. The bindings display in the Timeline window and in the Playable Director component (Inspector window).

![](images/spec-tl-overview-instance.png)

_The Playable Director component displays the Timeline asset (A) with its bound GameObjects (B). The Timeline window displays the same bindings (B) in the Track list._

The Timeline window provides an automated method of creating a Timeline instance while [creating a Timeline asset](wf-create-instance.md).

## Reusing Timeline assets

Because Timeline assets and Timeline instances are separate, you can reuse the same Timeline asset with many Timeline instances.

For example, you could create a Timeline asset named `VictoryTL` with the animation, music, and particle effects that play when the main game character (`Player`) wins. To reuse the `VictoryTL` Timeline asset to animate another game character (`Enemy`) in the same Scene, you can create another Timeline instance for the secondary game character.

![](images/tl-player-victory.png)

_The Player GameObject (red) is attached to the `VictoryTL` Timeline asset_

![](images/tl-enemy-victory.png)

_The Enemy GameObject (green) is also attached to the `VictoryTL` Timeline asset_

Because you are reusing the Timeline asset, any modification to the Timeline asset in the Timeline window results in changes to all Timeline instances associated with this asset.

For example, in the previous example, if you delete the Audio track while modifying the `Player` Timeline instance, the Timeline window removes this track from the `VictoryTL` Timeline asset. This removes the Audio track from all instances associated with the `VictoryTL` Timeline asset, including the `Enemy` Timeline instance.
