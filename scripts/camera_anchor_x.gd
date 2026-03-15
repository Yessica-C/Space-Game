extends Node3D

@export var rotational_sensitivity: float = 0.01
@export var panning_sensitivity = 0.05
@export var rotation_speed: float = 1.0
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
		rotate_x(-delta.y * rotational_sensitivity * rotation_speed)
		
	# Keep Z rotation (roll) at 0 to prevent rolling
	var clamped_x = clamp(rotation.x, -max_vertical_angle, max_vertical_angle)
	rotation.x = clamped_x
	rotation.z = 0
	rotation.y = 0
	


func _process(delta: float) -> void:
	if Input.is_action_pressed("cam_rotate"):
		if not is_rotating:
			is_rotating = true
	else:
		if is_rotating:
			is_rotating = false
