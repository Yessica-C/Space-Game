extends Node3D

@export var panning_sensitivity = 0.05
@export var mouse_rotation_speed: float = .01
@export var keyboard_rotation_speed: float = 5
@export var max_vertical_angle: float = 1.5 #in radians

var is_rotating: bool = false
var last_mouse_pos: Vector2

func _input(event):
	if event is InputEventMouseButton:
		if Input.is_action_pressed("cam_rotate"):
			is_rotating = event.pressed
			if is_rotating:
				last_mouse_pos = event.position
			else:
				Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	
	elif event is InputEventMouseMotion and is_rotating:
		var delta = event.relative
		rotate_x(-delta.y * mouse_rotation_speed)
		
	# Keep Z rotation (roll) at 0 to prevent rolling
	rotation.x = clamp(rotation.x, -max_vertical_angle, max_vertical_angle)
	rotation.z = 0
	rotation.y = 0
	


func _process(delta: float) -> void:
	
	#keyboard camera rotate
	if Input.is_action_pressed("rotate_up"):
		rotate_x(keyboard_rotation_speed * delta)
	if Input.is_action_pressed("rotate_down"):
		rotate_x(-keyboard_rotation_speed * delta)
	rotation.x = clamp(rotation.x, -max_vertical_angle, max_vertical_angle)
	rotation.z = 0
	rotation.y = 0
	
	#rotation toggle
	if Input.is_action_pressed("cam_rotate"):
		if not is_rotating:
			is_rotating = true
	else:
		if is_rotating:
			is_rotating = false
