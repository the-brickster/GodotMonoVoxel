extends MeshInstance

# class member variables go here, for example:
# var a = 2
# var b = "textvar"

func _ready():
	# Called every time the node is added to the scene.
	# Initialization here
	var mat = SpatialMaterial.new()
	mat.flags_use_point_size= true
	mat.params_point_size = 1.0
	
	
	var capsule = self.get_parent().get_child(1)
	var surf = SurfaceTool.new()
	
	surf.begin(Mesh.PRIMITIVE_POINTS)
	surf.create_from(capsule.mesh,0)

	
	self.mesh = surf.commit()
	self.set_surface_material(0,mat)

	

#func _process(delta):
#	# Called every frame. Delta is time since last frame.
#	# Update game logic here.
#	pass
