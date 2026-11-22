using System;
using System.Windows.Forms;
using PorteriaApp.UI;

namespace PorteriaApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
