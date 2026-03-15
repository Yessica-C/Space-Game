extends Camera3D
		
@export var mouse_sensitivity = 0.0075

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func _input(event):
	
	#zoom handling
	var mouse_input = 0.0
	if(Input.is_action_just_pressed("zoom_out")):
		mouse_input += 1.5
	if(Input.is_action_just_pressed("zoom_in")):
		mouse_input -= 1.5
	var mouse_dir = global_transform.basis.z * mouse_input
	global_position -= mouse_dir
	
