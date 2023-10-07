using System;
using System.Numerics;
using System.Collections.Generic;

namespace GameLogic
{
    public interface IGrid
    {
        void AttachNewGrid(System.Numerics.Vector3 gridOrigin, System.Numerics.Vector3 gridSize);
        void AttachNewGrid(Godot.Vector3 gridOrigin, Godot.Vector3 gridSize);

        Vector3 GetGridPointCenterCoordinate();
        void test();
    }
}