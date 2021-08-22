# EVRA_Grab - Version 2.0.0

This package is intended to allow developers to use grabber functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

## Table of Contents

The following details are mentioned here:
1. What the Package Comes With
    * Dependencies
2. Mechanics
    * Usage with OVRPlayerController
    * Layers and Physics !IMPORTANT
    * Script API
        * EVRA_Grabber
            * Public Variables
            * Serialized Variables in Inspector
            * Public Methods
        * EVRA_Grabbable
            * Public Variables
            * Serialized Variables in Inspector
            * Public Methods
        * EVRA_GrabTrigger
            * Public Variables
            * Public Methods
    * Setting up a Grabbabe Object
    * Setting up a Grabber
3. Event Logic
4. ChangeLog

---

## What the Package Comes With

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __EVRA_Grabber.prefab__
    * OVRControllerPrefab.prefab
* Scripts:
    * __EVRA_Grabber.cs__
    * __EVRA_Grabbable.cs__
    * __EVRA_GrabTrigger.cs__
* Materials:
    * EVRACursor.mat

### Dependencies
This package requires the following packages or dependencies in order to work:
* Oculus Implementations

---

## Mechanics

### Usage with OVRPlayerController

The Prefab `EVRA_Grabber` can be placed in the following location inside of the `OVRPlayerController`:

* __OVRPlayerController__
    * ForwardDirection
    * __OVRCameraRig__
        * __TrackingSpace__
            * LeftEyeAnchor
            * CenterEyeAnchor
            * RightEyeAnchor
            * TrackerAnchor
            * __LeftHandAnchor__
                * __EVRA_Hand__
                    * __*EVRA_Grabber*__
                    * OVRControllerPrefab
                    * EVRA_Pointer
            * __RightHandAnchor__
                * __EVRA_HAND__
                    * __*EVRA_Grabber*__
                    * OVRControllerPrefab
                    * EVRA_Pointer

### Layers and Physics !IMPORTANT

This sub-package relies on the use of layers and physics to enable unique interactions with the EasierVRAssets components.

In particular, this sub-package allows detection of grabbable objects by isolating `EVRA_Grabber` and `EVRA_GrabTrigger` object collisions to the "EasierVRAssets" layer. In other words, `EVRA_Grabber` can only detect collisions with `EVRA_GrabTrigger` if both types are on the "EasierVRAssets" layer.

In order to enable this to work, you must add perform the following:
1. Add a new "EasierVRAssets" layer via the Inspector:
    1. Click on any object currently in the scene's hierarchy.
    2. On the top-right, click the "Layer" dropdown.
    3. Select "Add Layer..."
    4. Add a new layer named "EasierVRAssets".
2. Modify Collision Physics via the project's Collision Matrix:
    1. Edit --> Project Settings --> Physics
    2. Disable all collision with the "EasierVRAssets" layer except for itself. In other words, let objects on the "EasierVRAssets" layer collide only with other objects on the "EasierVRAssets" layer.

### Script API

**NOTE:** The `EVRA_Grabber` prefab has the `EVRA_Grabber.cs` script automatically attached.

When viewing the following scripts inside of the Inspector, you will see the following variables:

#### EVRA_Grabber.cs:
##### Public Variables
|Variable|Type|Description|
|:-|:-|:-|
|`OtherGrabVolume`|`EVRA_Grabber`|Reference to the other `EVRA_Grabber` when holding a grabbable object with both grabbers. is `null` otherwise.|
|`inRange`|`List<EVRA_GrabTrigger>`|List of all `EVRA_GrabTrigger` objects that triggered collision with this object's own collider. Does not include any raycast targets from `EVRA_Pointer`, if set up with a pointer.|
|`grabbed`|`EVRA_GrabTrigger`|Reference to the `EVRA_GrabTrigger` that this grabber is currently holding onto. Is `null` when not grabbing anything.|
##### Serialized Variables in Inspector
|Variable|Type|Description|
|:-|:-|:-|
|`Custom Grabber`|`EVRA_Hand`|Reference to a parent `EVRA_Hand` object.|
|`Pointer`|`EVRA_Pointer`|Reference to an `EVRA_Pointer` object, if wanting to use distance grabbing|
|`Collision Origin`|`Transform`|Reference to a `Transform` that will be the center point for the grabbing. If not set, then the object itself will be used as the `Collision Origin`|
|`GrabType`|`GrabType`|Depending on the setting, the grab mechanism will either be local to the `Collision Origin's` Box Collider, to targets at a distance (via using `EVRA_Pointer`), or both.|
###### Public Methods
|Method|Return Type|Description|
|:-|:-|:-|
|`GrabBegin()`|`void`|Initializes the grabbing mechanism.|
|`GrabEnd()`|`void`|Stops the grabbing mechanism.|
|`AddOtherGrabVolume()`|`void`|Called from one `EVRA_Grabber` to another when two hands begin to hold the same grabbable object. Lets the other grabber know which other grabber is also holding onto the object.|
|`ReoveOtherGrabVolume()`|`void`|Called from one `EVRA_Grabber` to another when one hand lets go of a grabbable object originally held by two hands. Removes the reference to the other grabber.|

#### EVRA_Grabbable.cs:
##### Public Variables
|Variables|Type|Description|
|:-|:-|:-|
|`currentGrabber`|`EVRA_Grabber`|Who is the primary grabber holding this object? ("Primary grabber" == who's maintaining hold over this object and the object is rotating around if two grabbers are holding it?)|
##### Serialized Variables in Inspector
|Variables|Type|Description|
|:-|:-|:-|
|`Current Grabber`|`EVRA_Grabber`|Who is the primary grabber holding this object? ("Primary grabber" == who's maintaining hold over this object and the object is rotating around if two grabbers are holding it?)|
|`GrabbableParent`|`Transform`|The object that is meant to be grabbed - needed if the object that has this script is a child of the object and not the actual grabbable object. If not set, it will set to the object itself.|
|`Triggers`|`List<EVRA_GrabTrigger>`|The list of triggers associated with the grabbable object. All grabbable objects need at least one `EVRA_GrabTrigger` associated with it, whether that be the grabbable object itself or a child.|
|`Should Snap`|`bool`|Should this object snap to match the forward direction of the hand?|
##### Public Methods
|Variables|Return Type|Description|
|:-|:-|:-|
|`GrabBegin(EVRA_Grabber, EVRA_GrabTrigger)`|`void`|Called by a new primary `EVRA_Grabber` when an object is gripped. Notifies the object about which grabber is holding it and from which trigger.|
|`GrabEnd(Vector3, Vector3)`|`void`|Called by the primary `EVRA_Grabber` when an object is let go. Applies linear velocity and angular velocity of the hand to the object to simulate realistic throwing.|
|`GrabEnd()`|`void`|Similar to `GrabEnd(Vector3, Vector3)` except without any linear or angular velocity applied.|
|`SwitchHand(EVRA_Grabber)`|`void`|Called when primary ownership is switched from one `EVRA_Grabber` to another. Modifies the parenting of the object.|

#### EVRA_GrabTrigger.cs:
##### Public Variables
|Variables|Type|Description|
|:-|:-|:-|
|`GrabbableRef`|`EVRA_Grabbable`|Reference to the parent `EVRA_Grabbable` that this trigger is associated with.|
##### Public Methods
|Variables|Return Type|Description|
|:-|:-|:-|
|`Init(EVRA_Grabbable)`|`void`|Called when an `EVRA_Grabbble` parent wants to set itself as `GrabbableRef`.|

### Setting up a Grabbabe Object

To make an object grabbable, you need to modify it beyond simply adding the `EVRA_Grabbable.cs` script onto the object.
1. Attach the `EVRA_Grabbable.cs` to either the object itself or a child within the parent. Modify the public variables:
    1. Set `Grabbable Parent` as the object to be grabbed, whether that be the object itself or the parent object of the object where the `EVRA_Grabbable.cs` is added to.
    2. Set up the number of trigger points on the object. For example, a weapon's handle may have one or two trigger points to allow for two-handed holding.
    3. Determine if the object should snap to the hand's forward position upon being grabbed.
2. Add the necessary trigger points as children of the grabbable object.
    1. For each trigger point you wish to add to an object, create an empty child object with:
        * A `Collider` component set as a trigger
        * A `EVRA_GrabTrigger` component
        * On the "EasierVRAssets" layer !IMPORANT
    2. Add the triggers into the `Triggers` list inside of the object's `EVRA_Grabbable` component.

### Setting up a Grabber

To prep the grabbing mechanism to the player's hand, do the following:
1. Create a new GameObject of your choosing as a child of a `EVRA_Hand`.
    * The GameObject must have a `Collider` that is NOT a trigger
2. Add the `EVRA_Grabber` component to this GameObject.
3. Set the `EVRA_Grabber` onto the "EasierVRAssets" layer !IMPORTANT
4. Set up the `EVRA_Grabber` component: 
    2. Set the parent `EVRA_Hand` as the value of `Custom Grabber`.
    3. Set the `Grab Type` to either "Normal", "Distance", or "Both".
    3. If another `Transform` is intended to be the center of the grabbing mechanism, then set `Collision Origin` to that object.
    4. If using either "Distance" or "Both" types of grabbing, then:
        1. Add either a prefab `EVRA_Pointer.prefab` as a child of the `EVRA_Hand` or manually create a new pointer with a `LineRenderer` and `EVRA_Pointer.cs` component.
        2. Modify the `EVRA_Pointer` component of your pointer to your preferences.
        3. Set that `EVRA_Pointer` as the value of the `EVRA_Grabber`'s `Pointer`.
5. Link Events from `EVRA_Hand` to public methods in `EVRA_Grabber`
    * In the parent `EVRA_Hand`, connect the methods `GrabBegin()` and `GrabEnd()` to any input events.
    * Typically, `GrabBegin()` is linked to the `Grip Trigger Down` event, whereas `GrabEnd()` is linked to the `Grip Trigger Up` event.

---

## Event Logic

### Single-Hand Grabbing

When a user grabs an object, the event logic will flow as such:

1. During normal play, `EVRA_Grabber` will track any `EVRA_GrabTriggers` on the "EasierVRAssets" layer that trigger the collider of `EVRA_Grabber`.
    1. If using either `Distance` or `Both` grab types, `EVRA_Grabber` will also consider any targets of the `EVRA_Pointer` assigned to it via `Pointer`
1. The player pressed the button they've assigned as the GRAB button.
2. Upon press, the method `GrabBegin()` will be invoked.
3. Depending on the settings of `EVRA_Grabber` the grabber will find the closest `EVRA_GrabTrigger` on that object.
    1. If using either `Distance` or `Both` grab types, then the grabber will also consider the target of the `EVRA_Pointer` assigned to it via `Pointer`. This only happens if no `EVRA_GrabTriggers` are within range of the `EVRA_Grabber`.
4. If an `EVRA_GrabTrigger` is detected, the `EVRA_Grabber` modifies the object hierarchy of the scene by setting the grabbable object as a CHILD of the `EVRA_Grabber` object.
5) The `EVRA_Grabber` in question calls the method `GrabBegin()` inside of `EVRA_Grabbable`, telling it that its current grabber is the current `EVRA_Grabber`.

### Multi-Hand Grabbing

When a user uses two hands to grab an object, the event logic will flow as such:

1. The first `EVRA_Grabber` to grab the object will follow standard procedure and set itself as the new parent of the grabbable object.
2. The second `EVRA_Grabber` that attempts to grab the object will produce two possible reactions:
    1. If the second `EVRA_Grabber` grabs the same `EVRA_GrabTrigger` the first `EVRA_Grabber` is currently holding, then the grabbable object switches over to the other hand.
    2. If the second `EVRA_Grabber` grabs a different `EVRA_GrabTrigger` than the first `EVRA_Grabber`, then the grabbable object can be rotated by moving the second `EVRA_Grabber` around the first `EVRA_Grabber`.
3. Letting go of the grabbable object will produce two different effects, depending on which hand lets go of the object:
    1. If the second `EVRA_Grabber` lets go, then the object will still remain as a child of the first `EVRA_Grabber`, but the rotation will still remain as is.
    2. If the first `EVRA_Grabber` lets go, then the object switches parents to the second `EVRA_Grabber` while maintaining the orientation that it currently is.

---

## Changelog

### Version 2.0.0 - Revamped structure, new grabbing mechanism

* __Summary:__
    * Most other functions such as `CustomPointer` and `CustomLocomotion` relocated to other sub-packages and re-labeled.
    * Re-labeled to `EVRA_Grabber`
    * Grabbing functionality simplified for easier use and adjustment of grabbing triggers.
    * Dual-grab functionality added.
* __Additions:__
    * Added new prefabs:
        * "EVRA_Grabber.prefab"
    * Added new scripts:
        * "EVRA_Grabber.cs"
        * "EVRA_Grabbable.cs"
        * "EVRA_GrabTrigger.cs"
    * Added new materials:
        * "EVRA_CustomGrab.mat"

### Version 1.2.0 - Updated hands and pointer, New locomotion method

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

### Version 1.1.0 - New Pointer, Minor Updates & Code Fixes

* Uploaded:
    * Materials/
        * pointerColor.mat
    * Prefabs/
        * CustomPointer.prefab
    * Scripts/
        * CustomPointer.cs
        * __CustomOVRControllerHelper.cs__ (originally "OVRControllerHelper.cs")
* Modified:
    * Prefabs:
        * CustomHand.prefab
            * Added child with "CustomPointer.cs" component attached + pointer activates upon pressing a controller's index trigger
    * Scripts/
        * CustomGrabber.cs
            * Added script to allow functionality with ``CustomPointer`` object attached to the "CustomHand.prefab" object
            * Fixed a bug involving the angular velocity of an object that was just let go: inversed the angular velocity by ``-1`` to allow let-go objects to rotate in the correct orientation.
        * CustomGrabbable.cs
            * Fixed bug in ``GetGrabber()`` function where no controllers inside the ``grabbers`` List would result in ``grabbers[0]`` returning an error.

### Version 1.0.0 - First Upload

* Uploaded:
    * Materials/
        * GrabVol.mat
        * X-Axis.mat
        * Y-Axis.mat
        * Z-Axis.mat
    * Prefabs/
        * CustomHand.prefab
        * XYZ.prefab
    * Scripts/ 
        * CustomGrabbable.cs
        * CustomGrabber.cs
        * CustomGrabber_GrabVolume.cs
        * OVRControllerHelper.cs (imported from Oculus Implementations)
