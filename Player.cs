using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
	// Don't forget to rebuild the project so the editor knows about the new export variable.

    // How fast the player moves in meters per second.
	[Export]
	public int MousePositionRayLength = 2000;
	[Export]
	public float MaximumDistanceToTarget = 0.2f;
    [Export]
    public int Speed { get; set; } = 14;
    // The downward acceleration when in the air, in meters per second squared.
    [Export]
    public int FallAcceleration { get; set; } = 75;

	[Export]
	public int JumpImpulse { get; set; } = 20;

	[Export]
	public int BounceImpulse { get; set; } = 16;

	[Export]
	public double MouseRepeatDelay = 0.2;

	[Signal]
	public delegate void HitEventHandler();


	private double _timeElapsedSinceRightClick = 0;
	private bool _rightMouseButtonReleased = true;
    private Vector3 _targetVelocity = Vector3.Zero;
	private Vector3 _targetPosition =  Vector3.Zero;
	private List<Vector3> _pathToTarget = new List<Vector3>();

	private bool _isAtTargetPosition = false;

	private AStar _astar;

	private Vector3 _mousePosition = Vector3.Zero;

	public override void _Ready()
	{
		_astar = GetNode<AStar>("../AStar");
	}

	public override void _PhysicsProcess(double delta)
	{
		// We create a local variable to store the input direction.
		Vector3 direction = Vector3.Zero;

		//Player player = GetNode<Player>("Player");

		// We check for each move input and update the direction accordingly.
		if (Input.IsActionPressed("move_right"))
		{
			direction.X += 1.0f;
		}
		if (Input.IsActionPressed("move_left"))
		{
			direction.X -= 1.0f;
		}
		if (Input.IsActionPressed("move_down"))
		{
			// Notice how we are working with the vector's X and Z axes.
			// In 3D, the XZ plane is the ground plane.
			direction.Z += 1.0f;
		}
		if (Input.IsActionPressed("move_up"))
		{
			direction.Z -= 1.0f;
		}
		if (direction != Vector3.Zero)
		{
			direction = direction.Normalized();
			direction = direction.Rotated(Vector3.Up, GetNode<Camera3D>("../Camera3D").Rotation.Y);

			//GD.Print(GetNode<Camera3D>("../Camera3D").Rotation.Y);
			//GetNode<Node3D>("Player").LookAt(Position + direction, Vector3.Up);
		}

		_mousePosition = screenPointToRay();
		_astar.highlightGridCell(new Vector3(_mousePosition.X - _astar.GridStep/2, _mousePosition.Y,_mousePosition.Z - _astar.GridStep/2));

		if (Input.IsActionPressed("mouse_right"))
		{
			if (_timeElapsedSinceRightClick >= MouseRepeatDelay && _rightMouseButtonReleased)
			{
				_timeElapsedSinceRightClick = 0;
				_targetPosition = new Vector3(_mousePosition.X - _astar.GridStep/2, _mousePosition.Y,_mousePosition.Z - _astar.GridStep/2);
				//_targetPosition = _astar.GetCenterFromGridCoordinate(_mousePosition);
				//GD.Print(_astar.WorldPositionToGridCoordinates(_mousePosition));
				_pathToTarget = _astar.FindPath(Position, _targetPosition);	
				
				_targetPosition = _astar.AlignCoordinatesWithGrid(new Vector3(_mousePosition.X - _astar.GridStep/2, _mousePosition.Y,_mousePosition.Z - _astar.GridStep/2));
				GD.Print("_targetPosition: " + _targetPosition);
				//GD.Print("Rechts.");
			}
			_rightMouseButtonReleased = false;
		}
		else
			_rightMouseButtonReleased = true;

		if (!isAtPosition(_targetPosition, MaximumDistanceToTarget))
		{
			Vector3 _nextTargetPosition = _targetPosition;
			GD.Print("Distance to target: " + (_targetPosition - Position) + " IsAtTarget: " + isAtPosition(_targetPosition, MaximumDistanceToTarget) + " Pathing to :" + _pathToTarget[0] + " Current Position: " + Position + " Target Position: " + _targetPosition);	
			if (_pathToTarget.Count > 0)
				_nextTargetPosition = _pathToTarget[0];
			if (_pathToTarget.Count > 1)
			{
				//Vector3 _nextTargetPosition = _pathToTarget[0]; //_astar.AlignCoordinatesWithGrid(new Vector3(_pathToTarget[0].X - _astar.GridStep/2, _pathToTarget[0].Y,_pathToTarget[0].Z - _astar.GridStep/2));
				//GD.Print(_pathToTarget[0] +" ; " + _astar.AlignCoordinatesWithGrid(new Vector3(_pathToTarget[0].X - _astar.GridStep/2, _pathToTarget[0].Y,_pathToTarget[0].Z - _astar.GridStep/2)));
				if(isAtPosition(_nextTargetPosition, MaximumDistanceToTarget))
				{
					_pathToTarget.Remove(_pathToTarget[0]);
					//GD.Print("Pathing to :" + _pathToTarget[0] + " Current Position: " + Position + " Target Position: " + _targetPosition);
					
				}		
			}
			direction.X = _nextTargetPosition.X - Position.X;
			direction.Y = 0;
			direction.Z = _nextTargetPosition.Z - Position.Z;

			direction = direction.Normalized();		

		}

		//GD.Print(_astar.WorldPositionToGridCoordinates(_mousePosition));


		_targetVelocity.X = direction.X * Speed * (float)delta;
		_targetVelocity.Z = direction.Z * Speed * (float)delta;

		GD.Print("Stepsize X: " + _targetVelocity.X + "Stepsize Z: " + _targetVelocity.Z);

		_targetVelocity.Y = 0;

		// Moving the character
		Velocity = _targetVelocity;
		//GD.Print(Velocity.Length());
		GD.Print(Position + "PRE");
		Position += Velocity;
		GD.Print(Position + "POST");

		//GD.Print("Distance to target: " + (_targetPosition - Position));
		//MoveAndSlide();

		if (_timeElapsedSinceRightClick < MouseRepeatDelay)
			_timeElapsedSinceRightClick += delta;
	}
	private Vector3 screenPointToRay()
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Camera3D camera = GetTree().Root.GetCamera3D();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * MousePositionRayLength; 
		var rayArray = spaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd, 2));

		if (rayArray.ContainsKey("position"))
			return (Vector3)rayArray["position"];
		return Vector3.Zero;
	}

	private bool isAtPosition(Vector3 targetPosition, float maxDeltaToTarget)
	{
		if (Math.Abs(Position.X - targetPosition.X) <= maxDeltaToTarget && Math.Abs(Position.Z - targetPosition.Z) <= maxDeltaToTarget)
		{
			return true;
		}
		return false;
	}
}
