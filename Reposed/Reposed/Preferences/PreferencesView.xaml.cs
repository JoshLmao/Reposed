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
using System.Windows.Shapes;

namespace Reposed.Preferences
{
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesView : Window
    {
        public PreferencesView()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            c_slackGrid.MouseDown += OnGridMouseDown;
        }

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            c_slackGrid.MouseDown -= OnGridMouseDown;
        }

        private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            c_slackGrid.Focus();
            Keyboard.ClearFocus();
        }
    }
}
