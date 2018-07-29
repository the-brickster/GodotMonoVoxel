shader_type spatial;
render_mode blend_mix, unshaded, cull_disabled;
uniform sampler2D normal_tex;
uniform float time;
uniform float life;

void fragment()
{
	float tmp =COLOR.a - mix(0.0,1.0,mod(time,10.0));
	ALBEDO = COLOR.rgb;
	ALPHA = smoothstep(0.0,1.0,tmp);
	if(ALPHA < 0.01){
		discard;
	}
//	ALPHA = COLOR.a;
//	ALPHA = tmp;
}