# CODING TEST DONE FOR COMPANY XYZ (NOT REAL NAME)

* **Position**: Senior Unity Programmer.
* **Code Test Requirements**: [Code Test Design Requirements](https://github.com/EstebanGameDevelopment/CodingTestXYZ/blob/6c4a7b6e984e04b49c02525b988b947808808231/Documents/Coding%20Test%20Company%20XYZ%20(en).pdf)
* **Time to complete**: 3.5 days
* **Video Footage Project Completed**: https://youtu.be/no-AmHF3iIw

## ADDITIONAL IMPROMENTS DONE (not part of test, just for the sake of using my tools):

* **Multiplayer (1 day)**
	* Use pre-processor constant ENABLE_NETWORKING to enable the networking code. Without it you don't need to have the networking library (https://github.com/EstebanGameDevelopment/yourvrxp-networking).
	* Use pre-processor constant ENABLE_MIRROR for Mirror SDK networking.
	* Use pre-processor cosntant ENABLE_PHOTON for Photon SDK networking (Photon SDK not included, requires the purchase of Photon library)
* **Support Open XR (0.5 days)**
	* Use pre-processor constant ENABLE_OPENXR for OpenXR.
	* Use pre-processor constant ENABLE_OCULUS for Oculus/Meta.
* **Support Mobile (0.5 days)**
	* Do not use neither of the pre-processor constants ENABLE_OPENXR nor ENABLE_OCULUS
	
* **CURRENT CONFIGURATION**
	* Aside from cloning this repository, you should clone the submodules:
		* Util tools: https://github.com/EstebanGameDevelopment/yourvrxp-utils.git
		* VR Tools: https://github.com/EstebanGameDevelopment/yourvrxp-vr.git
		* Networking Tools (only if you want network comms): https://github.com/EstebanGameDevelopment/yourvrxp-networking.git
	* The current project has the pre-processor constants ENABLE_OPENXR, ENABLE_NETWORKING, ENABLE_MIRROR. It's ready to plug-and-play with Oculus Link. If you don't want the networking part, just remove the pre-processor constant ENABLE_NETWORKING and you will be ready to go.
	
## BUILDS TESTED IN REAL DEVICES (Using Mirror networking with NetworkDiscovery):

* **Mobile Android Build**: https://www.dropbox.com/s/kzqxsmkxpcozh85/CodingTestXYZ_AndroidMobile.apk?dl=0
* **Oculus Quest build (using Oculus SDK)**: https://www.dropbox.com/s/smxdtwzjdf9ld7t/CodingTestXYZ_OculusSDK.apk?dl=0
* **Oculus Quest build (using OpenXR SDK)**: https://www.dropbox.com/s/czfpdpb1k3z49bz/CodingTestXYZ_OpenXR_For_Oculus.apk?dl=0
* **Pico Neo Build (using OpenXR SDK)**: https://www.dropbox.com/s/ovag8u4gomivcny/CodingTestXYZ_OpenXR_For_Pico.apk?dl=0

## Free Assets Used:

* Skyboxes: https://assetstore.unity.com/packages/2d/textures-materials/sky/classic-skybox-24923
* Gun: https://assetstore.unity.com/packages/3d/props/guns/bit-gun-22922
