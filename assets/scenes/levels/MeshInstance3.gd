extends MeshInstance

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

# Called when the node enters the scene tree for the first time.
func _ready():
	self.get_surface_material(0).flags_use_point_size = true
	self.get_surface_material(0).params_point_size = 1.0
#	var surf = SurfaceTool.new()
#	surf.begin(Mesh.PRIMITIVE_POINTS)
#	surf.create_from(self.mesh,0)
#
#	self.mesh = surf.commit()

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
