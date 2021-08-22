# EasierVRAssets

Hello. My name is Ryan, and this is a guide for the __"EasierVRAssets"__ package. As the developer of this package, I feel that it is strongly within my responsibility to explain what this package is and how to use it.

The __"EasierVRAssets"__ package is a set of scripts and prefabs intended to make life a little easier for developers working with the Oculus Quest in Unity. While working with VR for school projects, I sometimes found that the __"Oculus Implementations"__ package available in the Unity Asset Store was very cumbersome to use and often had bugs in their code. I wanted to create a VR toolkit that would be simpler to use and more efficient in terms of workload on the developer.

In order to get this package working, you need to do several things prior:
1. Make sure that on whatever version of Unity you are working with, that you have the Android DevTools installed in Unity.
2. You must have the “Oculus Implementations” installed in your Unity project. The Oculus package is still required, as many scripts in __"EasierVRAssets"__ still utilize aspects of the code from the __"Oculus Implementations"__ package.

__"EasierVRAssets"__ does not rely on Unity's new XR systems. However, the __"Oculus Implementations"__ package has been slated to be removed from Unity support. For developers who wish to migrate to Unity XR's new system, I have a Github repository that offers tools and tips on how to develop VR apps with the new Unity XR system [here](https://github.com/kimryan0416/EasierXRAssets).

## Setting up your Unity Project

In order to get this project to work, you must install the __"Oculus Implementations"__ first, then adjust the settings of your Unity project to output properly for the Oculus.

To install the __"Oculus Implementations"__ package, just follow these instructions:

1. Navigate to the Unity Asset Store in Unity
2. Search for _"Oculus"_ – you will most likely see the __"Oculus Implementations"__ package as the first item that appears.
3. Click `Import` – it will take some time to install into your Unity project.

You must also ensure that the settings behind Unity are optimized for use of the Oculus scripts.
1. Open your Build Settings (`File` > `Build Settings`), and then ensure that you are outputting your projects for Android (`Platform` > `Android`).
2. Open "Player Settings" and navigate to "Player" (`Player Settings` > `Player`)
3. Inside "XR Settings", in "Virtual Reality SDKs", click the `+` button and add the "Oculus" package if you haven’t already.
3. Inside "Other Settings", do the following:
    1. Delete `Vulkan` from "Graphic APIs" – this is no longer needed
    1. Set "Minimum API Level" to `Android 4.4 "Kitkat"`
4. Still within "Player Settings", navigate to "Time" on the left side. Then, set the "Fixed Timestep" value to `1/90` or `0.01111` – this speeds up your game by a significant amount

To install the __"EasierVRAssets"__ package, simply clone this repo into your local computer, and then add the package to your Unity Project. Unity should automatically import all scripts and assets onto the computer from here.

## Contents

The contents of the __"EasierVRAssets"__ consist of the following:

* _EasierVRAssets/_
    * _README.md_: this file.
    * _ChangeLog.md_: Contains the list of changes made to this package.
    * _EVRA\_CoreElements_: All scripts, prefabs, and materials needed to run a bare-bones VR implementation.
    * _EVRA\_Grab/_: Sub-package for needed to allow for grabbing objects with hands.
        * _README.md_: A summary of the EVRA_Grab sub-package.
        * _Materials/_: The materials used by this sub-package.
        * _Prefabs/_: Prefabs created as either examples or convenient usage in Unity.
        * _Scripts/_: Scripts used in this sub-package.
        * _Deprecated/_: All deprecated scripts, prefabs, materials, and scenes that came with previous versions of this sub-package.
    * _EVRA\_Pointer_: Sub-package needed to allow for line pointers. Can be used with other sub-packages (ex. distance grabbing, locomotion).
        * _README.md_: A summary for the EVRA_Pointer sub-package
        * _Materials/_: The materials used by this sub-package
        * _Prefabs/_: Prefabs created as either examples or convenient usage in Unity.
        * _Scripts/_: Scripts used in this sub-package.
    * _EVRA\_Locomotion_: Sub-package needed to allow for locomotion outside of the default joystick movement.
        * _README.md_: A summary for the EVRA_Locomotion sub-package
        * _Materials/_: The materials used by this sub-package.
        * _Prefabs/_: Prefabs created as either examples or convenient usage in Unity.
        * _Scripts/_: Scripts used in this sub-package.
        * _Deprecated/_: All deprecated scripts, prefabs, materials, and scenes that came with previous versions of this sub-package.
    * _SampleScenes/_: Sample Unity scenes as examples of how to use the EasierVRAssets package.

---

## Layers and Physics !IMPORTANT

This sub-package relies on the use of layers and physics to enable unique interactions with the EasierVRAssets components.

In order to enable this to work, you must add perform the following:
1. Add a new "EasierVRAssets" layer via the Inspector:
    1. Click on any object currently in the scene's hierarchy.
    2. On the top-right, click the "Layer" dropdown.
    3. Select "Add Layer..."
    4. Add a new layer named "EasierVRAssets".
2. Modify Collision Physics via the project's Collision Matrix:
    1. Edit --> Project Settings --> Physics
    2. Disable all collision with the "EasierVRAssets" layer except for itself. In other words, let objects on the "EasierVRAssets" layer collide only with other objects on the "EasierVRAssets" layer.

---

For more information on how to utilize these subpackages, please read their respective README.md files. Each subpackage in this repo contains a README file that explains the nuances of each subpackage, including how they work and what public variables are most important when integrating these packages and prefabs into your own project.

Happy hunting! - Ryan Kim