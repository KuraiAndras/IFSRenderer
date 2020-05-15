﻿using System;
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
using WpfDisplay.ViewModels;

namespace WpfDisplay.Views
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        public Node()
        {
            InitializeComponent();
        }

        private int tx, ty;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            e.Handled = true;

            var vm = (IteratorViewModel)DataContext;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //vm.IsSelected = true;
                vm.SelectNode();
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                tx = vm.XCoord - (int)e.GetPosition(null).X;//vm.XCoord - e.GetPosition(Map).X;
                ty = vm.YCoord - (int)e.GetPosition(null).Y;//vm.YCoord - e.GetPosition(Map).Y;
            }


        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            e.Handled = true;

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            e.Handled = true;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                var vm = (IteratorViewModel)DataContext;
                e.Handled = true;
                vm.XCoord = /*e.GetPosition(Map).X + */tx + (int)e.GetPosition(null).X;
                vm.YCoord = /*e.GetPosition(Map).Y + */ty + (int)e.GetPosition(null).Y;
                vm.RefreshView();
            }
        }

        /*
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(IteratorViewModel), new PropertyMetadata(default(ICommand)));
        */


    }
}
