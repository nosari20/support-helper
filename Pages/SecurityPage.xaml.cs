using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.Principal;
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

namespace SupportHelper.Pages
{
    /// <summary>
    /// Interaction logic for SecurityPage.xaml
    /// </summary>
    public partial class SecurityPage : Page
    {
        public SecurityPage()
        {
            InitializeComponent();
        }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected async void ContentLoaded(object sender, RoutedEventArgs e)
        {

           
            await Task.Run(() =>
            {
                String bitlocker = GetBitLocker();


                this.Dispatcher.Invoke(() =>
                {

                    LABEL_BITLOCKER.Content = bitlocker;


                });

            });
        }



        private String GetBitLocker()
        {

            if (!IsAdministrator())
            {
                return "Administrative privileges required.";
            }

            PowerShell PowerShellInst = PowerShell.Create();
            PowerShellInst.AddScript(@"
                    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted 
                    Import-Module C:\Windows\System32\WindowsPowerShell\v1.0\Modules\BitLocker  
                    Get-BitLockerVolume -MountPoint C
                 
                    
            ");

            String output = "";
            try
            {
                Collection<PSObject> PSOutput = PowerShellInst.Invoke();


                NameValueCollection KeyProtectorTypes = new NameValueCollection()
                {
                    { "0", "Unknown or other protector type" },
                    { "1", "Trusted Platform Module (TPM)" },
                    { "2", "External key" },
                    { "3", "Numerical password" },
                    { "4", "TPM And PIN" },
                    { "5", "TPM And Startup Key" },
                    { "6", "TPM And PIN And Startup Key" },
                    { "7", "Public Key" },
                    { "8", "Passphrase" },
                    { "9", "TPM Certificate" },
                    { "10", "CryptoAPI Next Generation (CNG) Protector" }
                };

                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        System.Diagnostics.Debug.WriteLine(obj.Properties);
                        output += "EncryptionMethod: " + obj.Properties["EncryptionMethod"].Value + "\n";
                        output += "CapacityGB: " + obj.Properties["CapacityGB"].Value + "\n";
                        output += "AutoUnlockEnabled: " + obj.Properties["AutoUnlockEnabled"].Value + "\n";
                        output += "AutoUnlockKeyStored: " + obj.Properties["AutoUnlockKeyStored"].Value + "\n";
                        output += "EncryptionPercentage: " + obj.Properties["EncryptionPercentage"].Value + "\n";
                        
                        output += "KeyProtector: \n";
                        Newtonsoft.Json.Linq.JArray KeyProtectors = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject<Object>(JsonConvert.SerializeObject(obj.Properties["KeyProtector"].Value));
                        foreach (Newtonsoft.Json.Linq.JObject obj2 in KeyProtectors)
                        {
                            output += "\t" + KeyProtectorTypes.Get((string)obj2.GetValue("KeyProtectorType")) + "\n";
                        }
                        output += "LockStatus: " + obj.Properties["LockStatus"].Value + "\n";
                        output += "MountPoint: " + obj.Properties["MountPoint"].Value + "\n";
                        output += "ProtectionStatus: " + obj.Properties["ProtectionStatus"].Value + "\n";
                        output += "VolumeType: " + obj.Properties["VolumeType"].Value + "\n";
                        output += "WipePercentage: " + obj.Properties["WipePercentage"].Value + "\n";


                    }
                }



            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
                
            
            return output;
        }

        public static void var_dump(object obj)
        {
            using (var writer = new System.IO.StringWriter())
            {
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                System.Diagnostics.Debug.WriteLine(json.ToString());
            }
        }
    }

}
