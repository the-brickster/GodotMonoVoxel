shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,vertex_lighting;

uniform vec4 albedo : hint_color;
uniform float point_size : hint_range(0,128);//Splat radii
uniform float near;
uniform float far;
uniform float bottom; //bottom parameter of the viewing frustum
uniform float height; //Height of the viewport

//BOKEH UNIFORMS
// The Golden Angle is (3.-sqrt(5.0))*PI radians, which doesn't precompiled for some reason.
uniform float GOLDEN_ANGLE = 2.39996;
uniform int ITERATIONS = 150;
varying mat2 rot;
//mat2(cos(GOLDEN_ANGLE), sin(GOLDEN_ANGLE), -sin(GOLDEN_ANGLE), cos(GOLDEN_ANGLE))

//Normal Matrix will need to be calculated
//Formula transpose(inverse(gl_ModelViewMatrix))
varying mat3 normalMatrix;
varying vec3 ex_Normals;
varying flat float radiusPixels;
varying flat vec2 center;

//https://stackoverflow.com/questions/25780145/gl-pointsize-corresponding-to-world-space-size

void vertex() {
	vec4 gl_position = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX,1.0);
	center = (0.5 * gl_position.xy/gl_position.w + 0.5) * VIEWPORT_SIZE;
	POINT_SIZE =( VIEWPORT_SIZE.y * PROJECTION_MATRIX[1][1] * point_size/gl_position.w)*point_size;
	radiusPixels = POINT_SIZE/2.0;
//	normalMatrix = mat3(transpose(inverse(MODELVIEW_MATRIX)));
//	NORMAL = normalize(normalMatrix * NORMAL);
//	if(abs(NORMAL.z) <=0.1){
//		NORMAL.z = 0.1;
//	}
//
//	POINT_SIZE = point_size;
	rot = mat2(vec2(cos(GOLDEN_ANGLE), sin(GOLDEN_ANGLE)), vec2(-sin(GOLDEN_ANGLE), cos(GOLDEN_ANGLE)));
	COLOR = albedo;
}

void light(){
	
}

vec3 Bokek ( sampler2D tex, vec2 uv, float radius){
	vec3 acc = vec3(0), div = acc;
	float r = 1.;
    vec2 vangle = vec2(0.0,radius*.01 / sqrt(float(ITERATIONS)));
    
	for (int j = 0; j < ITERATIONS; j++)
    {  
        // the approx increase in the scale of sqrt(0, 1, 2, 3...)
        r += 1. / r;
	    vangle = rot * vangle;
        vec3 col = texture(tex, uv + (r-1.) * vangle).xyz; /// ... Sample the image
        col = col * col *1.8; // ... Contrast it for better highlights - leave this out elsewhere.
		vec3 bokeh = pow(col, vec3(4));
		acc += col * bokeh;
		div += bokeh;
	}
	return acc / div;
}

void fragment() {
	vec2 uv = FRAGCOORD.xy/VIEWPORT_SIZE.x;
	float blurFactor = 1.5;
	float rad = .8 - .8*cos(blurFactor * 6.283);
	
	
	
}
