extends Node3D

@export var rotational_sensitivity: float = 0.01
@export var panning_sensitivity = 0.05
@export var rotation_speed: float = 1.0
@export var max_vertical_angle: float = 1.5 #in radians

var is_rotating: bool = false
var last_mouse_pos: Vector2

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

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
		rotate_y(-delta.x * rotational_sensitivity * rotation_speed)
	rotation.x = 0
	rotation.z = 0
	
	#panning
	if (Input.is_action_pressed("cam_pan") or (Input.is_action_pressed("cam_rotate") and Input.is_action_pressed("cam_pan_modif"))) and event is InputEventMouseMotion:
		
		var cam_basis = self.global_transform.basis
		var right = cam_basis.x
		var up = cam_basis.y

		var movement = -right * event.relative.x * panning_sensitivity # I set mine at mouse_sensitivity = 0.0075
		movement -= up * -event.relative.y *  panning_sensitivity
		
		global_translate(movement)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_pressed("cam_rotate"):
		if not is_rotating:
			is_rotating = true
	else:
		if is_rotating:
			is_rotating = false
