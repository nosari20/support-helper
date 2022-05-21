using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace SupportHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();



        }

        public async void Window_ContentRendered(object sender, EventArgs e)
        {

            await LoadBasicInfo();
        }

        private async Task LoadBasicInfo()
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
            Collection<PSObject> PSOutput = PowerShellInst.Invoke();

            String output = "";
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

            Collection<PSObject> PSOutput = PowerShellInst.Invoke();

            String output = "";

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

            return output;
        }
    }
}
