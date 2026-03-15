extends RigidBody3D

# Movement parameters
@export var acceleration: float = .25
@export var max_speed: float = 10.0
@export var rotation_speed: float = 2.0
@export var deceleration: float = 0.99

# Internal variables
var forward_force: Vector3 = Vector3.ZERO
var rotation_target: Vector3 = Vector3.ZERO

func _ready():
	# Set up the rigidbody for zero-gravity
	gravity_scale = 0.0
	angular_damp = 0.5  # Add some angular damping for smoother rotation

func _process(delta):
	# Handle forward movement
	if Input.is_action_pressed("move_forward"):
		forward_force += transform.basis.z * acceleration * delta
	else:
		# Apply deceleration when not moving forward
		forward_force *= deceleration
	
	# Limit maximum speed
	if forward_force.length() > max_speed:
		forward_force = forward_force.normalized() * max_speed
	
	# Handle rotation controls
	rotation_target = Vector3.ZERO
	
	if Input.is_action_pressed("move_left"):
		rotation_target.x -= rotation_speed * delta
	if Input.is_action_pressed("move_right"):
		rotation_target.x += rotation_speed * delta
	if Input.is_action_pressed("move_up"):
		rotation_target.y -= rotation_speed * delta
	if Input.is_action_pressed("move_down"):
		rotation_target.y += rotation_speed * delta

func _integrate_forces(state):
	# Apply forward force
	state.apply_central_force(forward_force)
	
	# Apply rotation
	var desired_angular_velocity = Vector3.ZERO
	angular_velocity.x = rotation_target.x
	angular_velocity.y = rotation_target.y
	state.angular_velocity = desired_angular_velocity
