# EasierVRAssets - Change Log (Most Recent: V2.1.0)

## Version 2.1.0

### Major Updates:

* A new `Translate` method of locomotion has been added to `EVRA_Locomotion.cs`.

## Version 2.0.0

### Major Updates:

* `CustomGrab` sub-package renamed to `EVRA_Grab`.
* New sub-packages; pulled out of `EVRA_Grab` and placed into their own sub-packages, to better componentize the functions of EasierVRAssets:
    * `EVRA_CoreElements` - Contains core scripts and prefabs needed to run a bare-bones VR implementation
    * `EVRA_Pointer` - sub-package needed to allow for line pointers. Can be used with other sub-packages (ex. distance grabbing, locomotion).
    * `EVRA_Locomotion` - sub-package needed to allow for locomotion outside of the default joystick movement.
* Kinda-new sub-packages; from a build between V1.2.0 and V2.0.0 that needs to be re-vamped:
    * `CustomDialogue` - sub-package to allow for dialogue systems in VR
* `CustomHand` component now altered to `EVRA_Hand` + grabbing functionality moved to `EVRA_Grab`.

### _"EVRA\_Grab"_ Changes:

* __Summary:__
    * Input Events added: public functions from other scripts and components can be invoked by particular button inputs and manually added via Inspector for easy use.
    * Streamlined code process for grabbing objects, simplifying code.
    * Added dual-hand grabbing functionality
* __Updates:__
    * The grabbing functionality, originally stored within the `CustomHand` component from V1.2.0, has been moved into a separate subpackage `EVRA_Grab`.
    * Event Handling allows for public functions from separate scripts/components to be invoked via inputs from the hand's respective controller.
        * The inspector allows for easy drag-and-drop manipulation of events attached to each button press and release.
    * Streamline Code Process:
        * Older components `ExternalCollider` and `HoverCursor` now deprecated.
        * Objects that the player should be able to grab needs the following scripts:
            * `EVRA_Grabbable` - attach to the object itself
            * `EVRA_GrabTrigger` - attach to empty children inside of the object, attach a `Collider` to them
        * `EVRA_Grabber` will detect for the closest `GrabTrigger`, `GrabTrigger` will inform `EVRA_Grabber` about the parent `EVRA_Grabbable` component, `EVRA_Grabber` performs the necessary functions to make the object move around with the hand.
    * Dual-Hand Grabbing:
        * When a user grabs an object, it will move around with the hand that grabbed it, like normal. When a second `EVRA_Grabber` attempts to grab the object at a different trigger point, then the object can be rotated while still being grabbed by the first grabber, meaning objects can be rotated while simultaneously being grabbed by two objects.
        * When a user grabs an already-held object at the same trigger point, then the parenting will be switched over to the new grabber.
        * When a user is holding an object with Grabbers A (first grabber) and B (second grabber) and A lets go, then B becomes the new primary grabber.
        * When a user is holding an object with Grabbers A (first grabber) and b (second grabber) and B lets go, then the orientation of the object is maintained.



---

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