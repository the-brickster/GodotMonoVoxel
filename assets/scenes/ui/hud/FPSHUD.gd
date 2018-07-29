extends Node




onready var healthCounter = get_node("MarginContainer/Hbox/LifeArea/VBoxContainer/HealthLabel")
onready var fireMode = get_node("MarginContainer/Hbox/FireModeArea/VBoxContainer/FireModeText")
onready var magCounter = get_node("MarginContainer/Hbox/MagAmmoArea/VBoxContainer/AmmoCounter")
onready var ammoCounter = get_node("MarginContainer/Hbox/TotalAmmoArea/VBoxContainer/TotalAmmoCounter")

func _ready():
	pass

func updateHUDValues(fireMode, magCounter, ammoCounter):
	self.fireMode.text = str(fireMode)
	self.magCounter.text = str(magCounter)
	self.ammoCounter.text = str(ammoCounter)

func connectWeapon(weapon):
	print("weapon node %s"%[weapon])
	weapon.connect("updateFPSGUI",self,"updateHUDValues")
	weapon.setHUDValues()