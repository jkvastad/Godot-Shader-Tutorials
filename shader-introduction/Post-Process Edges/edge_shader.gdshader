// Shader used to draw edges around planes
// Assumes the model is colored according to its model normals
shader_type spatial;
//Screen pixel colors from precious rendering pass
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
	