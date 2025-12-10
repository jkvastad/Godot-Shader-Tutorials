/*
Highlights a grid in grid_lines shader
*/
shader_type spatial;
render_mode world_vertex_coords;

// how far above the mesh the grid lines appear, needed to not occlude underlying renders
const float overlay_offset = 0.005;
const float cell_size = 1.0;
const float line_thickness = 0.05;
const vec4 line_color = vec4(1.0, 1.0, 0.0, 1.0);

varying vec3 vertex_world_position;

// the grid cell coordinate to be highlighted - only uses z and x
uniform vec3 grid_point;

// Called for every vertex the material is visible on.
void vertex() {
	// We want to work in world coordinates in the fragment shader
	vertex_world_position = VERTEX;
	VERTEX.y += overlay_offset;
}

// Called for every pixel the material is visible on.
void fragment() {
	// Check if fragment belongs to outline grid cell:
	// Is our pixel's vertex in the same quadrant as our point?
	// (there is no -0 so we must check this before next step to avoid bad behavior near origin)
	if(sign(vertex_world_position.z) == sign(grid_point.z)
	&& sign(vertex_world_position.x) == sign(grid_point.x)){
		// does or grid point share integer parts with this fragment's vertex?
		if(int(vertex_world_position.x) == int(grid_point.x)
		&& int(vertex_world_position.z) == int(grid_point.z)){
			//draw outline - same as for grid_lines but other color
			float integer_x = float(int(vertex_world_position.x));
			float x = vertex_world_position.x;
			float remainder_x = abs(x - integer_x);
			bool x_line = remainder_x > cell_size - line_thickness || remainder_x < line_thickness;

			float integer_z = float(int(vertex_world_position.z));
			float z = vertex_world_position.z;
			float remainder_z = abs(z - integer_z);
			bool z_line = remainder_z > cell_size - line_thickness || remainder_z < line_thickness;
			if(x_line || z_line){
					ALBEDO = line_color.xyz;
			}else{
				ALPHA = 0.0;
			}
		}else{
			ALPHA = 0.0;
		}
	}else{
		ALPHA = 0.0;
	}
	// if we're not drawing outline we just make it transparent to show underlying renders
}