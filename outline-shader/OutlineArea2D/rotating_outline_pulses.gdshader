shader_type canvas_item;

uniform vec4 fill_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform int pulse_count : hint_range(1, 10) = 3; // number of pulses
uniform float seconds_per_rotation = 2.0;
uniform float pulse_width = 0.2; // width of each pulse (0-1)
uniform float edge_width = 0.02; // border thickness

void fragment() {
    vec4 color = texture(TEXTURE, UV); // default is texture

    // Time-based rotation
    float time_angle = 2.0 * PI * TIME / seconds_per_rotation;

    // Distances from edges
    float left = UV.x;
    float right = 1.0 - UV.x;
    float top = UV.y;
    float bottom = 1.0 - UV.y;

    float perimeter_pos = 0.0;
    bool on_border = false;

    // Clockwise: top → right → bottom → left
    if (top < edge_width) {
        perimeter_pos = left;
        on_border = true;
    }
    else if (right < edge_width) {
        perimeter_pos = 1.0 + top;
        on_border = true;
    }
    else if (bottom < edge_width) {
        perimeter_pos = 2.0 + (1.0 - left);
        on_border = true;
    }
    else if (left < edge_width) {
        perimeter_pos = 3.0 + (1.0 - top);
        on_border = true;
    }

    // Normalize perimeter to 0-1
    perimeter_pos /= 4.0;

    // Only add color if on the border
	// Idea is that for a fix time_angle, perimeter_pos goes from 0->1
	// This adds color when sin value is above 1.0 - pulse_width
	// This also makes the pulse loop per border, but we want the illusion of rotation
	// By normalizing, we do not loop around on a border (max angle from perimeter is only pi/2)
	// This also allows us to offset each borders pulse with the correct amount for the illusion
	// Finally just repeat the rotating pulse illusion with offsets for more pulses
    if (on_border) {
        for (int i = 0; i < pulse_count; i++) {
            float offset = float(i) / float(pulse_count) * 2.0 * PI;
            if (sin(time_angle + perimeter_pos * 2.0 * PI - offset) > 1.0 - pulse_width) {
                color = fill_color;
            }
        }
    }
    COLOR = color;
}