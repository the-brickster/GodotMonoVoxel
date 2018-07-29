extends "res://src/weapons/firemodes/WeaponFireMode.gd"


func onFireModeChange():
    pass

func canShoot():
    if(isTriggerPressed == true && fireCounter >= fireRate):
        fireCounter = 0
        return true
    return false

func triggerPressed():
    isTriggerPressed = true
    isShootable = true

func triggerReleased():
    isTriggerPressed = false;
    isShootable = false;

func update(delta):
    fireCounter +=delta