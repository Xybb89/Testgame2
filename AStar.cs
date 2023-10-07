using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using GameLogic;
//using Grid;

public partial class AStar : Node3D 	
{
	[Export]
	public bool DrawNavigationCubes = false;
	[Export]
	public bool DrawGridCell = true;
	private float _gridStep = 2.0f;
	private float _gridY = 0.5f;

	private Vector3 _worldOrigin;

	private Godot.Collections.Dictionary<string, long> _points = new Godot.Collections.Dictionary<string, long>(); 

	private AStar3D _astar = new AStar3D(); 

	private GridGD _grid;

	private BoxMesh _cubeMesh = new BoxMesh();
	private BoxMesh _lineMeshHorizontal = new BoxMesh();
	private BoxMesh _lineMeshVertical = new BoxMesh();
	private StandardMaterial3D _redMaterial = new StandardMaterial3D();
	private StandardMaterial3D _greenMaterial = new StandardMaterial3D();
	private StandardMaterial3D _yellowMaterial = new StandardMaterial3D();
	private StandardMaterial3D _blackMaterial = new StandardMaterial3D();

	private List<MeshInstance3D> _highlightedCells = new List<MeshInstance3D>();

	private bool _doOnce = true;
	private bool _worldOriginExists = false;

    public float GridStep { get => _gridStep; set => _gridStep = value; }

    public override void _Ready()
	{	
		_grid = new GridGD(_gridStep);

		_redMaterial.AlbedoColor = Color.FromString("red", Color.Color8(255, 255, 255, 0));
		_greenMaterial.AlbedoColor = Color.FromString("green", Color.Color8(255, 255, 255, 0));
		_yellowMaterial.AlbedoColor = Color.FromString("yellow", Color.Color8(255, 255, 255, 0));
		_blackMaterial.AlbedoColor = Color.FromString("black", Color.Color8(255, 255, 255, 0));
		_cubeMesh.Set("size", new Vector3(GridStep/4, GridStep/4, GridStep/4));
		_lineMeshHorizontal.Set("size", new Vector3(GridStep, 0.1f, GridStep/10));
		_lineMeshVertical.Set("size", new Vector3(GridStep/10, 0.1f, GridStep));

		var pathables = GetTree().GetNodesInGroup("pathable");
		addPoints(pathables);
		connectPoints();
	}

	private void addPoints(Array<Node> pathables)
	{
		foreach(Node pathable in pathables)
		{
			//Get bounding box around ground
			MeshInstance3D mesh = pathable.GetNode<MeshInstance3D>("MeshInstance3D");		
			Aabb aabb = mesh.GetAabb(); //Axis-Aligned Bounding Box

			//The AABB (Bounding Box) position is relative to MeshInstance3D
			Vector3 startPoint = aabb.Position + mesh.GlobalPosition;

			GD.Print("StepSize: " + _grid.StepSize);
			GD.Print(startPoint);

			if (!_worldOriginExists)
			{
				_worldOrigin = startPoint;
				_worldOriginExists = true;
			}

			int xSteps = (int)(aabb.Size.X / GridStep);
			int zSteps = (int)(aabb.Size.Z / GridStep);

			for (int x = 0; x < xSteps; x++)
			{
				for(int z = 0; z < zSteps; z++)
				{
					Vector3 nextPoint = startPoint + new Vector3(x * GridStep, 0, z * GridStep);
					addPoint(nextPoint);
					if (nextPoint.X < _worldOrigin.X && nextPoint.Z < _worldOrigin.Z)
						_worldOrigin = nextPoint;
				}
			}
        }
	}									   

	private void addPoint(Vector3 point)
	{
		point.Y = _gridY;

		long id = _astar.GetAvailablePointId();
		//_astar._ComputeCost(12, 14);
		_astar.AddPoint(id, point);
		_points.Add(worldToAStar(point), id);

		createNavigationCube(point);

		if (_doOnce)
		{
			createGridCell(point);
			//_doOnce = false;
		}
	}

	private string worldToAStar(Vector3 worldPoint)
	{
		decimal x = roundToNearestMultipleOf(worldPoint.X, GridStep);
		decimal y = roundToNearestMultipleOf(worldPoint.Y, GridStep);
		decimal z = roundToNearestMultipleOf(worldPoint.Z, GridStep);

		//GD.Print(x + "," + y + "," + z);

		return x + "," + y + "," + z;
	}

	private decimal roundToNearestMultipleOf(float value, float nearest)
	{
		return Decimal.Round((decimal)value / (decimal)nearest) *  (decimal)nearest;
	}

	private void connectPoints()
	{
		foreach (var point in _points)
		{
			Vector3 worldPosition = convertStringToVector3(point.Key);

			float[] searchOffset = {-GridStep, 0, GridStep};
			foreach (float x in searchOffset)
			{
				foreach (float z in searchOffset)
				{
					Vector3 currentOffset = new Vector3(x, 0, z);
					if (currentOffset != Vector3.Zero)
					{
						string potentialNeighbour = worldToAStar(worldPosition + currentOffset);
						if (_points.ContainsKey(potentialNeighbour))
						{
							long currentID = _points[point.Key];
							long neighbourID = _points[potentialNeighbour];
							if (!_astar.ArePointsConnected(currentID, neighbourID))
								_astar.ConnectPoints(currentID, neighbourID);			
						}
					}		
				}				
			}
		}	
	} 

	private Vector3 convertStringToVector3(string positionAsString)
	{
		string[] position = positionAsString.Split(",");
		Vector3 worldPositon = new Vector3(
			float.Parse(position[0], CultureInfo.InvariantCulture.NumberFormat),
			float.Parse(position[1], CultureInfo.InvariantCulture.NumberFormat),
			float.Parse(position[2], CultureInfo.InvariantCulture.NumberFormat)
		);
		return worldPositon;
	}	
	
	public List<Vector3> FindPath(Vector3 start, Vector3 end)
	{
		long startID = _astar.GetClosestPoint(start);
		long endID = _astar.GetClosestPoint(end);

		List<Vector3> path = new List<Vector3>();
		path.AddRange(_astar.GetPointPath(startID, endID));
		return path;
	}

	private void createNavigationCube(Vector3 position)
	{
		if (DrawNavigationCubes)
		{
			MeshInstance3D cube = new MeshInstance3D();
			AddChild(cube);
			cube.Mesh = _cubeMesh;
			cube.MaterialOverride = _redMaterial;

			position.Y = _gridY;
			cube.GlobalPosition = position + new Vector3(GridStep/2, 0, GridStep/2);
		}
	}

	private void createGridCell(Vector3 position)
	{
		if (DrawGridCell)
		{
			for (int x = 0; x <= 1; x++) 
			{
				for (int z = 0; z <= 1; z++)
				{
					MeshInstance3D cube = new MeshInstance3D();
					AddChild(cube);

					cube.CastShadow = 0;
					cube.MaterialOverride = _blackMaterial;

					position.Y = 0.2f;
					if (z == 0)
					{
						cube.Mesh = _lineMeshVertical;
						cube.GlobalPosition = position + new Vector3(x * GridStep, 0, GridStep / 2);
						//GD.Print(cube.GlobalPosition.X + " ; " + cube.GlobalPosition.Z + " ; " + " z = " + z);
					}						
					else
					{
						cube.Mesh = _lineMeshHorizontal;
						cube.GlobalPosition = position + new Vector3(GridStep / 2, 0, x * GridStep);
						//GD.Print(cube.GlobalPosition.X + " ; " + cube.GlobalPosition.Z + " ; " + " z = " + z);
					}

				}

			}
		}
	}
	
	public void highlightGridCell(Vector3 position)
	{
		if (_highlightedCells.Count > 0)
			foreach (MeshInstance3D meshInstance in _highlightedCells)
			{
				meshInstance.QueueFree();
			}
		_highlightedCells.Clear();

		for (int x = 0; x <= 1; x++) 
			{
				for (int z = 0; z <= 1; z++)
				{
					MeshInstance3D cube = new MeshInstance3D();
					AddChild(cube);
					_highlightedCells.Add(cube);

					cube.CastShadow = 0;
					cube.MaterialOverride = _yellowMaterial;

					position = convertStringToVector3(worldToAStar(position));
					position.Y = 0.3f;

					if (z == 0)
					{
						cube.Mesh = _lineMeshVertical;
						cube.GlobalPosition = position + new Vector3(x * GridStep, 0, GridStep / 2);
						//GD.Print(cube.GlobalPosition.X + " ; " + cube.GlobalPosition.Z + " ; " + " z = " + z);
					}						
					else
					{
						cube.Mesh = _lineMeshHorizontal;
						cube.GlobalPosition = position + new Vector3(GridStep / 2, 0, x * GridStep);
						//GD.Print(cube.GlobalPosition.X + " ; " + cube.GlobalPosition.Z + " ; " + " z = " + z);
					}
				}
			}
	}

	//does not work when meshInstances are not connected
	public Vector3 WorldPositionToGridCoordinates(Vector3 position)
	{
		Vector3 _origin = _worldOrigin;
		_origin += new Vector3(_gridStep/2, 0, _gridStep/2);
		_origin = convertStringToVector3(worldToAStar(_origin));

		position += new Vector3(_gridStep/2, 0, _gridStep/2);
		position = convertStringToVector3(worldToAStar(position));
		return (position - _origin)/_gridStep;
	}

	public Vector3 GetCenterFromGridCoordinate(Vector3 coordinate)
	{
		return new Vector3(coordinate.X + _gridStep/2, coordinate.Y, coordinate.Z + _gridStep/2);
	}

	public Vector3 AlignCoordinatesWithGrid(Vector3 coordinate)
	{
		//GD.Print(coordinate + " pre");
		coordinate = convertStringToVector3(worldToAStar(coordinate));
		//GD.Print(coordinate + " after");
		return new Vector3(coordinate.X + _gridStep/2, coordinate.Y, coordinate.Z + _gridStep/2);
	}
}
