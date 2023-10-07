using System;
using System.Numerics;

namespace GameLogic
{
    public class GridGD : Grid
    {
        public GridGD(float stepSize) : base(stepSize)
        {
        }

        public void AttachNewGrid (Godot.Vector3 gridOrigin, Godot.Vector3 gridSize)
        {
            base.AttachNewGrid(ToNumericsVector3(gridOrigin), ToNumericsVector3(gridSize));
        }

        public new Godot.Vector3 GetGridPointCenterCoordinates(Vector3 point)
        {
            return new Godot.Vector3(_stepSize/2, 0, _stepSize/2);
        }

        private Vector3 ToNumericsVector3(Godot.Vector3 vector3)
        {
            Vector3 numericsVector3 = new Vector3(
                vector3.X,
                vector3.Y,
                vector3.Z
            );
            return numericsVector3;
        }
    }
}