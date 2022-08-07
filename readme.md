# MobileNeRF Unity Viewer

![MobileNeRF Unity Viewer](/images/MobileNeRF_Unity.gif)

This repository contains the source code for a Unity port of the web viewer from the paper [MobileNeRF: Exploiting the Polygon Rasterization Pipeline for Efficient Neural Field Rendering on Mobile Architectures](https://mobile-nerf.github.io/)

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Setup

After cloning the project you can simply use the menu *MobileNeRF -> Asset Downloads* to download any of the sample scenes available.
In each scene folder there will be a convenient prefab, that you can then drag into the scene and you're good to go.

## Details

The project was created with Unity 2021.3 LTS using the Built-in Render Pipeline.

The biggest deviation from the official viewer is that this project doesn't use Deferred Rendering, but uses Forward Rendering instead.
Part of the reason was just me being more comfortable implementing custom shaders for forward rendering pipelines in Unity.
But I also think this makes it more practical to add VR support for this, by following [these steps](https://docs.unity3d.com/Manual/SinglePassInstancing.html). (I haven't had time for that yet).

For more details read the official paper here: https://mobile-nerf.github.io/