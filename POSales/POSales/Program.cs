using Quiz_App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSales
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Idle += (sender, args) => ModernUI.ApplyOpenForms();

            Home home = new Home();
            ModernUI.Apply(home);
            Application.Run(home);
        }
    }
}
