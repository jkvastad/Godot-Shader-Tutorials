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
