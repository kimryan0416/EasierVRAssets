# Custom Grab

This package is intended to allow developers to use grabber functionality with Oculus-oriented projects without relying on the __Oculus Implementation__'s _OVRGrabber_ and _OVRGrabbable_ components.

This package comes with the following (The __bolded__ ones require your upmost attention):
* Prefabs:
    * __CustomHand__
    * XYZ
* Scripts:
    * __CustomGrabber.cs__
    * CustomGrabber_GrabVolume.cs
    * __CustomGrabbable.cs__
    * OVRControllerHelper.cs (extracted from the "Oculus Implementations" package)
* Materials:
    * GrabVol
    * X-Axis
    * Y-Axis
    * Z-Axis

## CustomHand prefab

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
    * GrabSphere (*CustomGrabber_GrabVolume.cs*) - required for collision detection
    * OVRControllerPrefab (_OVRControllerHelper.cs_) - exported from the original "Oculus Implementations" package, renders the controls in the position of the hands in the VR space
    * customGripTrans - Reference centter point for grab position and rotation
        * XYZ - prefab for XYZ: appears when the current hand is in Debug mode

The only public variables you need to concern yourself with are the ones attached to _CustomGrabber.cs_. These include the following:

|Public Variable|Default Value/Reference|Description|Required?|
|:---|:---|:---|:---|
|Controller Anchor|_null_|Refers to the anchor for the hand the __Customhand__ is supposed to represent.|Yes|
|Grip Trans|customGripTrans|The centerpoint in which all grabbed objects wil snap to if necessary. If set to _null_, "ControllerAnchor" be the reference point for grip position and rotation instead.|Try to leave alone, safe to set to _null_|
|Grab Vol|GrabSphere|The position reference around which collisions are detected for the purposes of hand gripping.|Yes|
|OVR Controller Helper|OVRControllerPrefab|Reference to controller 3D render in the virtual world|Yes|
|Controller|None|Setting that allows a user or program to determine which hand the controller represents in the game.|Yes|
|ShouldSnap|true|When grabbing objects, this determines if objects should snap to the position and rotation of the "GrabVol" Transform of the grabber.|No|
|DebugToggle|false|If active, will render the sphere mesh for the GrabVol, the XYZ axis indicator for the Grip Trans, and activate the debug mode for any periphery scripts that are attached to the hand|No|




