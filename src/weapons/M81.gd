extends "res://src/weapons/ViewableWeapon.gd"

var maxSpriteFrames = 0
var currSpriteFrame = 0
var isShooting = false
var fireRate = 1000
var nextTimeToFire = 60.0/fireRate
var fireCounter = 0.0
var weaponRange = 100.0
var weaponHitEffect = preload("res://assets/scenes/effects/SparkParticleRound.tscn")
var weaponHitDecal = null
var mouseShootPos = null
var weaponSpread = 1.0
var matTest = SpatialMaterial.new()

var ejectingBrass = preload("res://assets/scenes/weapons/ejectingBrass/EjectingBrassTest.tscn")
var bulletTest = preload("res://assets/scenes/projectiles/BulletTest.tscn")

onready var muzzleFlash = get_node("MuzzleFlash")
onready var muzzleFlashSprite = get_node("MuzzleFlash/MuzzleFlashSprite")
onready var shootingPoint = get_parent().get_parent().get_node("Camera")
onready var parentFPSController = get_parent().get_parent().get_parent().get_parent()
onready var GunEjection = get_node("GunEjection")


func _ready():
	muzzleFlash.hide()
	set_process(true)
	set_process_input(true)
	set_physics_process(true)
	setup()

func setup():
	maxSpriteFrames = muzzleFlashSprite.vframes * muzzleFlashSprite.hframes
	print("Shooting node: %s"%[shootingPoint.get_name()])
	GlobalGameSystemsManager.RegisterGamePoolObj(ejectingBrass,1000)

func _physics_process(delta):
	shoot_weapon(delta)

func shoot_weapon(delta):
	if(isShooting == true && fireCounter >= nextTimeToFire):
#		print("%s fireCounter, %s nextTimeToFire"%[fireCounter,nextTimeToFire])
		
		if(currSpriteFrame >= maxSpriteFrames):
			currSpriteFrame = 0
		muzzleFlashSprite.set_frame(currSpriteFrame)
#		var muzzleFlashSize = rand_range(0.5,1)
#		muzzleFlash.scale = Vector3(muzzleFlashSize, muzzleFlashSize,muzzleFlashSize)
		currSpriteFrame+=1
		fireCounter = 0
		muzzleFlash.show()
		var root = get_tree().get_root()
		var hitEffectNode = weaponHitEffect.instance()
		
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
		hitEffectNode.global_translate(result.position)
		hitEffectNode.look_at_from_position(result.position,Vector3(0,1,0),result.normal)
		root.add_child(hitEffectNode)
		var globalEjecPos = self.to_global(GunEjection.translation)
		var brass = GlobalGameSystemsManager.AcquirePoolObject("EjectingBrassTest")
#		var brass = ejectingBrass.instance()
		if(brass != null):
			if(!root.is_a_parent_of(brass)):
				root.add_child(brass)
			var parentVelocity = parentFPSController.velocity
			brass.global_translate(globalEjecPos)
			brass.look_at(endPos,Vector3(0,1,0))
			var forward = (endPos-globalEjecPos).normalized()
			var force = rand_range(0.1,1)
			var rightVec = (forward.cross(Vector3(0,1,0))+parentVelocity)*force
			brass.apply_impulse(Vector3(0,0,0),rightVec)
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
	else:
		muzzleFlash.hide()

	fireCounter = fireCounter+delta

func handle_shoot(event):
	if(event.is_action_pressed("primary_attack")):
		isShooting = true
		mouseShootPos = event.position
	elif(event.is_action_released("primary_attack")):
		isShooting = false
		print("Mouse movement released")

func _on_FPSController_mouse_movement( mouseMove ):
	print("Got callback in here"+str(mouseMove))
	self.mouseMove = mouseMove
