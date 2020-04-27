# EasierVRAssets - Change Log (Most Recent: V1.2.0)

## Version 1.2.0

### "CustomGrab" Subpackage:

* __Summary:__
    * New functionality for grabbing - distance grabbing now allowed
    * New functionality for locomotion - Teleportation now allowed

* __Additions:__
    * Added new prefabs:
        * "HoverCursor.prefab"
        * "LocomotionIndicator.prefab"
    * Added new scripts:
        * "BezierCurves.cs"
        * "CommonFunctions.cs"
        * "CustomLocomotion.cs"
        * "ExternalCollider.cs"
        * "HoverCursor.cs"

* __Updates:__
    * Updated Prefabs:
        * "CustomPointer.prefab"
        * "CustomHand.prefab"
    * Updated Scripts:
        * "CustomGrabber.cs"
            * Added structure to code
            * Added getters and setters for private variables
            * Renamed certain variables and adjusted their scope in the class
            * Added a mechanism to track button and joystick inputs to scripts for common usage outside of the package
            * Modified grip detection, moved grip detection to "CustomGrabbable_GrabVolume.cs" script
            * Added distance grabbing functionality, in tandem with an updated "CustomPointer" script
        * "CustomGrabbable.cs"
            * Changed variable references, in tandem with changes made to "CustomGrabber.cs"
        * "CustomGrabbable_GrabVolume.cs"
            * Completely modified detection system for objects - migrated grip detection from "CustomGrabber.cs" to this script
            * Added complexity to hit detection, including the ability to avoid certain layers and objects in hit detection
            * Relies on a new "HoverCursor" prefab to indicate which game objects are detected in the grab volume
            * Added functionality to either use the "HoverCursor" prefab or simply alter the material color of the grab volume
        * "CustomPointer.cs"
            * Modified update function for pointer detection - now pointer detection and line rendering are adjustable by dropdown in the inspector
            * New dropdowns allow for functionality switching, allowing users to switch the purpose of the pointer between "Target", "Teleport", and "Set_Target"
            * Utilizes new "BezierCurves.cs" script to generate different lines depending on the pointer type