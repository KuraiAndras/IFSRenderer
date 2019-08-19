using System;
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
using IFSEngine.Helper;
using IFSEngine.Animation;

namespace WpfDisplay.Controls
{
    /// <summary>
    /// Interaction logic for TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        private const double activeAreaStart = 0.01;
        private const double activeAreaEnd = 0.99;

        private AnimationManager animationManager;
        private bool isMouseDown = false;
        private float currentMaximumTime = 10f;

        public TimeLine()
        {

            Loaded += (s, e) =>
            {
                animationManager = ((RendererGL)Application.Current.Windows.OfType<MainWindow>().First().DataContext).AnimationManager;
                CreateLines();

                void CreateLines()
                {
                    int secs = 10;
                    Create(TimeSlider, true);
                    Create(Dopesheet, false);
                    void Create(Canvas parentCanvas, in bool differentHeight)
                    {
                        for (int i = 0; i < secs + 1; i++)
                        {
                            var bigLine = new Line
                            {
                                X1 = 0,
                                Y1 = differentHeight ? parentCanvas.ActualHeight / 3 : 0,
                                X2 = 0,
                                Y2 = parentCanvas.ActualHeight,
                                Stroke = Brushes.Black,
                                StrokeThickness = 2,
                                Opacity = 0.5
                            };
                            PlaceLine(parentCanvas, ((double)i / secs), bigLine);
                        }

                        for (int i = 0; i < secs; i++)
                        {
                            var halfLine = new Line
                            {
                                X1 = 0,
                                Y1 = differentHeight ? 2 * parentCanvas.ActualHeight / 3 : 0,
                                X2 = 0,
                                Y2 = parentCanvas.ActualHeight,
                                Stroke = Brushes.Black,
                                StrokeThickness = 1,
                                Opacity = 0.5
                            };
                            PlaceLine(parentCanvas, (double)i / secs + 0.5 / secs, halfLine);


                            var quartersLine1 = new Line
                            {
                                X1 = 0,
                                Y1 = differentHeight ? 3 * parentCanvas.ActualHeight / 4 : 0,
                                X2 = 0,
                                Y2 = parentCanvas.ActualHeight,
                                Stroke = Brushes.Black,
                                StrokeThickness = 0.5,
                                Opacity = 0.5
                            };
                            PlaceLine(parentCanvas, ((double)i / secs + 0.25 / secs), quartersLine1);


                            var quartersLine2 = new Line
                            {
                                X1 = 0,
                                Y1 = differentHeight ? 3 * parentCanvas.ActualHeight / 4 : 0,
                                X2 = 0,
                                Y2 = parentCanvas.ActualHeight,
                                Stroke = Brushes.Black,
                                StrokeThickness = 0.5,
                                Opacity = 0.5
                            };
                            PlaceLine(parentCanvas, ((double)i / secs + 0.75 / secs), quartersLine2);
                        }


                    }

                    void PlaceLine(Canvas parent, double normalizedLeftOffset, Line lineToPlace)
                    {
                        Canvas.SetLeft(lineToPlace, MapToActiveArea(normalizedLeftOffset) * parent.ActualWidth);
                        parent.Children.Insert(0, lineToPlace);
                    }
                }
            };
            InitializeComponent();
        }

        private double MapToActiveArea(double normalizedOriginalValue) => normalizedOriginalValue.Remap(0, 1, activeAreaStart, activeAreaEnd);
        private double MapFromActiveArea(double normalizedOriginalValue) => normalizedOriginalValue.Remap(activeAreaStart, activeAreaEnd, 0, 1);


        private void TimeSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void TimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            ManipulateTime(e.GetPosition(this));

            //TimeSlider.w
            //e.ButtonState
            //e.GetPosition(this)
        }

        private void ManipulateTime(Point relativePositon)
        {
            var normalizedT = (relativePositon.X / this.ActualWidth).Clamp(activeAreaStart, activeAreaEnd);
            SetDopesheetLinePosition();
            SetAnimatorTime();

            void SetDopesheetLinePosition()
            {
                Canvas.SetLeft(DopesheetLine, normalizedT * this.ActualWidth);
                Canvas.SetLeft(ParentDopesheetLine, normalizedT * this.ActualWidth);
                Canvas.SetLeft(TimeSliderLine, normalizedT * this.ActualWidth);
            }

            void SetAnimatorTime()
            {
                animationManager.EvaluateAt(MapFromActiveArea(normalizedT) * currentMaximumTime);
            }
        }

        private void TimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void TimeSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
                ManipulateTime(e.GetPosition(this));
        }



        private void TimeSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }
    }
}
