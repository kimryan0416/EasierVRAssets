# EVRA_Pointer - Version 2.0.0

This package is intended to allow developers to use pointer functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

## Table of Contents

The following details are mentioned here:
1. What the Package Comes With
    * Dependencies
2. Script API
    * EVRA_Pointer.cs
        * Public Variables
        * Serialized Variables in Inspector
        * Public Methods
3. Mechanics
    * Usage with OVRPlayerController
    * Layers and Physics !IMPORTANT
    * Setting up a pointer with EVRA_Hand
    * Setting up a pointer with EVRA_Grabber
    * Setting up a pointer with EVRA_Locomotion
4. Event Logic
5. ChangeLog

---

## What the Package Comes With

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __EVRA_Pointer.prefab__
* Scripts:
    * __EVRA_Pointer.cs__
* Materials:
    * EVRA_Pointer.mat

### Dependencies
This package requires the following packages or dependencies in order to work:
* Oculus Implementations

---

## Script API

**NOTE:** The `EVRA_Pointer` prefab has the `EVRA_Pointer.cs` script automatically attached.

### EVRA_Pointer.cs:

#### Public Variables
|Variable|Type|Description|
|:-|:-|:-|
|`LR`|`LineRenderer`|Referene to the Line Renderer used for the pointer.|
|`forwardRaycastTarget`|`Transform`|The transform that the forward-facing raycast hit. If no raycast hit, will return `null`.|
|`forwardRaycastHitPosition`|`Vector3`|The current position of the forward-facing raycast hit. If no raycast hit, will return `Vector3.zero`.|
|`downwardRaycastTarget`|`Transform`|The transform that the downward-facing raycast (starting from the end of the forward-facing raycast) hit. If no raycast hit, will return `null`.|
|`downwardRaycastHitPosition`|`Vector3`|The current position of the downward-facing raycast hit. If no raycast hit, will return `Vector3.zero`.|
|`raycastTarget`|`Transform`|Depending on the `Line Type`, will return `downwardRaycastTarget` for `Bezier Curve` lines and `forwardRaycastTarget` for `Straight` lines.|
|`raycastHitPosition`|`Vector3`|Depending on the `Line Type`, will return `downwardRaycastHitPosition` for `Bezier Curve` lines and `forwardRaycastHitPosition` for `Straight` lines.|
|`alwaysShow`|`bool`|Returns whether the line will always appear when the pointer is activated or only when the pointer detects a raycast hit.|
|`defaultColor`|`Color`|The default color that the line should appear with. Only relevant if a `Material` is used to render the line's color in the `Line Renderer`'s settings in Inspector.|
|`hitColor`|`Color`|The color that the line should appear with if a raycast target is detected. Only relevant if a `Material` is used to render the line's color in the `Line Renderer`'s settings in Inspector.|

#### Serialized Variables in Inspector
|Variable|Type|Description|
|:-|:-|:-|
|`Line Type`|`LineType`|Determines what kind of line the pointer should produce. Either "Straight" or "Bezier Curve"|
|`Distance`|`float`|How far forward the pointer can reach out.|
|`Num Positions`|`int`|How many points will be used to render the line. At least *2* is needed for a straight line, whereas a Bezier Curve needs at least *3*. The more points used, the smoother the Bezier Curve will be.|
|`Always Show`|`bool`|Determines if the line that depicts the pointer will show up whenever the Pointer is activated or only if the pointer hits an object.|
|`Default Color`|`Color`|The line's color by default.|
|`Hit Color`|`Color`|The line's color when the pointer hits an object.|
|`Collision Type`|`CollisionType`|Should the Pointer detect all objects regardless of layer, all objects except for those on the "EasierVRAssets" layer, or only objects with the "EasierVRAssets" layer?|

#### Public Methods
|Variable|Return Type|Description|
|:-|:-|:-|
|`Activate()`|`void`|Turns on the `LineRenderer` component.|
|`Deactivate()`|`void`|Turns off the `LineRenderer` component.|
|`TrulyActivate()`|`void`|Allows the pointer to function with `Activate()` and `Deactivate()`.|
|`TrulyDeactivate`|`void`|Disables the pointer so that the `LineRenderer` is set to inactive, `Activate()` and `Deactivate()` have no effect, and the points used for the `LineRenderer` are set to produce no line at all.|

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
                    * __*EVRA_Pointer*__
            * __RightHandAnchor__
                * __EVRA_HAND__
                    * __*EVRA_Pointer*__

### Layers and Physics !IMPORTANT

This sub-package relies on the use of layers and physics to enable unique interactions with the EasierVRAssets components.

In particular, this sub-package allows for the `EVRA_Pointer` to distinguish different possible targets based on the layers used. The Pointer can be adjusted so that it can:
* target all objects regardless of layer,
* target all objects except those on the "EasierVRAssets" layer, or
* target only objects with the "EasierVRAssets" layer.

In order to enable this to work, you must add perform the following:
1. Add a new "EasierVRAssets" layer via the Inspector:
    1. Click on any object currently in the scene's hierarchy.
    2. On the top-right, click the "Layer" dropdown.
    3. Select "Add Layer..."
    4. Add a new layer named "EasierVRAssets".
2. Modify Collision Physics via the project's Collision Matrix:
    1. Edit --> Project Settings --> Physics
    2. Disable all collision with the "EasierVRAssets" layer except for itself. In other words, let objects on the "EasierVRAssets" layer collide only with other objects on the "EasierVRAssets" layer.

### Setting up a pointer with EVRA_Hand

**NOTE:** A Pointer already comes with the `EVRA_Hand` prefab in `EVRA_CoreElements`.

To add a pointer to an `EVRA_Hand`, follow these steps:
1. Create a new GameObject of your choosing as a child of a `EVRA_Hand`.
    * The GameObject must have a `LineRenderer` component.
2. Add the `EVRA_Pointer` component to this GameObject. If the GameObject doesn't have a `LineRenderer` component, it will auto-add it.
3. Set up the `EVRA_Pointer` component:
    1. Set the `Line Type` to either "Straight" or "Bezier Curve".
    2. Set the `Line Distance`, or the furthest forward distance the pointer can reach.
    3. Set `Num Positions` based on the line type of your choice.
        * If you've chosen "Straight", then you need at least 2 points.
        * If you've chosen "Bezier Curve", then you need at least 3 points.
    4. Adjust `Always Show` based on your preferences.
    5. Set the `Default Color` and `Hit Color` of the line.
    6. Adjust the `Collision Type` of the pointer.
4. Link public methods from `EVRA_Pointer` to input events in `EVRA_Hand`.
    * Typically, the method `Activate()` is linked to the `Index Trigger Down` event.
    * Typically, the method `Deactivate()` is linked to the `Index Trigger Up` event.

### Setting up a pointer with EVRA_Grabber

**NOTE:** It is assumed that you've set up `EVRA_Pointer` to an `EVRA_Hand`.
**NOTE:** It is assumed that the `EVRA_Grabber` is either a "Distance" or "Both" grab type.

To connect the functionality of an `EVRA_Pointer` to the `EVRA_Grabber` of an `EVRA_Hand`, set up the following:
1. Set the `EVRA_Pointer` as the value of the `Pointer` variable in the `EVRA_Grabber` component in the Inspector.

... That's honestly it.

### Setting up a pointer with EVRA_Locomotion

**NOTE:** It is assumed that you've set up `EVRA_Pointer` to an `EVRA_Hand`.
**NOTE:** It is assumed that you've set up `EVRA_Locomotion` with the exception of the pointer.

`EVRA_Locomotion` cannot operate without an `EVRA_Pointer`. To connect the functionality of an `EVRA_Pointer` to `EVRA_Locomotion`, do the following:
* Set the `EVRA_Pointer` as the value of the `Pointer` variable in the `EVRA_Locomotion` component in the Inspector.

... That's honestly it.

---

## Event Logic

The `EVRA_Pointer` will produce a line that, depending on the `Line Type`, is either straight or curved downward. Simultaneously, it will also keep track of any targets it hits, both straight forward and vertically downward from the end point.

The method `Activate()` will turn on the `LineRenderer` component, producing the line that represents a raycast forward. When activated, the `EVRA_Pointer` will simultaneously calculate the forward raycast target and position as well as the downward raycast target and position.

* If the forward raycast hits an object (based on the layering defined with `Collision Type`), the forward raycast target and position will be that object and the hit point of line collision, respectively.
* Consequently, the downward target and position will be directly below the point of collision of the forward raycast.

If you wish the line to only appear if the `EVRA_Pointer` hits a possible target, then adjust `Always Show`. If `EVRA_Pointer` needs to be deactivated, call the method `Deactivate()`. This will turn off the `LineRenderer` and subsequently prevent new raycast targets from being detected.

At some points, sometimes an `EVRA_Pointer` needs to turned completely off regardless if `Activate()` or `Deactivate()` is called. The methods `TrulyActivate()` and `TrulyDeactivate()` will do just that.

---

## Changelog

### Version 2.0.0 - Revamped structure, new grabbing mechanism

* __Summary:__
    * Most other functions such as `CustomPointer` and `CustomLocomotion` relocated to other sub-packages and re-labeled.
    * Re-labeled to `EVRA_Pointer`
    * Streamlined code for detecting forward and downward raycast targets.
* __Additions:__
    * Added new prefabs:
        * "EVRA_Pointer.prefab"
    * Added new scripts:
        * "EVRA_Pointer.cs"
    * Added new materials:
        * "EVRA_Pointer.mat"

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
