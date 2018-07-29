extends Node

var fireModeName = ""
var isTriggerPressed = false
var isShootable = false
var fireRate = 0.0
var fireCounter = 0.0

func onFireModeChange():
    pass

func canShoot():
    if(isTriggerPressed == true && isShootable == true):
        isShootable = false
        return true
    return false

func triggerPressed():
    isTriggerPressed = true
    isShootable = true

func triggerReleased():
    isTriggerPressed = false
    isShootable = false

func update(delta):
    pass