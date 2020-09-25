using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WindowCustomizeTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TestWindow();
            TestGlowWindow();
        }


        private void TestWindow()
        {
            Window window = new Window()
            {
                Title = "Window",
                Width = 1440,
                Height = 720,
                //Owner = this,
            };
            //glowWindow.ShowDialog();
            window.Show();
        }

        private void TestGlowWindow()
        {
            GlowWindow glowWindow = new GlowWindow()
            {
                Title = "GlowWindow",
                Width = 1440,
                Height = 720,
                Owner = this,
            };
            glowWindow.ShowDialog();
            //glowWindow.Show();
        }
    }
}
