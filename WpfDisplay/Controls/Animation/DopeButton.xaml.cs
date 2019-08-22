using IFSEngine.Animation;
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

namespace WpfDisplay.Controls.Animation
{
    /// <summary>
    /// Interaction logic for DopeButton.xaml
    /// </summary>
    public partial class DopeButton : UserControl
    {
        private TimeLine timeLine;
        private ControlPoint controlPoint;
        public DopeButton()
        {
            Loaded += (s, e) =>
            {

            };
            InitializeComponent();
        }

        public void SetControlPoint(ControlPoint controlPoint) => this.controlPoint = controlPoint;

    }
}
