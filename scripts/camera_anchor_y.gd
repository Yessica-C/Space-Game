extends Node3D

@export var mouse_panning_sensitivity = 0.1
@export var keyboard_panning_sensitivity = 100
@export var mouse_rotation_speed: float = 0.01
@export var keyboard_rotation_speed: float = 5
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
		rotate_y(-delta.x * mouse_rotation_speed)
	rotation.x = 0
	rotation.z = 0
	
	#panning
	if (Input.is_action_pressed("cam_pan") or (Input.is_action_pressed("cam_rotate") and Input.is_action_pressed("cam_pan_modif"))) and event is InputEventMouseMotion:
		
		# Create movement vector in local space
		var movement = Vector3(
			-event.relative.x * mouse_panning_sensitivity,  # Left/right (local x)
			0,  # No vertical movement
			-event.relative.y * mouse_panning_sensitivity  # Forward/backward (local z)
		)
		
		# Transform local movement to global space
		var global_movement = self.global_transform.basis * movement
		
		# Apply movement to global position
		global_position += global_movement

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	
	var camera = get_node("CameraAnchorX/Camera3D")
	var zoom_level = global_position.distance_to(camera.global_position)
	var zoom_factor = zoom_level / 50
	
	# keyboard camera panning
	var movement = Vector3(0, 0, 0)
	if(Input.is_action_pressed("move_left")):
		movement += Vector3(-keyboard_panning_sensitivity, 0, 0)
	if(Input.is_action_pressed("move_right")):
		movement += Vector3(keyboard_panning_sensitivity, 0, 0)
	if(Input.is_action_pressed("move_forward")):
		movement += Vector3(0, 0, -keyboard_panning_sensitivity)
	if(Input.is_action_pressed("move_backward")):
		movement += Vector3(0, 0, keyboard_panning_sensitivity)
	
	var global_movement = self.global_transform.basis * movement * delta * zoom_factor
	global_position += global_movement
	
	#keyboard camera rotation
	if Input.is_action_pressed("rotate_left"):
		rotate_y(keyboard_rotation_speed * delta)
	if Input.is_action_pressed("rotate_right"):
		rotate_y(-keyboard_rotation_speed * delta)
	rotation.x = 0
	rotation.z = 0
	
	#rotation check
	if Input.is_action_pressed("cam_rotate"):
		if not is_rotating:
			is_rotating = true
	else:
		if is_rotating:
			is_rotating = false
