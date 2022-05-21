using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Windows;

namespace SupportHelper.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected async void ContentLoaded(object sender, RoutedEventArgs e)
        {

            await Task.Run(() =>
            {

                String patch = GetPatch();
                String users = GetUsers();

                this.Dispatcher.Invoke(() =>
                {

                    LABEL_NAME.Content = Environment.MachineName;
                    LABEL_VERSION.Content = Environment.OSVersion.Version.ToString();
                    LABEL_PATCH.Content = patch;
                    LABEL_USERS.Content = users;
                });

            });
        }

        private String GetPatch()
        {
            PowerShell PowerShellInst = PowerShell.Create();
            PowerShellInst.AddScript(@"
                    Get-HotFix | Select-Object -Property Description, HotFixID, InstalledOn
            ");

            String output = "";

            try
            {
                Collection<PSObject> PSOutput = PowerShellInst.Invoke();
                output += "--------------------------------------------------------------\n";
                output += String.Format(
                            "{0,-20}{1,-20}{2,-20}\n",
                            "Description", "HotFixID", "InstalledOn");
                output += "--------------------------------------------------------------\n";
                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        System.Diagnostics.Debug.WriteLine(obj);
                        output += String.Format(
                            "{0,-20}{1,-20}{2,-20}\n",
                            obj.Properties["Description"].Value,
                            obj.Properties["HotFixID"].Value,
                            obj.Properties["InstalledOn"].Value
                            );

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return output;
        }

        private String GetUsers()
        {


            PowerShell PowerShellInst = PowerShell.Create();

            PowerShellInst.AddScript(@"
                $path = 'Registry::HKey_Local_Machine\Software\Microsoft\Windows NT\CurrentVersion\ProfileList\*'
                $items = Get-ItemProperty -path $path
                Foreach ($item in $items) {
                    $objUser = New-Object System.Security.Principal.SecurityIdentifier($item.PSChildName)
                    $objName = $objUser.Translate([System.Security.Principal.NTAccount])
                    $item.PSChildName = $objName.value
                }
                $items | Where {-not ($_.PSChildName -like 'NT AUTHORITY*')} | Select-Object -Property PSChildName
            ");


            String output = "";

            try
            {

                Collection<PSObject> PSOutput = PowerShellInst.Invoke();


                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {

                        output += String.Format(
                            "{0,-60}\n",
                            obj.Properties["PSChildName"].Value
                            );
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return output;
        }
    }
}
