using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WindowCustomizeTest
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //CreateCustomWindow();
        }

        
        private void CreateCustomWindow()
        {
            Window window = new Window()
            {
                Title = "CustomWindow",
                Width = 1440,
                Height = 720
            };
            
            window.Show();
        }
    }
}
