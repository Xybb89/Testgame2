using Godot;
using Godot.Collections;
using System;
using System.Globalization;
using GameLogic;

public partial class AStarGrid : Node3D 	
{
    private struct Point3
    {
        public decimal X;
        public decimal Y;
        public decimal Z;

        public Point3(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }
    }
    private AStar3D _astar3D = new AStar3D();
    private float _stepSize = 2.0f;

    private Dictionary<Point3, long> _pointLibrary = new Dictionary<Point3, long>();

    [Export]
    public float StepSize { get => _stepSize; set => _stepSize = value; }

    public override void _Ready()
    {
		var pathableNodes = GetTree().GetNodesInGroup("pathable");

        FillGrid(pathableNodes);
		ConnectPoints();
    }

	private void FillGrid(Array<Node> pathableNodes)
	{
		foreach(Node pathableNode in pathableNodes)
		{
			MeshInstance3D mesh = pathableNode.GetNode<MeshInstance3D>("MeshInstance3d");
			Aabb aabb = mesh.GetAabb(); //Axis-Aligned Bounding Box

			Vector3 gridOrigin = aabb.Position + mesh.GlobalPosition;

            int xStepAmount = (int)(aabb.Size.X / StepSize);
            int zStepAmount = (int)(aabb.Size.Z / StepSize);

            AddPoints(gridOrigin, xStepAmount, zStepAmount);
		}
	}

    private void AddPoints(Vector3 gridOrigin, int xStepAmount, int zStepAmount)
    {
        for (int x = 0; x < xStepAmount; x++)
        {
            for (int z = 0; z < zStepAmount; z++)
            {
                Vector3 point = gridOrigin + new Vector3(x * _stepSize, 0, z * _stepSize);
                AddPoint(point);
            }
        }
    }

    private void AddPoint(Vector3 point)
    {
        long id = _astar3D.GetAvailablePointId();
        _astar3D.AddPoint(id, point);
        _pointLibrary.Add(PointSnappedToIntersection(point), id);
    }

    private Point3 PointSnappedToIntersection(Vector3 point)
    {
        decimal x = roundToNearestMultipleOf(point.X, StepSize);
        decimal y = roundToNearestMultipleOf(point.Y, StepSize);
        decimal z = roundToNearestMultipleOf(point.Z, StepSize);

        return new Point3(x, y, z);
    }

    private decimal roundToNearestMultipleOf(float value, float divider)
    {
        return Decimal.Round((decimal)value / (decimal)divider) *  (decimal)divider;
    }

    private void ConnectPoints()
    {
       foreach (var point in _pointLibrary)
       {
            Vector3 worldPosition = point.Key.ToVector3();

            float[] searchOffset = {-StepSize, 0, StepSize};
       }
    }
}
