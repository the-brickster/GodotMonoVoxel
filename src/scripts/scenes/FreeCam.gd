extends Camera

const MOUSE_SENSITIVITY = 0.002

var sprint_move_speed = 2.5
# The camera movement speed (tweakable using the mouse wheel)
var move_speed = 0.5

# Stores where the camera is wanting to go (based on pressed keys and speed modifier)
var motion = Vector3()

# Stores the effective camera velocity
var velocity = Vector3()

# The initial camera node rotation
var initial_rotation = self.rotation.y

var camPosText:Label
var fpsTextLabel:Label
var currVal:String

#Shader params
var horizontalFOV:float = 140.0
var strength:float = 0.5
var cylindricalRatio:float = 2
var height:float = 0;
var screenSize = Vector2(0,0)
var aspect:float = 0.0

func _ready():
	fpsTextLabel= self.get_node("HUDNode/HUDCanvas/FPSText")
	camPosText = self.get_node("HUDNode/HUDCanvas/CamPosText")
	
	
func _enter_tree():
	horizontalFOV = fov
	$HUDNode/HUDCanvas/ShaderMenu/CheckButton.pressed = false
	screenSize.x = get_viewport().get_visible_rect().size.x # Get Width
	screenSize.y = get_viewport().get_visible_rect().size.y # Get Height
	aspect = screenSize.x/screenSize.y;
	height = tan(deg2rad(horizontalFOV)/2.0)/aspect
	
	fov = atan(height)*2*180/PI
	$HUDNode/HUDCanvas/ShaderMenu/FOVSlider.value = fov
	$HUDNode/HUDCanvas/ShaderMenu/BarrelPower.text = str(strength)
	$HUDNode/HUDCanvas/ShaderMenu/ShaderPropLabel2.text = "FOV: "+str(horizontalFOV)
	$HUDNode/HUDCanvas/ShaderMenu/cylinratio.value = cylindricalRatio
	
	
	$HUDNode/Barrel.material.set_shader_param("strength",strength)
	$HUDNode/Barrel.material.set_shader_param("height",height)
	$HUDNode/Barrel.material.set_shader_param("aspectRatio",aspect)
	$HUDNode/Barrel.material.set_shader_param("cylindricalRatio",cylindricalRatio)
	
	

func _input(event):
	# Mouse look (effective only if the mouse is captured)
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		# Horizontal mouse look
		rotation.y -= event.relative.x*MOUSE_SENSITIVITY
		# Vertical mouse look, clamped to -90..90 degrees
		rotation.x = clamp(rotation.x - event.relative.y*MOUSE_SENSITIVITY, deg2rad(-90), deg2rad(90))


	# Toggle HUD
#	if event.is_action_pressed("toggle_hud"):
#		FPSCounter.visible = !FPSCounter.visible

	# These actions do not make sense when the settings GUI is visible, hence the check
#	if not SettingsGUI.visible:
#		# Toggle mouse capture (only while the menu is not visible)
	if event.is_action_pressed("toggle_mouse_capture"):
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			$HUDNode/HUDCanvas/ShaderMenu/BarrelPower.release_focus()
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

		# Movement speed change

func _process(delta):

	fpsTextLabel.text = str(Engine.get_frames_per_second())
	camPosText.text = str(self.get_camera_transform().origin)
	
	if Input.is_action_just_pressed("ui_cancel"):
		get_tree().quit()

	# Movement
	if(Input.is_action_pressed("sprint")):
		move_speed = sprint_move_speed*2.0
	elif(Input.is_action_just_released("sprint")):
		move_speed = 2.0
		print("released "+str(move_speed))
	
	if Input.is_action_pressed("walk_left"):
		motion.x = -1
	elif Input.is_action_pressed("walk_right"):
		motion.x = 1
	else:
		motion.x = 0

	if Input.is_action_pressed("walk_forward"):
		motion.z = -1
	elif Input.is_action_pressed("walk_backward"):
		motion.z = 1
	else:
		motion.z = 0

	if Input.is_action_pressed("move_up"):
		motion.y = 1
	elif Input.is_action_pressed("move_down"):
		motion.y = -1
	else:
		motion.y = 0

	# Normalize motion
	# (prevents diagonal movement from being `sqrt(2)` times faster than straight movement)
	motion = motion.normalized()

	# Speed modifier
	if Input.is_action_pressed("move_speed"):
		motion *= 2

	# Rotate the motion based on the camera angle
	motion = motion \
		.rotated(Vector3(0, 1, 0), rotation.y - initial_rotation) \
		.rotated(Vector3(1, 0, 0), cos(rotation.y)*rotation.x) \
		.rotated(Vector3(0, 0, 1), -sin(rotation.y)*rotation.x)

	# Add motion
	velocity += motion*move_speed

	# Friction
	velocity *= 0.9

	# Apply velocity
	translation += velocity*delta
	


func _exit_tree():
	# Restore the mouse cursor upon quitting
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

#uniform float BarrelPower =1.1;
func _on_BarrelPower_text_changed(new_text:String):
	if(new_text.is_valid_float()):
		currVal = new_text
		$HUDNode/Barrel.material.set_shader_param("strength",currVal)
	else:
		$HUDNode/HUDCanvas/ShaderMenu/BarrelPower.text = currVal


func _on_FOVSlider_value_changed(value:float):
	horizontalFOV = value
	height = tan(deg2rad(horizontalFOV)/2.0)/aspect
	
	fov = atan(height)*2*180/PI
	$HUDNode/HUDCanvas/ShaderMenu/ShaderPropLabel2.text = "FOV: "+str(horizontalFOV)
	$HUDNode/Barrel.material.set_shader_param("height",height)


func _on_CheckButton_pressed():
	if($HUDNode/HUDCanvas/ShaderMenu/CheckButton.pressed):
		$HUDNode/Barrel.show()
	else:
		$HUDNode/Barrel.hide()


func _on_cylinratio_value_changed(value:float):
	cylindricalRatio = value
	$HUDNode/HUDCanvas/ShaderMenu/ShaderPropLabel4.text = "Cyln Ratio: "+str(cylindricalRatio)
	$HUDNode/Barrel.material.set_shader_param("cylindricalRatio",cylindricalRatio)
