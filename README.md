# QuadTree Map Engine

This is a sample Unity3D project to view quadtree tiled terrain models. It also contains some sample tools to pack tile models into asset bundles in order to increase runtime performance.

![alt text](screenshot.png "Screenshot of QuadTree Map Engine running in the Unity editor showing the Gränsö tileset.")

## Getting started

1. Clone this repository: `git clone git@github.com:wara-ps/quadtree-map-unity.git`.
2. Download the Gränsö tileset from [here](https://warapsportalstorage.z16.web.core.windows.net/granso-quadtree-tiles-unity-bundles.zip) and unzip somewhere on your machine.
3. Open the *src* folder as a project in Unity3D (tested with version 2019.3.7f).
4. Open the sample scene *Assets/Scenes/SampleScene.unity*.
5. Locate the *GransoWorld* object in the Hierarchy view and set the *Base Url* field to `file:///<path to tileset>`. Replace *\<path to tileset>* with the absolute path to the unzipped folder, containing the file *metadata.xml*.
6. Press play (`CTRL + P`).
7. Right click in the Game view to capture the mouse. The camera is controlled by looking around with the mouse, and flying using the WASD keys. Right click again to uncapture the mouse.

## Using your own tileset

Unity cannot read large models in open file formats (.obj, .fbx, etc.) at runtime in a very performant way. Therefore this tile engine requires the tileset to be processed in Unity first and then packed as asset bundles. This way the runtime loading performance is greatly increased.

Given that you have a 3D terrain model structured as a QuadTree already, there is functionality included in this repository to generate metadata and asset bundle files which can be loaded by this tile engine. You will probably have to modify the code somewhat, though.
