/*
Adds grid lines to a surface
*/
shader_type spatial;
render_mode world_vertex_coords;

// how far above the mesh the grid lines appear, needed to not occlude underlying texture
const float overlay_offset = 0.001;
const float cell_size = 1.0;
const float line_thickness = 0.05;
const vec4 line_color = vec4(0.0, 0.0, 0.0, 1.0);

varying vec3 vertex_world_position;

// Called for every vertex the material is visible on.
void vertex() {
	// We want to work in world coordinates in the fragment shader
	vertex_world_position = VERTEX;
	VERTEX.y += overlay_offset;
}

// Called for every pixel the material is visible on.
void fragment() {
	// Idea is that we want lines periodically near every integer point of x or z
	// The "lines" are really just pixels colored with line_color within line_thickness of integer x and z coordinates.

	//get this pixels associated vertex's world coordinate
	//(see barycentric shader introduction for details)
	float x = vertex_world_position.x;
	//get integer part
	float integer_x = float(int(vertex_world_position.x));
	// get remainder - the distance from closest line center
	float remainder_x = abs(x - integer_x);
	// color line pixel if within line_thickness to left or right of line center
	bool x_line = remainder_x > cell_size - line_thickness || remainder_x < line_thickness;

	// same logic as for x but for z
	float integer_z = float(int(vertex_world_position.z));
	float z = vertex_world_position.z;
	float remainder_z = abs(z - integer_z);
	bool z_line = remainder_z > cell_size - line_thickness || remainder_z < line_thickness;
	if(x_line || z_line){
		ALBEDO = line_color.xyz;
	}else{
		discard;
	}
}