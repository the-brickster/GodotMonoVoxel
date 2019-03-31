extends "res://src/Pawn.gd"

# Inspector properties
export(Vector2) var camera_sensitivity = Vector2(0.15, 0.15)
#Signals
signal mouse_movement(mouseMove)

# Internal properties
var camera = null
var camera_yaw = 0.0
var camera_pitch = 0.0
var camera_yaw_node = null
var camera_pitch_node = null
var weapon_node = null
var fpshud_node = null
##
# Init
##

func _ready():
	
	set_process_input(true)
#	set_fixed_process(true)
	set_physics_process(true)

	camera_yaw_node = get_node("CameraYaw")
	camera_pitch_node = camera_yaw_node.get_node("CameraPitch")
	camera = camera_pitch_node.get_node("Camera")
	weapon_node = get_node("CameraYaw/CameraPitch/GunNode").get_child(0)
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	connect("mouse_movement",self,"_mousemove_callback")
	fpshud_node = get_node("CameraYaw/CameraPitch/Camera/FPSHUD")
	fpshud_node.connectWeapon(weapon_node)

func _input(event):
	handle_camera_input(event)
	handle_jump_input(event)
	handle_escape(event)
	

func _physics_process(delta):
	_fixed_process(delta)

func _fixed_process(delta):
	update_locomotion()
	

func handle_escape(event):
	if event.is_action_pressed("ui_cancel"):
		get_tree().quit()

func handle_jump_input(event):
	if event.is_action_pressed("jump"):
		jump()
		
##
# Handle locomotion
##
func update_locomotion():
	var aim_direction = camera_yaw_node.get_global_transform().basis
	move_direction = Vector3()

	if Input.is_action_pressed("walk_forward"):
		move_direction -= aim_direction.z
	
	if Input.is_action_pressed("walk_backward"):
		move_direction += aim_direction.z
	
	if Input.is_action_pressed("walk_left"):
		move_direction -= aim_direction.x
	
	if Input.is_action_pressed("walk_right"):
		move_direction += aim_direction.x

	move_direction = move_direction.normalized()

##
# Handle camera input
##
func handle_camera_input(event):
	if event is InputEventMouseMotion == false:
		return
	emit_signal("mouse_movement",event.get_relative())
	#Calculate Yaw
	camera_yaw = fmod(camera_yaw - event.get_relative().x * camera_sensitivity.y, 360)
	#Calculate Pitch
	camera_pitch = max(min(camera_pitch - event.get_relative().y * camera_sensitivity.x, 85), -85)
	
	#Apply the yaw and the pitch
	camera_yaw_node.set_rotation(Vector3(0, deg2rad(camera_yaw), 0))
	camera_pitch_node.set_rotation(Vector3(deg2rad(camera_pitch), 0, 0))
	
func _mousemove_callback(mouseMove):
	weapon_node._on_FPSController_mouse_movement(mouseMove)
##
# Gets the look base point
##
func get_look_base():
	return camera.get_global_transform().origin

##
# Gets the look direction
##
func get_look_direction():
	return camera.get_global_transform().basis.z

