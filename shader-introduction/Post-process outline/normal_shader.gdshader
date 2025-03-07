// Shader for coloring surfaces with model normals.
// Used to e.g. separate planes in post processing
shader_type spatial;
varying flat vec3 vertex_normal;   

void vertex() {	        
	// Use model normals instead of world normals
	vertex_normal = NORMAL;
}

void fragment() {		
	// negative values are clamped to 0, instead discard negative sign
	ALBEDO = abs(vertex_normal);
}
