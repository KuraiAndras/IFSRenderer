using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace IFSEngine.Animation
{
    public class ControlPoint
    {
        public double t;
        public float Value;
        public Vector2 LeftTangent; //maybe angle enough
        public Vector2 RightTangent;
    }
}
