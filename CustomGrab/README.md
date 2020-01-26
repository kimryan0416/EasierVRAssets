# Custom Grab - Version 1.1.0

This package is intended to allow developers to use grabber functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

## Table of Contents

The following details are mentioned here:
1. What the Package Comes With
    * Dependencies
2. CustomHand Prefab
    * Public Variables for CustomHand
    * CustomHand Behavior
3. CustomGrabbable.cs Script
    * Colliders are Important
    * Snap To Requirements
4. CustomPointer.cs Script
5. ChangeLog

---

## What the Package Comes With

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __CustomHand.prefab__
    * __CustomPointer.prefab__
    * XYZ.prefab
* Scripts:
    * __CustomGrabber.cs__
    * CustomGrabber_GrabVolume.cs
    * __CustomGrabbable.cs__
    * OVRControllerHelper.cs (extracted from the "Oculus Implementations" package)
    * __CustomPointer.cs__
* Materials:
    * GrabVol.mat
    * pointerColor.mat
    * X-Axis.mat
    * Y-Axis.mat
    * Z-Axis.mat
* SampleScenes
    * TestGrabberScene.unity

### Dependencies
This package requires the following packages or dependencies in order to work:
* Oculus Implementations

---

## CustomHand Prefab

__Added: Version 1.0.0__

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
    * __GrabSphere__ (*CustomGrabber_GrabVolume.cs*) - required for collision detection
    * __OVRControllerPrefab__ (_OVRControllerHelper.cs_) - exported from the original "Oculus Implementations" package, renders the controls in the position of the hands in the VR space
    * __customGripTrans__ - Reference Transform for grab position and rotation
        * __XYZ__ - prefab for XYZ: appears when the current hand is in Debug mode
    * __CustomPointer__ (_CustomPointer.cs_) - adds a toggable pointer to your hand

### Public Variables for CustomHand

The only public variables you need to concern yourself with are the ones attached to _CustomGrabber.cs_. These include the following:

|Public Variable|Default Value/Reference|Description|Required?|
|:---|:---|:---|:---|
|Controller Anchor|_null_|Refers to the anchor for the hand the __Customhand__ is supposed to represent.|Yes|
|Grip Trans|customGripTrans|The centerpoint in which all grabbed objects wil snap to if necessary. If set to _null_, "ControllerAnchor" be the reference point for grip position and rotation instead.|Try to leave alone, safe to set to _null_|
|Grab Vol|GrabSphere|The position reference around which collisions are detected for the purposes of hand gripping. If this is set to a different Transform object other than the default __GrabVol__ Transform provided with the hand, please ensure they have the *CustomGrabber_GrabVolume.cs* script attached to it.|Yes|
|OVR Controller Helper|OVRControllerPrefab|Reference to controller 3D render in the virtual world|Yes|
|Custom Pointer|CustomPointer|A laser pointer that is toggable by pressing the index trigger on the controller|No|
|Controller|None|Setting that allows a user or program to determine which hand the controller represents in the game.|Yes|
|ShouldSnap|true|When grabbing objects, this determines if objects should snap to the position and rotation of the "GrabVol" Transform of the grabber.|No|
|DebugToggle|false|If active, will render the sphere mesh for the GrabVol, the XYZ axis indicator for the Grip Trans, and activate the debug mode for any periphery scripts that are attached to the hand|No|

### CustomHand Behavior

By default, the grab functionality tied to the _Hand Trigger_ button of the Oculus Quest controller.
1. When the Grip trigger is pressed down:
    * if _DebugToggle_ is set to _true_, then the _GrabVol_ sphere mesh as well as the XYZ meshes for the Grip Trans are both rendered.
    * If there is something the hand is already grabbing, the loop is ended early.
    * The *CustomGrabber_GrabVolume.cs* script will detect any colliders within range that has the _CustomGrabbable.cs_ script attached.
    * The closest collider will be searched for - if the closest is not _null_, then it will initialize the __GrabBegin()__ function within the closest _CustomGrabbable.cs_ object
2. Else, if the __CustomHand__ is holding any object, the _GrabEnd()_ function is initialized. 

---

## CustomGrabbable.cs Script

__Added: Version 1.0.0__

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

## CustomPointer.cs

__Added: Version 1.1.0__




---

## Changelog

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
