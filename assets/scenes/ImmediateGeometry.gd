extends ImmediateGeometry

# class member variables go here, for example:
# var a = 2
# var b = "textvar"

func _ready():
	# Called every time the node is added to the scene.
	# Initialization here
	self.begin(Mesh.PRIMITIVE_LINES,null)
	add_vertex(Vector3(0,0,0))
	add_vertex(Vector3(10,10,10))
	self.end()

#func _process(delta):
#	# Called every frame. Delta is time since last frame.
#	# Update game logic here.
#	pass
