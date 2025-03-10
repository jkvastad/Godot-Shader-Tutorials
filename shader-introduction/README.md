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

**Figure below:** Rasterisation on primitives (here triangles, could be lines or points), picture from [Microsoft article on rasterisation rules](https://learn.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-rasterizer-stage-rules). This is what the rasterizer sees after the viewport transform has been performed, primitives outside the viewport have been clipped and backfacing primitives have been culled.

![Rasterisation](https://learn.microsoft.com/en-us/windows/win32/direct3d11/images/d3d10-rasterrulestriangle.png)

**Figure below:** Pipeline flowchart of typical 3D hardware. Note the position of the vertex shader and pixel (fragment) shader.

![3D Pipeline](https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/3D-Pipeline.svg/1280px-3D-Pipeline.svg.png)
## Godot shaders in practice
### Fragment and Vertex shader
When writing godot shaders we will mostly be writing code in a vertex or fragment function, a.k.a. vertex/fragment shaders. 
* The vertex shader runs on each vertex of a given mesh prior to the matrix transformations.
* The fragment shader runs on each pixel after rasterisation.

It is possible to send data to the fragment shader from the vertex shader. This can at first be quite perplexing, as it is not obvious how a vertex (a concept close to a point in 3D space) should send data to a screen pixel. The answer lies in [barycentric interpolation](https://en.wikipedia.org/wiki/Barycentric_coordinate_system). 

When the fragment shader is to decide what color to choose for its pixel, it is handed a single "best" triangle by the rasterizer. The current pixel might not fall perfectly on a vertex of this triangle - it most likely falls somewhere on the triangles interior. To decide on a pixel value, **the fragment shader interpolates a final value based on the values assigned to the triangle's vertices.** 

This is why the variable type for sending data between vertex and fragment shader is called a varying - because the value will vary depending on where the pixel hits on the triangle and the result of the interpolation. But exactly how does it vary? Behold the cube:

![Barycentric interpolation](https://github.com/user-attachments/assets/0bf6ca90-41e4-45b4-99d6-126b4f5ab8de)

The cube is a godot BoxMesh with the following shader applied:

```glsl
// Displays barycentric coordinates as colors
// This illustrates how varying type variables interpolate values sent from the vertex shader to the fragment shader.
shader_type spatial;
varying vec3 vertex_position;

//Corners of a triangle receive a unit weight (báros greek for weight)
const vec3 BARYCENTRIC_COORDINATES[] = {
	vec3(1.0,0.0,0.0),
	vec3(0.0,1.0,0.0),
	vec3(0.0,0.0,1.0)
};

void vertex() {
	// Current vertex index (search for "VBO indexing" for more details)
	// Every three indices make a triangle
	vertex_position = BARYCENTRIC_COORDINATES[VERTEX_ID%3];	
}

void fragment() {
	// Note how the colors are interpolated between the vertices of our cubes triangles
	ALBEDO = vertex_position;	
}

```

Note how each corner of each triangle on the cube is red, greed or blue, and the values inbetween are interpolated between them.

### Shader application: drawing cube edges
In the shader introduction project, there is a folder called [Cube Vertex Edges](https://github.com/jkvastad/Godot-Shader-Tutorials/tree/main/shader-introduction/Cube%20Vertex%20Edges). Running the scene in this project yields the following animation loop:

![Edge_Shader](https://github.com/user-attachments/assets/327987c3-3869-4718-896d-427e3818709e)

Notice how there is some z-fighting due to floating point rounding when the cubes overlap. The edges are drawn by exploiting the geometry of the cube and passing the (interpolated) information along to the fragment shader:
```glsl
// Draws the edges of a cube
// Uses cube specific vertex positions to calculate edges
shader_type spatial;
render_mode unshaded;
varying vec3 model_vertex;

void vertex() {	
	model_vertex = VERTEX;
}

void fragment() {	
	float threshold = 0.99;
	float xy = abs(model_vertex.x) + abs(model_vertex.y);
	float xz = abs(model_vertex.x) + abs(model_vertex.z);
	float yz = abs(model_vertex.y) + abs(model_vertex.z);
	if(xy > threshold ||xz > threshold ||yz > threshold ){
		ALBEDO = vec3(0);
	}else{
		ALBEDO = model_vertex;						
	}
}

```

### Post-process shader application: drawing cube edges
In the shader introduction project, there is a folder called [Post-Process Edges](https://github.com/jkvastad/Godot-Shader-Tutorials/tree/main/shader-introduction/Post-Process%20Edges). Running the scene in this project yields the following animation loop:

![Godot_fragment_edges](https://github.com/user-attachments/assets/fccd902d-b288-4f72-83a8-e2f43773ea68)

Notice how the edges disappear when the cubes touch! This is because we are using a post-process shader, which calculates the edges based on the screen colors of a previously rendered screen. This animation uses two shaders - first one to color the face normals of the cube, which is then used by the second shader to color edges when normals change:

```glsl
// Shader for coloring surfaces with model normals.
// Used to e.g. separate planes in post processing
shader_type spatial;
render_mode unshaded;
varying flat vec3 model_normal;   

void vertex() {	        
	// Use model normals instead of world normals
	model_normal = NORMAL;
}

void fragment() {		
	// negative values are clamped to 0, instead discard negative sign
	ALBEDO = abs(model_normal);
}
```
```glsl
// Shader used to draw edges around planes
// Assumes the model is colored according to its model normals
shader_type spatial;
render_mode unshaded;
//Screen pixel colors from previous rendering pass
uniform sampler2D screen_texture : hint_screen_texture; 

bool is_neighbour_pixel_same_color(vec3 color,vec2 screen_uv_offset){
	// Arbitrary value to avoid inexact floating point equality
	float error_threshold = 0.01; 
	return length(color - texture(screen_texture,screen_uv_offset,0.0).rgb)>error_threshold;
}

void fragment() {
	//pixels to screen UV
	int outline_pixels = 1;
	float outline_width = float(outline_pixels)/VIEWPORT_SIZE.x; 
		
	vec2 left = SCREEN_UV + vec2(-outline_width,0);
	vec2 right = SCREEN_UV + vec2(outline_width,0);
	vec2 up = SCREEN_UV + vec2(0,outline_width);
	vec2 down = SCREEN_UV + vec2(0,-outline_width);
	
	vec3 pixel_color = texture(screen_texture,SCREEN_UV,0.0).rgb;	
		
	// Compare colors from previous pass around the current fragment pixel
	if(is_neighbour_pixel_same_color(pixel_color,left)
	||is_neighbour_pixel_same_color(pixel_color,right)
	||is_neighbour_pixel_same_color(pixel_color,up)
	||is_neighbour_pixel_same_color(pixel_color,down)){
		ALBEDO = vec3(0,0,0);	
	}else{
		ALBEDO = pixel_color;				
	}
	}	
```
