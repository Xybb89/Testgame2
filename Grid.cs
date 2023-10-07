using System;
using System.Numerics;
using System.Collections.Generic;

namespace GameLogic
{
    public class Grid
    {
        private struct Point3
        {
            public decimal X;
            public decimal Y;
            public decimal Z;

            public Point3 (decimal x, decimal y, decimal z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        protected float _stepSize;

        protected long _pointID;

        private Dictionary<Point3, long> _pointLibrary;

        public float StepSize { get => _stepSize; }

        public Grid(float stepSize)
        {
            _stepSize = stepSize;

            _pointID = 0;

            _pointLibrary = new Dictionary<Point3, long>();
        }

        public void AttachNewGrid (Vector3 gridOrigin, Vector3 gridSize)
        {
           	int xSteps = (int)(gridSize.X / _stepSize);
			int zSteps = (int)(gridSize.Z / _stepSize);

            for (int x = 0; x < xSteps; x++)
            {
                for (int z = 0; z < zSteps; z++)
                {
                    Point3 point3 = new Point3(
                        (decimal)gridOrigin.X + x * xSteps, 
                        (decimal)gridOrigin.Y, 
                        (decimal)gridOrigin.Z + z * zSteps);
                    _pointLibrary.Add(point3, _pointID);

                    _pointID++;
                }
            }
        }

        public Vector3 GetGridPointInLocalCoordinates(Vector3 point)
        {
            return Vector3.Zero; //point - _gridOrigin;
        }

        public Vector3 GetGridPointCenterCoordinates(Vector3 point)
        {
            return point + new Vector3(_stepSize/2, 0, _stepSize/2);
        }

        public Vector3 GetPointAt(int x, int z)
        {
            return Vector3.Zero; //_grid[x,z];
        }
    }
}