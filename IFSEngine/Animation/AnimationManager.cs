using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IFSEngine.Helper;

namespace IFSEngine.Animation
{
    public class AnimationManager
    {
        public delegate void AnimationCreatedHandler();
        public delegate void ControlPointCreatedHandler(ControlPoint controlPoint,double animationDuration);

        public event AnimationCreatedHandler OnAnimationCreated;
        public event ControlPointCreatedHandler OnControlPointCreated;
        private List<PropertyAnimation> animations = new List<PropertyAnimation>();
        private PropertyAnimation currentAnimation;
        public void AddNewAnimation(Action<float> applyAction)
        {
            animations.Add(new PropertyAnimation(applyAction));
            currentAnimation = animations[animations.Count - 1];
            OnAnimationCreated?.Invoke();

            CreateControlPoint(0f, 0f);
            CreateControlPoint(10f, 10f);
        }

        private void CreateControlPoint(in double timeInSeconds,in double value)
        {
            var newCP = new ControlPoint { t = new ChangeDetector<double>(timeInSeconds) , Value = new ChangeDetector<double>(value) };
            currentAnimation.AnimationCurve.AddControlPoint(newCP);
            OnControlPointCreated?.Invoke(newCP, currentAnimation.AnimationCurve.GetDuration());
        }

        public void PlayAnimation()
        {

        }

        public void EvaluateAt(in double timeInSeconds)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Animate(timeInSeconds);
            }
        }
    }
}
