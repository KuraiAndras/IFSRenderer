using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IFSEngine;
using IFSEngine.Animation;

namespace WpfDisplay.Controls
{
    /// <summary>
    /// Interaction logic for TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        private AnimationManager animationManager;
        public TimeLine()
        {
            
            //Loaded += (s, e) =>
            //{
            //    animationManager = ((RendererGL)Application.Current.Windows.OfType<MainWindow>().First().DataContext).AnimationManager;
            //    var button = new Button();
            //    Canvas.SetLeft(button, 40);
            //    Dopesheet.Children.Add(button);
            //};
            InitializeComponent();
        }

        private void TimeSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void TimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.ButtonState
            //e.GetPosition(this)
        }

        private void TimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void TimeSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void SetAnimatorTime(float t)
        {

        }

    }
}
