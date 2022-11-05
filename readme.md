# MobileNeRF Unity Viewer

![MobileNeRF Unity Viewer](/images/MobileNeRF_Unity.gif)

This repository contains the source code for a Unity port of the web viewer from the paper [MobileNeRF: Exploiting the Polygon Rasterization Pipeline for Efficient Neural Field Rendering on Mobile Architectures](https://mobile-nerf.github.io/)

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Setup

After cloning the project you can simply use the menu *MobileNeRF -> Asset Downloads* to download any of the sample scenes available.
In each scene folder there will be a convenient prefab, that you can then drag into the scene and you're good to go.

## Updating

Since the initial release a small number of features have been added to the automatic shader generation code.
That means, that if you have already downloaded some scenes before, you'll have to regenerate the source files by going to *MobileNeRF -> Asset Downloads* again.
(This will not actually redownload Assets unless necessary, so this shouldn't take too long)

## Importing self-trained scenes

If you have successfully trained your own MobileNeRF scenes using the [official code release](https://github.com/google-research/jax3d/tree/main/jax3d/projects/mobilenerf) and want to render them in Unity, you can use the menu *MobileNeRF -> Import from disk*.
This lets you choose a folder that should contain all the output files of your training process.

More specifically, the following assets are required:
- multiple shapes as OBJ files with naming convention: shapeX.obj
- multiple PNG files with naming convention: shapeX.pngfeatZ.png (where Z goes from 0 to 1, so 2 .pngs per shape)
- The weights of the MLP in a file called mlp.json

## Details

The project was created with Unity 2021.3 LTS using the Built-in Render Pipeline.

The biggest deviation from the official viewer is, that this project doesn't use Deferred Rendering, but uses Forward Rendering instead.
This has certain implications on performance.
While the MobileNeRF representation itself greatly reduces the cost to render NeRFs, it still requires evaluating a small, view-dependent MLP (Multi Layer Perceptron) per fragment. Whenever the bottleneck is in the fragment shader, Deferred Rendering has obvious advantages, as each pixel only needs to run a single fragment shader. 

Forward Rendering however gives us MSAA, which is important for VR use cases. Additionally, in VR the image has to be rendered twice, once for each eye, with a fairly large resolution. The larger the G-buffer, the smaller the benefit of Deferred Rendering. Still, MobileNeRFs mesh representations have a fairly large poly count which works against us using Forward Rendering here. 

Some things to possibly look into:
* Forward Rendering, but do a Depth Prepass to reduce overdraw (might require URP, see [here](https://forum.unity.com/threads/need-clarification-on-urps-use-of-the-depth-prepass.1004577/) and [here](https://gist.github.com/aras-p/5e3aa6f81c543ca74e3ae296c72ffcaf))
* Implement Deferred Rendering and compare performance in various scenarios


For more details read the official paper here: https://mobile-nerf.github.io/