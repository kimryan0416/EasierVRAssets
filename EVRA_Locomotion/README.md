# EVRA_Locomotion - Version 2.0.0

This package is intended to allow developers to use grabber functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

## Table of Contents

The following details are mentioned here:
1. What the Package Comes With
    * Dependencies
2. Script API
    * EVRA_Locomotion
        * Public Variables
        * Serialized Variables in Inspector
        * Public Methods
    * EVRA_LocCursor
        * Public Variables
        * Serialized Variables in Inspector
        * Public Methods
3. Mechanics
    * Usage with OVRPlayerController
    * Setting up EVRA_Locomotion
    * Types of Locomotion Methods
    * Creating a Custom Locomotion Cursor
4. Event Logic
    * Instant, Fade, SmoothDamp Locomotion
    * Translate Locomotion
5. ChangeLog

---

## What the Package Comes With

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __EVRA_LocCursor.prefab__
    * __EVRA_Locomotion.prefab__
* Scripts:
    * __EVRA_Locomotion.cs__
    * __EVRA_LocCursor.cs__
* Materials:
    * LocomotionCursorPosition.mat
    * ColumnGlow.mat

### Dependencies
This package requires the following packages or dependencies in order to work:
* Oculus Implementations

---

## Script API

### EVRA_Locomotion.cs:

#### Public Variables
|Variable|Type|Description|
|:-|:-|:-|
|`screenFader`|`OVRScreenFade`|Reference to the OVR Screen Fade used with the OVR center camera.|
|`heightTolerancePercent`|`float`|If checking the height of the character and space available at the destination, then this is a percentage (0f - 1f) of the character's established height in OVR's `CharacterController` component that is allowed.|
|`isPreparing`|`bool`|If the player is attempting to select a destination for locomotion.|
|`isTeleporting`|`bool`|If the player is currently teleporting.|
|`maxSpeed`|`float`|The maximum speed the translation should move the player, if using **Translate** locomotion.|

#### Serialized Variables in Inspector
|Variable|Type|Description|
|:-|:-|:-|
|`CharacterController`|`CharacterController`|Reference to the OVR `Character Controller` component, normally used in the `OVRPlayerController` prefab.|
|`Player Controller`|`PlayerController`|Reference to the OVR `Player Controller` component, normally used in the `OVERPlayerController` prefab.|
|`Screen Fader`|`OVRScreenFade`|Reference to the OVR Screen Fade used with the OVR center camera.|
|`Pointer`|`EVRA_Pointer`|Reference to an `EVRA_Pointer` that will direct the destination of locomotion.|
|`Locomotion Cursor`|`EVRA_LocCursor`|Reference to a GameObject in the world that acts as the cursor for locomotion and has the component `EVRA_LocCursor`.|
|`Check Height`|`CheckHeight`|Check if the player can fit at the target destination, regarding height?|
|`Height Tolerance Percent`|`float`|If checking the height of the character and space available at the destination, then this is a percentage (0f - 1f) of the character's established height in OVR's `CharacterController` component that is allowed.|
|`Show if Invalid`|`bool`|If checking height, then should the cursor indicate that a teleportation is impossible?|
|`Valid Color`|`Color`|The color to use if a teleportation destination is possible.|
|`Invalid Color`|`Color`|The color to use if a teleportation destination is impossible.|
|`Rbs`|`List<Rigidbody>`|List of rigidbodies that must be set to `Kinematic=true` during teleportation.|
|`Fade Levels`|`AnimationCurve`|Determine the fade behavior of screen darkening/brightening during the `Fade` locomotion type.|
|`Max Speed`|`float`|The maximum speed the translation should move the player, if using **Translate** locomotion.|

#### Public Methods
|Method|Return Type|Description|
|:-|:-|:-|
|`StartDestinationSelection()`|`void`|Called if the player wants to start selecting a destination for locomotion. Must be called before either `Instant()`, `Fade()`, or `SmoothDamp()`.|
|`LocomoteDestinationFromFloor(Vector3)`|`Vector3`|If provided a position, will return a new position with the Y-coordinate increased by half of the player's height.|
|`Instant()`|`void`|Called to execute an instant teleportation. Can be called via `EVRA_Hand` event invoke.|
|`InstantTeleport(Vector3)`|`IEnumerator`|Performs the instant teleportation. Requires a target destination.|
|`Fade()`|`void`|Called to execute a fade locomotion. can be called via `EVRA_Hand` event invoke.|
|`FadeTeleport(Vector3, float)`|`IEnumerator`|Performs the fade locomotion. Requires a target destination and a time for the locomotion to occur.|
|`SmoothDamp()`|`void`|Called to execute a smooth damp locomotion. Can be called via `EVRA_Hand` event invoke.|
|`SmoothDampTeleport(Vector3, float)`|`IEnumerator`|Performs the smooth damp locomotion. Requires a target destination and a time for the locomotion to occur.|
|`StartTranslate(string)`|`void`|Initializes the **Translate** locomotion and maintains the translation until `EndTranslate()` is called.|
|`EndTranslate()`|`void`|Ends the **Translate** locomotion.|
|`TranslateTeleport(Vector3, float, float)`|`void`|Performs the translation locomotion based on a direction, speed, and time elapsed (usually `Time.deltaTime`). Used in the `Update()` loop inside of `EVRA_Locomotion.cs` but also can be publicly called to programmatically move the player in a desired direction at a desired speed.|
|`CheckIfValid()`|`void`|Checks if, when checking for the feasibility of a target destination regarding height, the location is valid or not.|
|`ShowCursor(Vector3)`|`void`|Both makes the cursor visible and relocates it to a target destination. Also adjusts its color to indicate if a locomotion destination is valid, if `showIfValid` is set to `true`.|
|`HideCursor()`|`void`|Hides the locomotion cursor.|
|`ChangePlayerMovement(bool, bool)`|`void`|Changes the player's positioning and rotational ability via joysticks.|
|`ResetPlayerMovement()`|`void`|Resets the player's positioning and rotational ability to defaults.|

### EVRA_LocCursor.cs:

#### Public Variables
|Variables|Type|Description|
|:-|:-|:-|
|`renderers`|`List<Renderer>`|List of renderers attached to the cursor - will be used to adjust the cursor color if `EVRA_Locomotion` is checking the height valdiity of a locomotion destination.|

---

## Mechanics

### Usage with OVRPlayerController

The `EVRA_Locomotion` prefab can be placed anywhere in the scene hierarchy. If attempting to manually add the `EVRA_Locomotion.cs` component to an existing object, it is recommended that it is added at the following location inside of the `OVRPlayerController`:

* __*OVRPlayerController*__

### Setting up EVRA_Locomotion

`EVRA_Locomotion` can be placed anywhere in the scene hierarchy. To set up `EVRA_Locomotion` in a scene, do the following:
1. Add the `EVRA_Locomotion` component to any GameObject of your choosing.
2. Add a Locomotion Cursor in the scene hierarchy by either using the `EVRA_LocCursor` prefab or by creating one manually. To create one manually:
    1. Create a new GameObject without any colliders.
    2. Add the `EVRA_LocCursor` component to this GameObject.
    3. If you wish to set up `EVRA_Locomotion` to change the color of the GameObject when checking for valid locomotion destinations, either:
        * Create children with a `Renderer` component in each, then add them to `EVRA_LocCursor`'s `Renderers` list, or
        * Add a `Renderer` component to the GameObject, then add a reference to it inside of `EVRA_LocCursor`'s `Renderers` list.
2. Set up the variables via the Inspector:
    1. Set `Character Controller`, `Player Controller`, and `Screen Fader` with OVR's `CharacterController`, `PlayerController`, and `OVRScreenFade` respectively from `OVRPlayerController` prefab.
    2. Set up an `EVRA_Pointer` attached to one of the player's hands as the `Pointer`.
    3. Add a reference to the `EVRA_LocCursor` added in Step #2.
    4. If you wish to check the validity of a locomotion destination via height, then:
        1. Modify the `Check Height` value.
        2. Adjust the `Height Tolerance Percent` value, which will represent the percentage of the player's height that would be valid.
    5. If you wish to show if a locomotion destination is valid (i.e. if checking height from Step #4), then set `Show If Invalid` to `true`.
    6. If you wish to show if a locomotion destination is valid, then adjust the colors `Valid Color` and `Invalid Color`.
    7. If you want to apply a Fade locomotion, then you can adjust the fade animation with `Fade Levels`.
3. Adjust the `EVRA_Pointer` linked to `EVRA_Locomotion` to your liking, such as its `Line Type` and `Line Distance`.
4. Add the appropriate methods to the `EVRA_Hand` input events:
    1. Add `StartDestinationSelection()` (if using **Instant**, **Fade**, or **Smooth Damp**) or `StartTranslate()` (if using **Translate**) to a button input event (typically `Index Trigger Down`).
        * If you are using the **Translate** method, you must define the input you are mapping the **Translate** mechanism to. Specifically:
            * Index Trigger == `index`
            * Grip Trigger = `grip`
            * Thumbstick Button = `thumbstickPress`
            * Button One = `one`
            * Button Two = `two`
            * Start Button = `start`
    2. Depending on the type of locomotion desired, add the related method to a button input event (typically `Index Trigger Up`).
        * For **Instant** teleportation, link `Instant()` to the input event.
        * For **Fade** teleportation, link `Fade()` to the input event.
        * For **Smooth Damp** teleportation, link `SmoothDamp()` to the input event.
        * For **Translate** locomotion, link `EndTranslate()` to the input event; this ends the translation.

### Types of Locomotion Methods

Currently, there are **4** ways that `EVRA_Locomotion` supports:

* __Instant:__ The locomotion is instant without delay. The fastest method of locomotion from one position to another.
* __Fade:__ The locomotion teleports the player from one location to another with a fade transition. The Fade animation and timing can be adjusted via the Inspector.
* __SmoothDamp:__ The locomotion teleports the player from one location to another without a fade transition and through a Lerp transition. Without the fade transition, the player sees the locomotion uninterrupted. The transition timing can be adjusted via the inspector.
* __Translate:__ The locomotion moves the player at a constant speed in a direction. If used, the direction of locomotion is based off of the forward direction of `Pointer`.

### Creating a Custom Locomotion Cursor

A locomotion cursor is needed for the Locomotion to work properly. Add a Locomotion Cursor in the scene hierarchy by either using the `EVRA_LocCursor` prefab or by creating one manually. 

To create one manually:
1. Create a new GameObject without any colliders.
2. Add the `EVRA_LocCursor` component to this GameObject.
3. If you wish to set up `EVRA_Locomotion` to change the color of the GameObject when checking for valid locomotion destinations, either:
    * Create children with a `Renderer` component in each, then add them to `EVRA_LocCursor`'s `Renderers` list, or
    * Add a `Renderer` component to the GameObject, then add a reference to it inside of `EVRA_LocCursor`'s `Renderers` list.

---

## Event Logic

### Instant, Fade, SmoothDamp Locomotion
#### Selecting a Locomotion Destination

Before `EVRA_Locomotion` can perform any kind of locomotion, the player needs to select the destination of locomotion. This is done in conjunction with an `EVRA_Pointer` and an `EVRA_LocCursor`.

When the player invokes `StartDestinationSelection()`, the cursor appears at the expected destination. The cursor appears directly below the forward raycast target (or at the end of the forward raycast, if no targets are hit), showing where the player will locomote to. In other words, if the pointer hits an object, the locomotion destination will be the floor right in front of the object.

Whether the line from the pointer is **Straight** or a **Bezier Curve** is irrelevant to the locomotion destination's position; the line's appearance is merely aesthetic in nature. Additionally, the **distance** that the player can locomote is determined by the `Distance` the pointer is allowed to point to. In other words, the pointer controls many aspects of how far the player can locomote.

If `Check Height` is set, then `EVRA_Locomotion` will perform an additional check at the locomotion destination regarding the available height clearance. If the locomotion destination does not have enough clearance (which is determined via a percentage of the player's height, defined by `Height Tolerance Percent` in `EVRA_Locomotion`), then the locomotion will not occur at that destination.

#### Locomoting

Depending on whether the button input event is mapped to `Instant()`, `Fade()`, or `SmoothDamp()`, the animation for the locomotion will be different.

Prior to any of these methods, a locomotion destination must be selected via `EVRA_Pointer` and `EVRA_LocCursor`. The locomotion will **not** occur if:
* No locomotion destination is set
* The player hasn't initialized the selection of the locomotion destination prior (via `StartDestinationSelection()`) before any of these three methods are called.
* The player is already in the middle of teleporting to a location.

During a locomotion, the player will be unable to select a new destination until the locomotion is complete. Once the locomotion is complete, the player will be able to select another destination.

### Translate
#### Initializing the Locomotion

Rather than using a target location, the locomotion is performed by moving the player in a direction at a constant speed.

While there is no acceleration to the locomotion, the speed of the translation has a `Max Speed` that determines the fastest speed the player can move. If the **Translate** locomotion is tied to a **trigger** input such as the index trigger or a grip trigger (or any input that has a `float` value), the speed the player moves is `Max Speed * pressure on trigger`. For example, if the player holds down the index trigger halfway, the player will move at half of the `Max Speed`; whereas if the player presses the trigger down fully, the player will move at `Max Speed`. If the locomotion is tied to a button instead, the `Max Speed` is the only speed available.

To initialize locomotion, `StartTranslate(string)` needs to be called only once.

#### Locomoting

Locomotion will occur constantly once `StartTranslate(string)` is called. The direction of lcoomotion is determined by the forward direction of the `Pointer`. The direction of the locomotion can be changed whenever during the locomotion, allowing the player to change direction by pointing in a different direction if needed.

To end the locomotion, call `EndTranslate()`.

---

## Changelog

### Version 2.1.0 - New "Translate" locomotion mechanism

* __Summary:__
    * A new `Translate` method of locomotion has been added to `EVRA_Locomotion.cs`.

### Version 2.0.0 - Revamped structure, new locomotion mechanism

* __Summary:__
    * Most other functions such as `CustomPointer` and `CustomLocomotion` relocated to other sub-packages and re-labeled.
    * Re-labeled to `EVRA_Grabber`
    * Locomotion functionality simplified to accomodate for new input event invoking in `EVRA_Hand` and for new methods in `EVRA_Pointer`.
* __Additions:__
    * Added new prefabs:
        * "EVRA_LocCursor.prefab"
    * Added new scripts:
        * "EVRA_Locomotion.cs"
        * "EVRA_LocCursor.cs"
    * Added new materials:
        * "LocomotionCursorPosition.mat"

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
