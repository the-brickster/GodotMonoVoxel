extends "res://src/weapons/firemodes/WeaponFireMode.gd"

var burstCounter = 3
var burstDelay = 0.5
var delayCounter = burstDelay

func onFireModeChange():
	pass

func canShoot():
	if(isTriggerPressed == true && fireCounter >= fireRate && burstCounter > 0 && isShootable == true):
		fireCounter = 0
		burstCounter -= 1
		return true
	if(burstCounter <=0):
		burstCounter = 3
		isTriggerPressed = false
		isShootable = false
		delayCounter = 0.0

	return false

func triggerPressed():
	if(delayCounter >= burstDelay):
		isTriggerPressed = true
		isShootable = true

func triggerReleased():
#	isTriggerPressed = false;
#	burstCounter = 3
	pass

func update(delta):
	fireCounter +=delta
	if(isShootable == false):
		delayCounter += delta