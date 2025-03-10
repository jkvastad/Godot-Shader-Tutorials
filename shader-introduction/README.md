# Shader Introduction
[An introduction to shaders in Godot](https://docs.godotengine.org/en/stable/tutorials/shaders/introduction_to_shaders.html), expanding on things i felt were left out of the docs or otherwise not made explicit when learning shaders for Godot 4.

## What is a shader?
"In computer graphics, a [shader](https://en.wikipedia.org/wiki/Shader) is a computer program that calculates the appropriate levels of light, darkness, and color during the rendering of a 3D scene — a process known as shading."

While an ordinary program runs on the CPU (Central Processing Unit), a shader is a program which runs on the GPU (Graphics Processing Unit). Just like there are different languages to write code for a CPU (gdscript, C# etc.), there are different languages for the GPU (GLSL, HLSL, [Godot shading language](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/shading_language.html#shading-language)). 

### Why don't we just use CPU languages for the GPU?

When writing for a CPU we (mostly) write code which runs line by line in a single thread. This is usually fine, until we want to write a computer game and have to update the screen pixel colors.

A common desktop display resolution is 1920×1080 = 2 073 600 (~10^6) pixels. If we want 60 fps then that is 1920×1080×60 = 124 416 000 (~10^8) pixels per second. Modern CPs have a few GHz (~10^9) in clock speed, which means the job of keeping the screen painted would eat a sizeable chunk of the CPU budget, leaving little for running the game.

GPUs solve this problem by calculating the pixels in parallell, thus cutting the number of cycles down potentially by the millions. This comes at the cost of using another hardware architecture with its own constraints and languages.

## How a shader works
The [Wikipedia page on graphics pipelines](https://en.wikipedia.org/wiki/Graphics_pipeline) outlines the basics. In summary, to go from a 3D model to pixels on the screen:

1. Create models using polygon meshes.
2. Place the models in a game world relative to a camera.
3. Project the objects onto pixels on the screen.

### In more detail

* A model, or [mesh](https://en.wikipedia.org/wiki/Polygon_mesh), is created in e.g. [Blender](https://www.blender.org/). To the computer, the mesh is mostly a collection of [vertices](https://en.wikipedia.org/wiki/Vertex_(computer_graphics)) - data objects similar to a bag of vectors or arrays; specifying e.g. the various corners of the polygon mesh and colors related to these corners. Often vertex is used interchangeably with a [3D coordinate vector](https://en.wikipedia.org/wiki/Coordinate_vector) (even though the position vector is just a part of the vertex).
* A model is defined in its own local coordinate system. Since we want multiple models in a game the vertices of the respective models must be transformed so that the models are placed at the right place in our game world (we don't want everything stacked in a heap at the world origin). Transforming vectors is done via matrix multiplication, and the relevant matrix is called the "world matrix" which is specific to each model - specifying where it is placed in the global coordinate system of the game world.
* We look at the game world via a camera, and so a "camera matrix", or "view matrix",  transforms the models yet again to place them relative to the cameras coordinate system.
* There are different kinds of camera projections. A perspective camera gives a "normal" view where objects further away are smaller. An orthographic camera instead keeps object heights, making it look as if everything is up close. The type of projection is decided by the "projection matrix".
* There are differently sized screens (and within screens, application windows). This is handled by the "Window-Viewport" matrix, taking care of aspect ratios and window sizes.
* Finally we are ready to color pixels. This is called [rasterisation](https://en.wikipedia.org/wiki/Rasterisation), where we make a grid of pixels corresponding to the screen or viewport pixels, then look through this grid (or raster) at the scene displayed after all of the above transformations. Following [rasterisation rules](https://learn.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-rasterizer-stage-rules) the GPU decides which color to assign to which pixel.

Figure below: Pipeline flowchart of typical 3D hardware. Note the position of the vertex shader and pixel (fragment) shader.
![3D Pipeline](https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/3D-Pipeline.svg/1280px-3D-Pipeline.svg.png)
## Godot shaders in practice
When writing godot shaders we will mostly be writing code in a vertex or fragment function, a.k.a. vertex/fragment shaders. 
* The vertex shader runs on each vertex of a given mesh prior to the matrix transformations.
* The fragment shader runs on each pixel after rasterisation.

It is possible to send data to the fragment shader from the vertex shader. This can at first be quite perplexing, as it is not obvious how a vertex (a concept close to a point in 3D space) should send data to a screen pixel. The answer lies in [barycentric interpolation](https://en.wikipedia.org/wiki/Barycentric_coordinate_system). 

When the fragment shader is to decide what color to choose for its pixel, it is handed a single "best" triangle by the rasterizer. The current pixel might not be perfectly on a vertex of this triangle - it is most likely somewhere on the triangles interior. To decide on a pixel value, **the fragment shader interpolates a final value based on the values assigned to the triangle's vertices.**

This is why the variable type for sending data between vertex and fragment shader is called a varying - because the value will vary depending on where the pixel hits on the triangle and the result of the interpolation.
