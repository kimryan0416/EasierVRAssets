# EasierVRAssets (Latest: V2.1.1 Unofficial, Stable: V2.0.0)

Hello. My name is Ryan, and this is a guide for the __"EasierVRAssets"__ package. As the developer of this package, I feel that it is strongly within my responsibility to explain what this package is and how to use it.

The __"EasierVRAssets"__ package is a set of scripts and prefabs intended to make life a little easier for developers working with the Oculus Quest in Unity. While working with VR for school projects, I sometimes found that the __"Oculus Implementations"__ package available in the Unity Asset Store was very cumbersome to use and often had bugs in their code. I wanted to create a VR toolkit that would be simpler to use and more efficient in terms of workload on the developer.

In order to get this package working, you need to do several things prior:
1. Make sure that on whatever version of Unity you are working with, that you have the Android DevTools installed in Unity.
2. You must have the “Oculus Integragion” installed in your Unity project. This Oculus package is still required, as many scripts in __"EasierVRAssets"__ still utilize aspects of the OVR scripts from the __"Oculus Integration"__ package.

__"EasierVRAssets"__ does not rely on Unity's new XR systems. For developers who wish to migrate to Unity XR's new system, I have a Github repository that offers tools and tips on how to develop VR apps with the new Unity XR system [here](https://github.com/kimryan0416/EasierXRAssets).

## Setting up your Unity Project

**Topics:**
* Simplified Oculus Integration Setup
    * Setting Up Android Environment
    * Importing Oculus Integration
    * Setting Up Oculus Integration
* Setting Up EasierVRAssets

In order to get this project to work, you must install the __"Oculus Integration"__ first, then adjust the settings of your Unity project to output properly for the Oculus. To install the __"Oculus Integration"__ package, I highly recommend that you refer to existing guides on how to do so. The guides that tend to work best on a fresh install of Unity are listed below. I'll also highlight the most important steps that worked for me with a fresh install of Unity.

* [Dilmer Valecillos's overall guide to Unity documentation](https://www.youtube.com/watch?v=YwFXQeBmxZ4)
* [Oculus's documentation on Unity implementations](https://developer.oculus.com/documentation/unity/unity-gs-overview/)

### Simplified Oculus Integration Setup

#### Setting Up Android Environment
1. Open Project in Unity
2. `File` --> `Build Settings`
3. On `Platform`, select `Android`
4. Click `Switch Platform`
5. Close the `Build Settings` window - it will be needed later, but not immediately

#### Importing Oculus Integration
1. Click `Asset Store` tab in the main editor panel
2. Click `Search Online`
3. Search for "Oculus" in the search bar
4. Select "Oculus Integration" from the search results
5. Either download (if it's your first time) or `Open in Unity` (if you already installed the integrations package before)
6. `Window` --> `Package Manager`
7. Select `Oculus Integration`, import the package. Make sure to update it, if possible. 
    * NOTE: BE CAREFUL ABOUT IF YOU'RE UPDATING WITH A PRE-EXISTING PROJECT WHEN UPDATING.
8. In the new window that pops up, make sure to import everything into your project.

#### Setting Up Oculus Integration
1. `File` --> `Build Settings`
2. `Player Settings` (in the bottom-left corner)
3. (Optional) In the top-right, modify the "Company Name", "Product Name", and "Version"
4. `Other Settings` tab
5. `Minimum API Level` - set to `Android 6.0 Marshmallow`
6. `Target API Level` - set to `Automatic (highest installed)`
7. `Install Location` - set to `Automatic`

8. On the left, `XR Plug-in Management`
9. (If you haven't already) `Install XR Plug-in Management`
10. On the "Android" tab (right tab), select `Oculus`
11. On the left, click the new dropdown under `XR Plug-in Management`
12. Select `Oculus`

13. On the left, `Player`
14. `Other Settings` tab
15. `Color Space` - change from `Gamma` to `Linear`
16. `Graphics API` - Remove `Vulkan`, set `OpenGLES 3` as the topmost API
17. `Multithreaded Rendering` - checkmark it

### Setting Up EasierVRAssets 

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