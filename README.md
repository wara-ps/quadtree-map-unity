# QuadTree Map Engine

This is a sample Unity3D project to view quadtree tiled terrain models. It also contains some sample tools to pack tile models into asset bundles in order to increase runtime performance.

## Getting started

1. Clone this repository: `git clone git@github.com:wara-ps/quadtree-map-unity.git`.
2. Download the tileset from [here](https://warapsportalstorage.z16.web.core.windows.net/granso-quadtree-tiles-unity-bundles.zip) and unzip somewhere on your machine.
3. Open the project in Unity3D (tested with version 2019.3.7f).
4. Open the sample scene *Assets/Scenes/SampleScene.unity*.
5. Locate the *GransoWorld* object in the Hierarchy view and set the *Base Url* field to `file:///<path to tileset>`. Replace *\<path to tileset>* with the absolute path to the unzipped folder, containing the file *metadata.xml*.
6. Press play (`CTRL + P`).
7. Right click in the Game view to capture the mouse. The camera is controlled by looking around with the mouse, and flying using the WASD keys. Right click again to uncapture the mouse.
