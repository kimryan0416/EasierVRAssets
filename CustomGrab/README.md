# Custom Grab - Version 1.2.0

This package is intended to allow developers to use grabber functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

## Table of Contents

The following details are mentioned here:
1. What the Package Comes With
    * Dependencies
    * Setup (IMPORTANT)
2. CustomHand Prefab
    * Public Variables for CustomHand
    * Variables Usable by Your Own Scripts
    * CustomHand Behavior
3. CustomGrabbable.cs Script
    * Colliders are Important
    * Snap To Requirements
4. CustomGrabber_GrabVolume.cs
    * Public Variables to worry about
    * Variables Usable by Your Own Scripts
5. CustomPointer Prefab
    * Public Variables for CustomPointer
    * Variables Usable by Your Own Scripts
    * Locomotion Necessities
6. ChangeLog

---

## What the Package Comes With

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __CustomHand.prefab__
    * __CustomPointer.prefab__
    * LocomotionIndicator.prefab
    * HoverCursor.prefab
    * XYZ.prefab
* Scripts:
    * __CustomGrabber.cs__
    * __CustomGrabber_GrabVolume.cs__
    * __CustomGrabbable.cs__
    * __CustomPointer.cs__
    * __CustomLocomotion.cs__
    * HoverCursor.cs
    * BezierCurves.cs
    * CommonFunctions.cs
    * ExternalCollider.cs
* Materials:
    * GrabVol.mat
    * pointerColor.mat
    * X-Axis.mat
    * Y-Axis.mat
    * Z-Axis.mat
    * ColumnGlow.mat
    * ErrorGlow.mat
    * HoverCursor.mat
* SampleScenes
    * TestGrabberScene.unity
    * GrabAndLocomotion.unity

### Dependencies
This package requires the following packages or dependencies in order to work:
* Oculus Implementations

### Setup (IMPORTANT)

Prior to doing anything with this subpackage, you need to add __2__ layers to your Unity project:

* `AvoidHover` - prevents any objects from being detected by any `CustomGrabber_GrabVolume.cs` script.
* `Locomotion` - used when one of your pointers is set to `Teleport` mode.

---

## CustomHand Prefab

__Added: Version 1.0.0__
__Most Updated Ver: Version 1.2.0__

The __CustomHand__ prefab requires that you have an _OVRPlayerController_ present in the scene. In particular, the _OVRPlayerController_ must be structure like so in the hierarchy:

* OVRPlayerController
    * ForwardDirection
    * OVRCameraRig
        * TrackingSpace
            * LeftEyeAnchor
            * CenterEyeAnchor
            * RightEyeAnchor
            * TrackerAnchor
            * LeftHandAnchor
                * LeftControllerAnchor
                * __CustomHand__
            * RightHandAnchor
                * RightControllerAcnhor
                * __CustomHand__

The __CustomHand__ prefab is structured like so:

* CustomHand (_CustomGrabber.cs_]
    * __GrabSphere__ (*CustomGrabber_GrabVolume.cs*) - required for collision detection for the grip
    * __OVRControllerPrefab__ (_OVRControllerHelper.cs_) - exported from the original "Oculus Implementations" package, renders the controls in the position of the hands in the VR space
    * __customGripTrans__ - Reference Transform for grab position and rotation <- DEPRECATED
        * __XYZ__ - prefab for XYZ: appears when the current hand is in Debug mode
    * __CustomPointer__ (_CustomPointer.cs_) - adds a toggable pointer to your hand

### Public Variables for CustomHand

The only public variables you need to concern yourself with are the ones attached to _CustomGrabber.cs_. These include the following:

|Public Variable|Default Value/Reference|Description|Required?|
|:---|:---|:---|:---|
|Controller Anchor|`null`|(NOW DEPRECATED) Refers to the anchor for the hand the __Customhand__ is supposed to represent.|No|
|OVR Controller|`None`|Setting that allows a user or program to determine which hand the controller represents in the game.|Yes|
|Grip Detector|`GrabSphere`|Reference to a "CustomGrabber_GrabVolume.cs" object that acts as the detection system for gripping objects by hand. Necessary if you wish for grabbing to be allowed by conventional grips on the Oculus Quest controller|No|
|Tooltip Detector|`null`|(TO BE IMPLEMENTED) Reference to a "CustomGrabber_GrabVolume.cs" object that acts as the detection system for a tooltip attached to the front of the Oculus controller.|No|
|Pointer|`CustomPointer`|A laser pointer that is toggable by pressing the index trigger on the controller. Controls distance grabbing or locomotion, depending on what settings you put for the "CustomPointer"|No|
|Grab Destination|`GrabSphere`|Reference to the transform where grabbed objects should snap to, if "ShouldSnap" is activated to "true"|Yes, if "Grip Detector" and "ShouldSnap" are both active|
|Grab Type|`Grip`|The grab metaphor for grabbign items. If set to "Grip", a "Grip Detector" is required; if set to "Distance", a Pointer set to "Target" is required. "Both" naturally requires both a "Grip Detector" and "Pointer"|Yes|
|OVR Controller Helper|`OVRControllerPrefab`|Reference to controller 3D render in the virtual world|Yes|
|Should Snap|`true`|When grabbing objects, this determines if objects should snap to the position and rotation of the "GrabVol" Transform of the grabber.|No|

### Variables Usable by Your Own Scripts

Some of the variables available in the `CustomGrabber` script are also available for use externally, if you wish:

* `CustomGrabber.OVRController` (OVRInput.Controller) : Get the OVR controller associated with this hand
* `CustomGrabber.pointer` (CustomPointer) : get the reference to the "CustomPointer" attached to this hand, if it exists
* `CustomGrabber.grabDestination` (Transform) : Get a reference to where objects should snap to, if `Should Snap` is active in the script. Mostly used by `CustomGrabbble.cs`
* `CustomGrabber.grabbedObject` (CustomGrabbable) : Get a reference to whatever the hand is currently holding, if it is indeed holding something
* `CustomGrabber.inputTimes` (Dictionary<string, float>) : A list of timings (from Time.time) when buttons are pressed on the controller. Only keeps this value as the button is held down, resets to `-1` if the button is let go. Can be useful for detecting at what point a button has been pressed down in real time.
* `CustomGrabber.inputDowns` (Dictionary<string, float>) : A list of if buttons are pressed on the controller DURING THAT UPDATE CYCLE. Since this dictionary updates every Update cycle, it is useful for detecting if buttons have been pressed in that moment
* `CustomGrabber.thumbDirection` (Vector2) : The current direction of the joystick on the controller. The joystick is mapped to 2D grid between -1 and 1 on both axes. This is the raw mapping of that joystick, not adjusted for any angle discrepancies (which are accounted for inside `CustomGrabber.thumbAngle`)
* `CustomGrabber.thumbAngle` (Vector2) : The current angle of the joystick on the controller - 0 degrees = West, 180 degrees = East.
    * Due to Oculus's weird programming, joystick directions are offset by about 5 degrees depending on the controller - `CustomGrabber.thumbAngle` adjusts for that by first calculating the angle from `CustomGrabber.thumbDirection` and then adjusting the value by 5 degrees depending on the controller. In this respect, the joysticks on both controllers, if pressed in the same direction, will output the same angle regardless of which controller the joysticks are attached to.

### CustomHand Behavior

By default, the grab functionality tied to the _Hand Trigger_ button of the Oculus Quest controller. For distance grabbing, the grab functionality is tied to the _Index Trigger_ button of the Oculus Quest controller.

The `Update()` function inside `CustomGrabber.cs` runs the following functions in this order:

1. `CheckInputs()` - updates `inputDowns`, `inputTimes`, `thumbDirection`, and `thumbAngle`
2. `CheckPointer()` - enables or disables the pointer on the hand, if it is referneced in the script, based on if the index trigger is held down or not
3. `CheckGrip()` - Controls the grabbing metaphor for the hand based on whether the hand grip trigger is pressed down or let go and on the hand's `Grab Type`.

In the `CheckGrip()` function in particular, this is what happens:

1. If the hand's `Grab Type` is set to `Grip` or `Both` and there is no `Grip Detector` set, then the function simply ends there
2. If the grip trigger is PRESSED down and the is nothing that the hand is holding now:
    1. Depending on the hand's `Grab Type`, detection of the closest object is either found or results in `null`.
    2. If an object is detected by the `Grip Detector` or `Pointer` (depending on the hand's `Grab Type`), then we grab it
3. If the grip trigger is NOT HELD DOWN and we are currently grabbing something, it simply lets go of the object

---

## CustomGrabbable.cs Script

__Added: Version 1.0.0__
__Most Updated Ver: 1.2.0__

When you want an object to be grabbable in your scene, you must attach the __customGrabbable.cs__ script as a component of the object. 

The __CustomGrabbable.cs__ script usually can be left alone when being added to a grabbable object, as long as the grabbable object as a Collider attached to it. When this component is attached, the __CustomHand__ prefab (or any hands that have the __CustomGrabbable.cs__ script attached) will take care of any physics and collision detection. This includes letting go of objects as well.

### Colliders are Important

Colliders on your grabbable object determine where your grab detection will be located at. This is hugely important, especially for when you have grabbable objects that have multiple handles or places that you wish for the object to be grabbed at.

Please ensure that you set up your colliders properly to reflect where on your grabbable object you wish for the hand to detect the grabbable object.

### Snap To Requirements

If you wish to have the grabbable object snap to a __customGrabber.cs__ object in a particular orientation and position, then your grabbable object requires _Transform_ references that act as snap points. These snap points can be children of your grabbable object, but they are referenced via the __SnapTransforms__ public List within the __customGrabbable.cs__ script.

When a __CustomHand__ (or any hand that contains the _CustomGrabber.cs_ script) has their _ShouldSnap_ boolean active, then any objects with the _CustomGrabbable.cs_ script that is closest to the hand will snap to the hand with respect to the orientation and position of the closest Transform object referenced inside the _CustomGrabber.cs_'s __SnapTransforms__ List.

For example, if you have a weapon in your scene that is set up like the following:
* __PlayerWeapon__ (_CustomGrabbable.cs_)
    * Shaft
    * GlowingOrb
    * __Handle__

Inside the __PlayerWeapon__ object's _CustomGrabbable.cs_ variables, you can reference the GameObject __Handle__ as one of the snap points. When a _CustomGrabber.cs_ object finds the __PlayerWeapon__ object, then grabbing physics will automatically orient the object in the grabber with reference to __Handle__'s position and orientation.

---

## CustomGrabber_GrabVolume.cs

__Added: Version 1.0.0__
__Most Updated Ver: 1.2.0__

This script was originally added in Version 1.0.0, but Version 1.2.0 adds significant updates to how this function operates in the grand scheme of things. This script can be added to any gameObject as a method of hit detection, and multiple objects with the `CustomGrabber_GrabVolume.cs` attached can be added to the Custom Hand

### Public Variables to worry about

You need to pay special close attention to the following variables for this to work properly:

|Public Variable|Default Value/Reference|Description|Required?|
|:---|:---|:---|:---|
|Collision Radius|`0.5f`|The radius at which hit detection is detected from a center point - the value is in world coordinates, never local coordinates (ex. 0.5f = 0.5 meters in world-space)|Yes|
|Hover Color|`Color.yellow`|The color that should show up when a collision is detected. If a `Hover Cursor prefab` is set as a reference, it will adjust the color of the Hover Cursor that appears around the collided object - else, this adjusts the color of the object this script is attached to|Yes|
|Collision Origin|`null`|The Transform where the collision center should be. If set to `null`, it refers its own GameObject's transform|No|
|Hover Cursor Prefab|`null`|A reference to the "Hover Cursor" prefab - instantiates a new Hover Cursor for every object detected in collision. Functionality still works i no prefab is referenced|No|
|Should Start On Run|`true`|Since the function initializes with an `Init()` function, this just makes it so that the collision detection can start immediately upon runtime start|No|
|Can Collide|`false`|A boolean that controls whether any `Colliders` attached to this GameObject or any children GameObjects can affect other Colliders in the scene. Updates colliders in real time|No|

### Variables Usable by Your Own Scripts

Similar to `CustomGrabber.cs`, you can also access certain values from external scripts:

* `CustomGrabber_GrabVolume.inRange` (List<Transform>) : The list of all Transforms in range of the hit collision center - sorted in ascending order of distance from the center
* `CustomGrabber_GrabVolume.closestInRange` (Transform) : Get the closest Transform in range of the hit collision center (aka the first value from `inRange`)
* `CustomGrabber_GrabVolume.canCollide` (bool) : A toggle for whether this GameObject and any children GameObjects can collide with other objects in the world.

---

## CustomPointer.cs

__Added: Version 1.1.0__
__Most Updated Ver: Version 1.2.0__

The __CustomPointer__ is intended to be a prefab that does the following:
* Performs a raycast from either itself or from a different ``Transform`` component of a different ``GameObject`` up to a distance defined by ``Raycast Distance``
* When toggled, will create a line renders between the origin defined above and whatever the Raycast hits (or, if no such hit is detected, to a distance defined by ``Raycast Distance`` in front of the CustomPointer's origin point).

The useful aspect about the __CustomPointer__ is that it performs raycast measurements, such that if you require any raycast functionality then the __CustomPointer__ can do that alongside rendering a laser - this raycast hit object is also accessibly publicly, so your object can simply reference the __CustomPointer.cs__'s public ``raycastTarget`` variable if needed.

### Public Variables for CustomPointer

|Public Variable|Default Value/Reference|Description|Required?|
|:---|:---|:---|:---|
|Grabber|`null`|A reference to the `CustomGrabber` that controls this pointer|Yes|
|Pointer Type|`Target`|Determines the purpose of the pointer. If set to `Target`, it acts as any pointer you might expect. If set to `Teleport`, it alters the functionality of the pointer to allow for locomotion, in tandem with the `CustomLocomotion` script. If set to `Set_Target`, it will always point to a target object regardless of what buttons are pressed|Yes|
|Laser Type|`Laser`|The way the laser is rendered and detects objects. `Laser` = a straight laser, `Parabolic` = points downward in a parabola.|Yes|
|Raycast Origin|`null`|A reference to the starting point of the raycast. If set to `null`, it refers to itself as the raycast origin|No|
|Raycast Distance|`100f`|In world coordinates, how far the laser can reach out|Yes|
|Line Color|`Color.yellow`|The color of the laser. If the `Hover Cursor` prefab is referenced, any hover cursors instantiated by a hit from the raycast also adapts to this color|Yes|
|Detect Only When Active|`true`|If set to true, detection of any objects is only allowed if the line renderer is enabled. Otherwise, detection is automatic regardless if a line is being rendered or not|Yes|
|Num Points|`30`|The number of points that the line renderer uses to render the line. The lesser this number, the more jaggy the line will appear if using `Parabolic` as the laser type|Yes|
|Hover Cursor|`null`|A reference to the `Hover Cursor` prefab. If referenced, the object that is hit by the raycaster will have a hover cursor attached to it. If `null`, no such cursor will appear|No|
|Debug Mode|`false`|(DEPRECATED)|No|
|XYZ|`XYZ` prefab|(DEPRECATED)|No|

### Variables Usable by Your Own Scripts

Like with `CustomGrabber`, certain variables are accessible outside of the scope of this script and can be used for other purposes.

* `CustomPointer.pointerDest` (Vector3) : Get the hit position of the raycaster, if it hit anything
* `CustomPointer.raycastTarget` (GameObject) : Get or Set the target of the raycast or line
* `CustomPointer.raycastOrigin` (Transform) : Get a reference to where the raycast is originating from
* `CustomPointer.raycastDistance` (float) : Get or Set the distance the raycast can travel
* `CustomPointer.lineColor` (Color) : Get or Set the color of the line and any hover cursor generated by the Custom Pointer
* `CustomPointer.detectOnlyWhenActivated` (bool) : Get or Set the detection whether it should be tied to the Line Renderer's status or not
* `CustomPointer.numPoints` (int) : Get or Set the number of points the line renderer uses to render the line
* `CustomPointer.locomotionPosition` (Vector3) : Get or Set the destination of where users must teleport to - only useful if the pointer's `Pointer Type` is set to `Teleport`
* `CustomPointer.instantiatedHoverCursor` (HoverCursor) : get a reference to the hover cursor generated by the pointer
* `CustomPointer.isActive` (bool) : Get or Set a toggle that determines whether this Custom Pointer should even run or not.

### Locomotion Necessities

If you wish for locomotion to work in your Unity project, some additional steps need to be taken.

1. You must have the `CustomLocomotion.cs` script attached to any object in the Unity hierarchy.
2. You must have a `LocomotionIndicator` prefab instantiated in the world prior to runtime
3. In the inspector, assign the following values to your `CustomLocomotion.cs` component:
    1. Character Controller: your `OVRPlayerController` in the scene
    2. Player Controller: your `OVRPlayerController` in the scene
    3. Screen Fader: Your `CenterEyeAnchor` located inside your `OVRPlayerController`'s Tracking Space
    4. Custom Grabber: the `CustomGrabber` that you wish to direct locomotion with - this script will automatically adjust that `CustomGrabber`'s pointer so that it's prepared for teleportation
    5. Locomotion Trigger: `Primary Index Trigger`
    6. Valid Locomotion Material: `ColumnGlow.mat`
    7. Invalid Locomotion Material: `ErrorGlow.mat`
    8. Rbs: any objects attached to your OVRPlayerController that has a Rigidbody component attached to it
    9. Locomotion type: either `Fade` or `Instant` - your choice
    10. Max Distance: your preference, also adjust the `CustomPointer.raycastDistance` value automatically
    11. Transition Time: the time for transitioning, if using the `Fade` method of teleportation
    12. Fade Levels: Leave as is, or adjust to control the way the fader works if using the `Fade` method of teleportation

---

## Changelog

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
