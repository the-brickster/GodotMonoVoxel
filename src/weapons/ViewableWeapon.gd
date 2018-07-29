extends Spatial

#InspectorVariables
export(float) var moveAmount = 0
export(float) var smoothAmount = 0
export(float) var maxAmount = 0

#Local Variables
var initialPosition = ""
var mouseMove = Vector2()


func _ready():
	initialPosition = Vector3(get_transform().origin)
	set_process_input(true)
	set_physics_process(true)
func _input(event):
	handle_shoot(event)

func handle_shoot(event):
	pass

func _physics_process(delta):
	move_weapon(delta)

	
func move_weapon(delta):
	var movementX = -mouseMove.x * moveAmount
	var movementY = -mouseMove.y * moveAmount
	
	movementX = clamp(movementX, -maxAmount, maxAmount)
	movementY = clamp(movementY, -maxAmount, maxAmount)
	
	var finalPosition = Vector3(movementX, movementY,0)
	var curr = get_transform().origin
	var lerpPos = self.initialPosition+finalPosition
	curr = curr.linear_interpolate(lerpPos,delta*smoothAmount)
	set_translation(curr)