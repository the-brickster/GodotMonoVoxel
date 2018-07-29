extends KinematicBody

var _is_loaded = false
var velocity=Vector3()
var yaw = 0
var pitch = 0
var is_moving=false
var on_floor=false
var jump_timeout=0
var step_timeout=0
var attack_timeout=0
var fly_mode=false
var alive=true
var current_target=null
var current_target_2d_pos=null
var multijump=0
var is_attacking=false
var is_jumping=false
var is_running=false
var is_crouching=false
var weapon_base=null
var body_position="stand"

onready var node_camera=get_node("yaw/camera")
#onready var node_sfx=get_node("eco_sfx")
onready var node_tween=get_node("tween")
onready var node_leg=get_node("body/leg")
onready var node_yaw=get_node("yaw")
onready var node_body=get_node("body")
onready var node_head_check=get_node("headCheckArea")
onready var node_action_ray=get_node("yaw/camera/actionRay")
onready var node_ray = get_node("ray")
onready var node_step_ray=get_node("stepRay")

const GRAVITY_FACTOR=3
## physics
export(float) var ACCEL= 2
export(float) var DEACCEL= 4 
export(float) var FLY_SPEED=100
export(float) var FLY_ACCEL=4
export(float) var GRAVITY=-9.8
export(float) var MAX_JUMP_TIMEOUT=0.2
export(float) var MAX_ATTACK_TIMEOUT=0.2
export(int) var MAX_SLOPE_ANGLE = 40
export(float) var STAIR_RAYCAST_HEIGHT=0.75
export(float) var STAIR_RAYCAST_DISTANCE=0.58
export(float) var STAIR_JUMP_SPEED=5
export(float) var STAIR_JUMP_TIMEOUT=0.1
export(float) var footstep_factor=0.004
export(float) var view_sensitivity = 0.3

#Player movement key variables
var move_forward = "walk_forward"
var move_backward = "walk_backward"
var move_left = "walk_left"
var move_right = "walk_right"
var jump = "jump"
var crouch = "crouch"

func _ready():
	# Called every time the node is added to the scene.
	# Initialization here
	
	node_ray.add_exception(self)
	
	set_fixed_process(true)
	set_process_input(true)

#Mouse input processing
func _input(event):
	if event.type == InputEvent.MOUSE_MOTION:
		yaw = fmod(yaw - event.relative_x * view_sensitivity,360)
		pitch = max(min(pitch - event.relative_y * view_sensitivity,90),-90)
		node_yaw.set_rotation(Vector3(0,deg2rad(yaw),0))
		node_camera.set_rotation(Vector3(deg2rad(pitch),0,0))
		

#Main Loop
#Fixed process is same as process, has delta parameter. But it used for synchronization with physics engine
func _fixed_process(delta):
	self._walk(delta)

func _enter_tree():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _exit_tree():
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

#Movement processing
var prevValue = Vector3(1,1,1)

func _walk(delta):
	
	if(jump_timeout > 0):
		jump_timeout-=delta
	
	var aimDirection = node_camera.get_global_transform().basis
	var moveDir = Vector3()
	
	if Input.is_action_pressed(self.move_forward):
		moveDir -= aimDirection[2]
	if Input.is_action_pressed(self.move_backward):
		moveDir += aimDirection[2]
	if Input.is_action_pressed(self.move_left):
		moveDir -= aimDirection[0]
	if Input.is_action_pressed(self.move_right):
		moveDir +=aimDirection[0]
#	#Reset the flag for actor movement
	self.is_moving = (moveDir.length() > 0)
	
	moveDir.y = 0
	moveDir = moveDir.normalized()
	var is_ray_colliding = node_ray.is_colliding()
	if !on_floor and is_ray_colliding and velocity.y <= 0:
		self.set_translation(node_ray.get_collision_point())
		on_floor = true
	elif on_floor and not is_ray_colliding:
		on_floor = false;
	if on_floor:
		if step_timeout <= 0:
			step_timeout = 1
		else:
			step_timeout = velocity.length()*footstep_factor
			
		# if on floor move along the floor. To do so, we calculate the velocity perpendicular to the normal of the floor.
		var n=node_ray.get_collision_normal()
		velocity=velocity-velocity.dot(n)*n
		# if the character is in front of a stair, and if the step is flat enough, jump to the step.
		if is_moving and node_step_ray.is_colliding():
			var step_normal=node_step_ray.get_collision_normal()
			if (rad2deg(acos(step_normal.dot(Vector3(0,1,0))))< MAX_SLOPE_ANGLE):
				velocity.y=STAIR_JUMP_SPEED
				jump_timeout=STAIR_JUMP_TIMEOUT
		# apply gravity if on a slope too steep
		if (rad2deg(acos(n.dot(Vector3(0,1,0))))> MAX_SLOPE_ANGLE):
			velocity.y+=delta*GRAVITY*GRAVITY_FACTOR
	else:
		# apply gravity if falling
		velocity.y+=delta*GRAVITY*GRAVITY_FACTOR
	
	# calculate the target where the player want to move
	var target=moveDir*10
	# if the character is moving, he must accelerate. Otherwise he deccelerates.
	var accel=DEACCEL
	if is_moving:
		accel=ACCEL
	
	# calculate velocity's change
	var hvel=velocity
	hvel.y=0
	
	# calculate the velocity to move toward the target, but only on the horizontal plane XZ
	hvel=hvel.linear_interpolate(target,accel*delta)
	velocity.x=hvel.x
	velocity.z=hvel.z
	
	# move the node
	var motion=velocity * delta
	motion=move(motion)
	var string = "%s value"
	var string2 = string % motion
	print (string2)
	# slide until it doesn't need to slide anymore, or after n times
	var original_vel=velocity
	if(motion.length()>0 and is_colliding()):
		var n=get_collision_normal()
		motion=n.slide(motion)
		velocity=n.slide(velocity)
		# check that the resulting velocity is not opposite to the original velocity, which would mean moving backward.
		if(original_vel.dot(velocity)>0):
			motion=move(motion)
	
	if on_floor:
		# move with floor but don't change the velocity.
		var floor_velocity=_get_floor_velocity(node_ray,delta)
		if floor_velocity.length()!=0:
			move(floor_velocity*delta)
		
		# jump
		if Input.is_action_pressed(jump) and body_position=="stand" and jump_timeout <= 0:
			print("jumporini")
			velocity.y=10
			jump_timeout=MAX_JUMP_TIMEOUT
			on_floor=false
			multijump=0

	elif Input.is_action_pressed(jump) and multijump>0 and jump_timeout<=0:
		
		velocity.y=9
		jump_timeout=MAX_JUMP_TIMEOUT
		on_floor=false
		multijump-=1
	
	# update the position of the raycast for stairs to where the character is trying to go, so it will cast the ray at the next loop.
	if is_moving:
		var sensor_position=Vector3(moveDir.z,0,-moveDir.x)*STAIR_RAYCAST_DISTANCE
		sensor_position.y=STAIR_RAYCAST_HEIGHT
		node_step_ray.set_translation(sensor_position)

func _get_floor_velocity(ray,delta):
	var floor_velocity=Vector3()
	# only static or rigid bodies are considered as floor. If the character is on top of another character, he can be ignored.
	var object = ray.get_collider()
	if object is RigidBody or object is StaticBody:
		var point = ray.get_collision_point() - object.get_translation()
		var floor_angular_vel = Vector3()
		# get the floor velocity and rotation depending on the kind of floor
		if object is RigidBody:
			floor_velocity = object.get_linear_velocity()
			floor_angular_vel = object.get_angular_velocity()
		elif object is StaticBody:
			floor_velocity = object.get_constant_linear_velocity()
			floor_angular_vel = object.get_constant_angular_velocity()
		# if there's an angular velocity, the floor velocity take it in account too.
		if(floor_angular_vel.length()>0):
			var transfurm = Matrix3(Vector3(1, 0, 0), floor_angular_vel.x)
			transfurm = transfurm.rotated(Vector3(0, 1, 0), floor_angular_vel.y)
			transfurm = transfurm.rotated(Vector3(0, 0, 1), floor_angular_vel.z)
			floor_velocity += transfurm.xform_inv(point) - point
			
			# if the floor has an angular velocity (rotation force), the character must rotate too.
			yaw = fmod(yaw + rad2deg(floor_angular_vel.y) * delta, 360)
			node_yaw.set_rotation(Vector3(0, deg2rad(yaw), 0))
	return floor_velocity
