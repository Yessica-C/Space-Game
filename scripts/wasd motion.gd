extends RigidBody3D
# Movement parameters
@export var move_speed: float = 10
@export var deceleration_factor: float = 0.99 # 1% deceleration per frame
@export var min_velocity: float = 0.1  # Minimum velocity before stopping completely

# auto rotation parameters
@export var self_rotation_speed: float = 10.0 # How quickly the body rotates toward the target direction
@export var torque_multiplier: float = 2
@export var min_velocity_threshold: float = 0.1 # Minimum threshold for linear velocity to trigger rotation
@export var alignment_threshold: float = 5 # Angle threshold in degrees to consider the body aligned with velocity direction

@export var target_location: Vector3 = Vector3(30, 20, 10)


func _ready():
	angular_damp = 0.5
	linear_damp = 0.5	
	var timer = Timer.new()
	timer.wait_time = 1.0
	timer.connect("timeout", _print_transform, 1)
	timer.autostart = true	
	add_child(timer)

func _print_transform():
	print("---------------------------------")
	print("Global Position:\t", global_position)
	print("Global Rotation:\t", global_rotation)
	print("Local Quaternion:\t", self.quaternion)
		
# Get the direction from the rigidbody to the target
func _is_pointing_at(location: Vector3) -> bool:
	var direction_to_target = (location - global_transform.origin).normalized()
	# Get the forward direction of the rigidbody (typically the Z-axis)
	var forward_direction = global_transform.basis.z.normalized()
	# Calculate the angle between the directions
	var angle = acos(clamp(forward_direction.dot(direction_to_target), -1.0, 1.0))
	# Convert to degrees
	var angle_degrees = rad_to_deg(angle)
	# Check if the angle is greater than the tolerance (not pointing within 5 degrees)
	var is_not_pointing_at_target = angle_degrees > alignment_threshold
	# Use the condition as needed
	if is_not_pointing_at_target:
		return false
	else:
		return true
		
func _process(delta):
	pass
	
func _get_rotation_to_face(direction: Vector3) -> Quaternion:
	# Create a basis that points in the direction of 'direction'
	var forward = direction.normalized()
	var up = Vector3(0, 1, 0)  # Assuming Y-up coordinate system

	# Handle case where forward is parallel to up
	if abs(forward.dot(up)) > 0.99:
		up = Vector3(1, 0, 0)

	var right = forward.cross(up).normalized()
	up = right.cross(forward).normalized()

	var basis = Basis(right, up, forward)
	return basis.get_rotation_quaternion()
	
func _update_rotation(): #vibe coded as hell
	# Get current global rotation as Euler angles
	var current_rotation = global_rotation
	# Get current forward direction (assuming Y-up coordinate system)
	var direction_to_target = (target_location - global_position).normalized()
	# Calculate the rotation difference (using quaternions for smooth rotation)
	var current_quat = Transform3D(Basis.from_euler(current_rotation)).basis.get_rotation_quaternion()
	var target_quat = _get_rotation_to_face(direction_to_target)
	# Calculate the rotation difference
	var rotation_diff = target_quat * current_quat.inverse()
	# Apply torque (simplified approach)
	var torque = rotation_diff.get_euler() * torque_multiplier

	# Apply the torque
	apply_torque(torque)

func _physics_process(delta):
	
	if !_is_pointing_at(target_location):
		_update_rotation()
	if Input.is_action_pressed("move_forward"):
		apply_central_force(-move_speed * transform.basis.z)
	if Input.is_action_pressed("move_backward"):
		apply_central_force(move_speed * transform.basis.z)
	if Input.is_action_pressed("rotate_left"):
		apply_torque(self_rotation_speed * transform.basis.y)
	if Input.is_action_pressed("rotate_right"):
		apply_torque(-self_rotation_speed * transform.basis.y)
	if Input.is_action_pressed("rotate_up"):
		apply_torque(self_rotation_speed * transform.basis.x)
	if Input.is_action_pressed("rotate_down"):
		apply_torque(-self_rotation_speed * transform.basis.x)
	
func _integrate_forces(state: PhysicsDirectBodyState3D) -> void:
	pass
