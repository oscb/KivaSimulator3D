KivaSimulator3D
===============

A simple Kiva Systems Simulator made on Unity 3D Engine.

# How-to
Before using the simulator you will need to install the **LP Solve dylib** you may find it on the repo on 
	Assets/Scripts/libs/liblpsolve55.dylib

Move the dylib to wherever you want. Now open Unity.app using *Show package contents* and add the following line to: 			

	Unity.app/Contents/Frameworks/Mono/etc/mono/config
	<dllmap dll="lpsolve55.dll" target=“/path/to/liblpsolve55.dylib" os="osx"/>

If you don’t want to mess up with the Unity package you can just build the game and the libs will be copied automatically.

————————————————
## Notes:
Only tested on MacOSX.


