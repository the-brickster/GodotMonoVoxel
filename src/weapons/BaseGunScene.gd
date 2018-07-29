extends "res://src/weapons/ViewableWeapon.gd"

signal updateFPSGUI(fireMode,magValue,ammoValue)
#EXPORT VARIABLES
#External Scenes
export(PackedScene) var weaponHitEffect
export(PackedScene) var ejectingBrass
#External Variables
export(int) var fireRate = 100
export(int) var magSize = 30
export(int) var maxAmmo = 300
export(int) var brassCount = 100
export(int) var tracerIndicator = 3
export(float) var weaponSpread = 1
export(float) var weaponRange =100
export(float) var tracerStartThickness = 0.01
export(float) var tracerEndThickness = 0.01
export(Color) var startTracerColor = Color(244, 66, 66)
export(Color) var endTracerColor = Color(244, 66, 66)

#Internal Variables
var maxSpriteFrames = 0
var currSpriteFrame = 0
var pressTrigger = false
var nextTimeToFire = 0
var currMagAmmo = magSize
var currMaxAmmo = maxAmmo
var fireCounter = 0.0
var mouseShootPos = null
var activeState = null
var tracerCounter = 0


enum WEAPON_STATE{
	IDLE,
	SHOOT,
	RELOAD
}

#Weapon Fire Mode Variables
onready var fireModes = {"Semi":preload("res://src/weapons/firemodes/WeaponFireMode.gd").new(),
"Auto":preload("res://src/weapons/firemodes/AutomaticFireMode.gd").new(),
"Burst":preload("res://src/weapons/firemodes/BurstFireMode.gd").new()}
var activeFireMode = null
var fireModeIndex = 1


onready var muzzleFlash = get_node("MuzzleFlash")
onready var muzzleFlashSprite = get_node("MuzzleFlash/MuzzleSprite")
onready var shootingPoint = get_parent().get_parent().get_node("Camera")
onready var parentFPSController = get_parent().get_parent().get_parent().get_parent()
onready var GunEjection = get_node("EjectionPort")
onready var TracerPoint = get_node("TracerPoint")


func _ready():
	print("Loaded Gun Controller")
	muzzleFlash.hide()
	set_process(true)
	set_process_input(true)
	set_physics_process(true)
	setup()
	Mesh.new()

func _physics_process(delta):
	shoot_weapon(delta)
	

func _process(delta):
	self.activeFireMode.update(delta)
	
	
func setHUDValues():
	emit_signal("updateFPSGUI",activeFireMode.fireModeName,currMagAmmo,currMaxAmmo)

func setup():
	maxSpriteFrames = muzzleFlashSprite.vframes * muzzleFlashSprite.hframes
	print("Shooting node: %s %s"%[shootingPoint.get_name(),fireRate])
	if(ejectingBrass != null):
		GlobalGameSystemsManager.RegisterGamePoolObj(ejectingBrass,brassCount)
	nextTimeToFire = 60.0/fireRate
	self.currMagAmmo = magSize
	self.currMaxAmmo = maxAmmo
	self.activeFireMode = fireModes[fireModes.keys()[fireModeIndex]]
	self.activeFireMode.fireModeName = fireModes.keys()[fireModeIndex]
	self.activeFireMode.fireRate = nextTimeToFire
	
#	var root = get_tree().get_root()
#	GlobalGameSystemsManager.createTracerLine(root,shootingPoint,0.01,0.01,startTracerColor,endTracerColor)
	

func shoot_weapon(delta):
	if(activeFireMode.canShoot()):
#	if(pressTrigger == true && fireCounter >= nextTimeToFire):
				
		if(currSpriteFrame >= maxSpriteFrames):
			currSpriteFrame = 0
		muzzleFlashSprite.set_frame(currSpriteFrame)
		#		var muzzleFlashSize = rand_range(0.5,1)
		#		muzzleFlash.scale = Vector3(muzzleFlashSize, muzzleFlashSize,muzzleFlashSize)
		currSpriteFrame+=1
		fireCounter = 0
		muzzleFlash.show()
		var root = get_tree().get_root()
		
		var hitEffectNode = null
		
		
		
		var space_state = get_world().get_direct_space_state()
		var globalShPPos = shootingPoint.project_ray_origin(mouseShootPos)
		var endPos = globalShPPos + shootingPoint.project_ray_normal(mouseShootPos) * weaponRange
		endPos.x = endPos.x + rand_range(-weaponSpread,weaponSpread)
		endPos.y = endPos.y + rand_range(-weaponSpread,weaponSpread)
		endPos.z = endPos.z + rand_range(-weaponSpread,weaponSpread)
		var result = space_state.intersect_ray(globalShPPos,endPos,[parentFPSController])
				
		#		var im = ImmediateGeometry.new()
		#		var begin = get_parent().get_node("ShootPosTest")
		#		matTest.set_line_width(100.0)
		##		print("%s line width: %s"%[begin.to_global(begin.translation),matTest.get_line_width()])
		#		im.set_material_override(matTest)
		#		im.clear()
		#		im.begin(Mesh.PRIMITIVE_LINES,null)
		#		im.add_vertex(begin.to_global(begin.translation))
		#		im.add_vertex(endPos)
		#		im.end()
		#		root.add_child(im)
		#		print("position: %s ; normal: %s ; collider %s; shooting point %s" % [result.position, result.normal, result.collider, shootingPoint.project_ray_normal(mouseShootPos)])
				
				
		#		print(hitEffectNode.rotation_deg)
		# var tracerPos = to_global(TracerPoint.translation)
		# GlobalGameSystemsManager.createTracerLine(root,shootingPoint,tracerStartThickness,tracerEndThickness,startTracerColor,
		# endTracerColor,tracerPos,result.position,50,10.0)
		if(tracerCounter == tracerIndicator):
			tracerCounter = 0;
			self.createTracer(root,shootingPoint,TracerPoint.translation,result.position,10,100.0)
#		GlobalGameSystemsManager.updateTracerLine(tracerPos,result.position,100)
		self.createHitEffect(root,result)
		# if(weaponHitEffect != null):
		# 	hitEffectNode = weaponHitEffect.instance()
		# 	hitEffectNode.global_translate(result.position)
		# 	hitEffectNode.look_at_from_position(result.position,Vector3(0,1,0),result.normal)
		# 	root.add_child(hitEffectNode)
		self.ejectBrass(root,endPos)
		tracerCounter+=1
# 		if(ejectingBrass != null):
# 			var globalEjecPos = self.to_global(GunEjection.translation)
# 			var brass = GlobalGameSystemsManager.AcquirePoolObject("EjectingBrassTest")
# 			#		var brass = ejectingBrass.instance()
# 			if(brass != null):
# 				if(!root.is_a_parent_of(brass)):
# 					root.add_child(brass)
# #				var parentVelocity = parentFPSController.velocity
# 				brass.global_translate(globalEjecPos)
# 				brass.look_at(endPos,Vector3(0,1,0))
# 				var forward = (endPos-globalEjecPos).normalized()
# 				var force = rand_range(0.1,1)
# 				var rightVec = (forward.cross(Vector3(0,1,0)))*force
# 				brass.apply_impulse(Vector3(0,0,0),rightVec)
		#		print(hitEffectNode.rotation_deg)
				
		#		var dir = endPos - globalShPPos
		#		var b = bulletTest.instance()
		#		b.speed = 2000.0
		#		b.projectileDirection = dir
		#		b.origin = globalShPPos
		#		b.projectileHitEffect = hitEffectNode
		##		print("origin: %s dir: %s -"%[globalShPPos,dir])
		#		root.add_child(b)
		##
		#		b.look_at_from_position(globalShPPos,dir,Vector3(0,1,0))
		#
		#		hitEffectNode.global_transform.origin = result.position
#		emit_signal("updateFPSGUI",activeFireMode.fireModeName,currMagAmmo,currMaxAmmo)
	else:
		muzzleFlash.hide()
	
	fireCounter = fireCounter+delta

func createTracer(node,camPos,startPos,endPos,numSegments,life):
	var tracerPos = to_global(startPos)
	GlobalGameSystemsManager.createTracerLine(node,camPos,tracerStartThickness,
	tracerEndThickness,startTracerColor,endTracerColor,tracerPos,endPos,50,10.0)

func createHitEffect(node,result):
	if(weaponHitEffect != null):
		var hitEffectNode = weaponHitEffect.instance()
		node.add_child(hitEffectNode)
		hitEffectNode.global_translate(result.position)
		hitEffectNode.look_at_from_position(result.position,Vector3(0,1,0),result.normal)

func ejectBrass(node, endPos):
	if(ejectingBrass != null):
		var globalEjecPos = self.to_global(GunEjection.translation)
		var brass = GlobalGameSystemsManager.AcquirePoolObject("EjectingBrassTest")
#		var brass = ejectingBrass.instance()
		if(brass != null):
			if(!node.is_a_parent_of(brass)):
				node.add_child(brass)
			var parentVelocity = parentFPSController.velocity
			brass.global_translate(globalEjecPos)
#			brass.transform.basis.x 
			brass.look_at(endPos,Vector3(0,1,0))
			var forward = (endPos-globalEjecPos).normalized()+ parentVelocity
			var force = rand_range(0.1,1)
			var rightVec = (forward.cross(Vector3(0,1,0)))*force
			brass.apply_impulse(Vector3(0,0,0),rightVec)

func handle_shoot(event):
	if(event.is_action_pressed("primary_attack")):
#		pressTrigger = true
		activeFireMode.triggerPressed()
		mouseShootPos = event.position
#		print("%s fireCounter, %s nextTimeToFire"%[fireCounter,nextTimeToFire])
	elif(event.is_action_released("primary_attack")):
#		pressTrigger = false
		activeFireMode.triggerReleased()
	elif(event.is_action_pressed("toggle_fire_mode")):
		fireModeIndex+=1
		var size = fireModes.keys().size()
		if(fireModeIndex>=size):
			fireModeIndex = 0
		activeFireMode = fireModes[fireModes.keys()[fireModeIndex]]
		activeFireMode.fireModeName = fireModes.keys()[fireModeIndex]
		self.activeFireMode.fireRate = nextTimeToFire
		emit_signal("updateFPSGUI",activeFireMode.fireModeName,currMagAmmo,currMaxAmmo)
		

func _on_FPSController_mouse_movement(mouseMove):
	self.mouseMove = mouseMove



