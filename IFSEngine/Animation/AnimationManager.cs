using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFSEngine.Animation
{
    public class AnimationManager
    {
        private List<PropertyAnimation> animations = new List<PropertyAnimation>();

        public void AddNewAnimation(Action<float> applyAction)
        {
            animations.Add(new PropertyAnimation(applyAction));

            animations[animations.Count - 1].AnimationCurve.AddControlPoint(new ControlPoint { t = 0, Value = 0f });
            animations[animations.Count - 1].AnimationCurve.AddControlPoint(new ControlPoint { t = 10, Value = 10f });
            //animations[0].Animate(5f);
        }

        public void PlayAnimation()
        {

        }

        public void EvaluateAt(double timeInSeconds)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Animate(timeInSeconds);
            }
        }
    }
}
