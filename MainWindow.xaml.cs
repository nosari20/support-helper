using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace SupportHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private List<Page> PagesIntances = new List<Page>();


        public MainWindow()
        {
            InitializeComponent();

            NavigateHome(null, null);
            if (IsAdministrator())
            {
                BUTTON_RUNAS.Visibility = Visibility.Hidden;
            }

        }


        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo adminProcess = new System.Diagnostics.ProcessStartInfo();
                adminProcess.UseShellExecute = true;
                adminProcess.WorkingDirectory = System.Environment.CurrentDirectory;
                adminProcess.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                adminProcess.Verb = "runas";

                System.Diagnostics.Process.Start(adminProcess);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NavigateHome(object sender, RoutedEventArgs e)
        {
            foreach(Page page in PagesIntances)
            {
                if(page is Pages.HomePage){
                    MainFrame.Content = page;
                    return;
                }
            }
            Page newpage = new Pages.HomePage();
            MainFrame.Content = (newpage);
        }

        private void NavigateSecurity(object sender, RoutedEventArgs e)
        {
            foreach (Page page in PagesIntances)
            {
                if (page is Pages.SecurityPage)
                {
                    MainFrame.Content = page;
                    return;
                }
            }
            Page newpage = new Pages.SecurityPage();
            MainFrame.Content = (newpage);
        }



    }
}
