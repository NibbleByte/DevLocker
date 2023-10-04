# DevLocker

Collection of useful Unity 3D tools and scripts that don't have their own repo just yet. Some of the scripts are gathered from the internet and improved over time (since they didn't have a proper home).

## DragRigidbodyBetter.cs
Simple script to drag around ragdoll (or any object with Rigidbody) with the mouse.
It works by temporarily attaching invisible spring joint to the clicked rigidbody and moving it with the mouse.
Useful for testing out the limits of your ragdoll setup.
Usage: attach the DragRigidbodyBetter script to an empty object in the scene. It will spawn dragged springs as child objects.

This script is a replacement for the now removed DragRigidbody that was packed with the Unity Standard Assets.
It adds some neat features and controls like twisting, pin the springs and render the springs.
```
Controls: 
Left mouse button on any rigidbody - drag it.
While dragging:
- Scroll wheel - drag object closer or further
- Space - pin active spring at current mouse position
- Delete - destroy all pinned springs
- Z/C - twist active spring to left or right
```
![DragRagdollBetter](Docs/Screenshots/DragRagdollBetterShot.png)

## FlyCamera.cs
Allows the game camera to be controlled like the editor Scene View camera (using right-click-drag + WASD). This is an improved version of [Christer Kaitila](https://gist.github.com/McFunkypants/5a9dad582461cb8d9de3)'s FlyCamera script. Just slap it on your camera and it will start working.
Very useful for game jams or prototyping.
```
Controls: 
- WASD to move
- Q / E are up / down
- Hold Shift to speed up
- Pan with the middle mouse button
- Rotate with the right mouse button
- Scroll wheel - zoom
```
![FlyCamera](Docs/Screenshots/FlyCameraShot.png)

## SkinnedBonesGizmos.cs
Draws gizmos for the skinned bones and their links. Select any SkinnedMeshRenderer to use. Can click on the gizmos to select the bone directly.

![SkinnedBonesGizmos](Docs/Screenshots/SkinnedBonesGizmosShot.png)

## TransformResetEditor.cs
Adds reset buttons next to position, rotation and scale controls of the Transform component. Additionally, shows the world position.

![TransformResetEditor](Docs/Screenshots/TransformResetEditorShot.png)

## PrefabInstance.cs (a.k.a. "Poor Mans Nested Prefabs")
This WAS used to bypass the Unity limitation of not having nested prefabs support.
Put this script on a proxy object, link a prefab and it will render that prefab in the scene as if it was there.
You can select and move the proxy object. When the scene runs, the script spawns that prefab onto the proxy object.
When build prefab instances are baked into the scene directly, so there should be no performance overhead.
Can be used recursively.
 
This is an improved version of the ["Poor Mans Nested Prefabs"](http://framebunker.com/blog/poor-mans-nested-prefabs) by Nicholas Francis.
Additionally, we have added a proper Editor.

This script is now obsolete as Unity finally unrolled nested prefabs support.

![PrefabInstance](Docs/Screenshots/PrefabInstanceShot.png)

# Others
There are other even smaller but still helpful scripts. Just take a look around.
