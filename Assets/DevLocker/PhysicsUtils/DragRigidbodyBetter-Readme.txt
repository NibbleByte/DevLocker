==== Drag Ragdoll Rigidbody ====
Simple script to drag around ragdoll (or any object with Rigidbody) with the mouse.
It works by temporarily attaching invisible spring joint to the clicked rigidbody and moving it with the mouse.
Useful for testing out the limits of your ragdoll setup.
Usage: attach the DragRigidbodyBetter script to an empty object in the scene. It will spawn dragged springs as child objects.

This script is a replacement for the now removed DragRigidbody that was packed with the Unity Standard Assets.
It adds some neat features and controls like twisting, pin the springs and render the springs.

Controls: 
Left mouse button on any rigidbody - drag it.
While dragging:
- Scroll wheel - drag object closer or further
- Space - pin active spring at current mouse position
- Delete - destroy all pinned springs
- Z/C - twist active spring to left or right