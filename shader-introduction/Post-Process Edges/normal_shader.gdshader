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
