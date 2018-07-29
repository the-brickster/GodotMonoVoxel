extends KinematicBody

# class member variables go here, for example:
# var a = 2
# var b = "textvar"
export(int) var health = 100
export(int) var max_health = 100
export(float) var movement_speed = 10.0
export(float) var jumping_force = 15.0
export(bool) var can_double_jump = false
export(float) var air_control = 0.9
export(float) var line_of_sight = 50.0
export(float) var floor_angle_max = 45.0

# Internal properties
var components = []
var move_direction = Vector3(0, 0, 0)
var focused_actor = null
var focused_position = Vector3(0, 0, 0)
var velocity = Vector3(0, 0, 0)
var jumping_modifier = 0
var has_double_jumped = false

##
# Init
##
func _ready():
	init_components()
	init_signals()

func _physics_process(delta):
	update_velocity(delta)
	update_focused_actor()

##
# Fixed update
##
func _fixed_process(delta):
	print("called")
	update_velocity(delta)
	update_focused_actor()

##
# Initialises signals
##
func init_signals():
	pass

##
# Initialises components
##
func init_components():
	for node in get_children():
        # Add Accord components
#        if node is Component:
#            components.append(node)

        # Add native components
		if node is Area:
			components.append(node)

##
# Gets a component by type
##
func get_component(type):
	for component in components:
		if component is type:
			return component

##
# Event: Collision
##
func on_collision(collider):
	pass

##
# Takes damage
##
func take_damage(amount, instigator):
	health -= amount

	if health <= 0:
		die()

##
# Kills the Pawn
##
func die():
	queue_free()

##
# Updates the focused object
##
func update_focused_actor():
	focused_actor = null
	focused_position = get_look_base() - get_look_direction() * line_of_sight

	var space_state = get_world().get_direct_space_state()
	var hit = space_state.intersect_ray(get_look_base(), focused_position, [ self ])

	if !hit.empty():
		focused_actor = hit.collider
		focused_position = hit.position

##
# Get look base
##
func get_look_base():
	return get_global_transform().origin + get_global_transform().basis.y

##
# Get look direction
##
func get_look_direction():
	return get_global_transform().basis.z

##
# Moves towards a target
##
func move_towards(target):
	move_direction = (target - get_global_transform().origin).normalized()

##
# Moves towards the focus
##
func move_towards_focus():
	move_towards(focused_position)

##
# Stops moving
##
func stop_moving():
	move_direction = Vector3(0, 0, 0)

##
# Updates the velocity
##
func update_velocity(delta):
    # Reset velocity
	var velocity = Vector3(0, 0, 0)

    # Get physics information
	var gravity_direction = PhysicsServer.area_get_param(get_world().get_space(), PhysicsServer.AREA_PARAM_GRAVITY_VECTOR)
	var collider = null

    # Apply movement
	velocity = move_direction * movement_speed

    # Apply air control
	if !is_on_floor():
		velocity *= air_control

    # Apply jump modifier
	if jumping_modifier > 0:
		velocity -= gravity_direction * jumping_modifier

		jumping_modifier -= delta * 9.8

    # If we hit the ceiling, cancel the jump delta
	if is_on_ceiling():
		jumping_modifier = 0

    # Move with floor
	if is_on_floor():
		velocity += get_floor_velocity()

		has_double_jumped = false

    # Apply gravity
	velocity += gravity_direction * 9.8
    # Apply velocity
	velocity = move_and_slide(velocity, -gravity_direction, 0, 4, floor_angle_max)

##
# Performs a jump
##
func jump():
	if is_on_floor():
		jumping_modifier = jumping_force

	elif can_double_jump && !has_double_jumped && jumping_modifier > jumping_force * 0.5:
		has_double_jumped = true
		jumping_modifier = jumping_force

