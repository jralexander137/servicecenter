using System;
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
using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace ToolKit_Cheese
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            clearText();
            //programListView.Items.Add("Hello");
        }



        private void queryBtn_Click(object sender, RoutedEventArgs e)
        {
            dnsText.Text = "";
            try
            {
                checkConnection(hostnameTextBox.Text);
                userQuery(hostnameTextBox.Text);
                OSquery(hostnameTextBox.Text);
                compQuery(hostnameTextBox.Text);
                diskQuery(hostnameTextBox.Text);
                biosQuery(hostnameTextBox.Text);
                networkQuery(hostnameTextBox.Text);
                dcQuery(hostnameTextBox.Text);
                //getPrograms(hostnameTextBox.Text);
            }
            catch (Exception ex)
            {
                clearText();
            }
        }

        private void getProgramBtn_Click(object sender, RoutedEventArgs e)
        {
            getProgramsReg(hostnameTextBox.Text);
            //getPrograms(hostnameTextBox.Text);
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            clearText();
        }

        /////////////////////////////////////////////////
        ///                METHODS                    ///
        /////////////////////////////////////////////////

        private void checkConnection(String hostname)
        {
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send(hostname);

                if (reply.Status == IPStatus.Success)
                {
                    ipAddressText.Text = reply.Address.ToString();
                }//end if

            }//end try
            catch (Exception p)
            {
                MessageBox.Show("Target host is not connected to the network! Please try another host.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);

            }//end catch

        }//end checkConnection

        private void OSquery(String hostname)
        {
            ManagementObjectSearcher host = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT * FROM Win32_OperatingSystem");
            Console.Write(hostname);
            foreach (ManagementObject os in host.Get())
            {
                if (os["ServicePackMajorVersion"].ToString() == "0")
                {
                    osVersionText.Text = os["Caption"].ToString();

                }
                else
                {
                    osVersionText.Text = os["Caption"].ToString() + " Service Pack " + os["ServicePackMajorVersion"].ToString();
                }//end if else

                DateTime installDate = ManagementDateTimeConverter.ToDateTime(os.Properties["InstallDate"].Value.ToString());
                DateTime lastBoot = ManagementDateTimeConverter.ToDateTime(os.Properties["LastBootUpTime"].Value.ToString());
                TimeSpan upDuration = DateTime.Now - lastBoot;

                installDateText.Text = installDate.ToString();
                upTimeText.Text = upDuration.ToString("%d") + " days " + upDuration.ToString("%h") + " hours " + upDuration.ToString("%m") + " minutes";
                architectureText.Text = os["OSArchitecture"].ToString();

            }//end foreach

            RegistryKey rkey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostname);
            RegistryKey rkeySoftware = rkey.OpenSubKey("Software");
            RegistryKey rkeyMicrosoft = rkeySoftware.OpenSubKey("Microsoft");
            RegistryKey rkeyIE = rkeyMicrosoft.OpenSubKey("Internet Explorer");

            var ieVersion = rkeyIE.GetValue("Version").ToString();
            var ieSvcVersion = rkeyIE.GetValue("svcVersion").ToString();

            ieVersionText.Text = ieVersion;
            ieSvcVersionText.Text = ieSvcVersion;

        }//end OSQuery

        private void getProgramsReg(String hostname)
        {
            String key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey rkey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostname);
            RegistryKey rkeySoftware = rkey.OpenSubKey(key);
            //RegistryKey rkeyMicrosoft = rkeySoftware.OpenSubKey("Microsoft");
            // RegistryKey rkeyIE = rkeyMicrosoft.OpenSubKey("Internet Explorer");
            programListView.Items.Add("Program Name");
            foreach (String prog in rkeySoftware.GetSubKeyNames())
            {
                RegistryKey progName = rkeySoftware.OpenSubKey(prog);
                programListView.Items.Add(progName.GetValue("DisplayName"));


            }//end for each


        }//end getProgramsReg

        private void userQuery(String hostname)
        {
            ManagementObjectSearcher user = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT LogonId FROM Win32_LogonSession WHERE LogonType=2");
            if (user.Get().Count == 0)//checks if query returned any rows. If 0 then no user currently logged in.
                userText.Text = "None";
            foreach (ManagementObject us in user.Get())
            {
                ManagementObjectSearcher inuser = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "Associators of {Win32_LogonSession.LogonId=" + us["LogonId"] + "} Where AssocClass=Win32_LoggedOnUser Role=Dependent");
                foreach (ManagementObject inus in inuser.Get())
                {
                    userText.Text = inus["Name"].ToString();
                }//end inner foreach
            }//end outer foreach
        }//end userQuery

        private void compQuery(String hostname)
        {
            ManagementObjectSearcher comp = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject pc in comp.Get())
            {
                manufacturerText.Text = pc["Manufacturer"].ToString();
                modelText.Text = pc["Model"].ToString();
                ramText.Text = Math.Round((Convert.ToDecimal(pc["TotalPhysicalMemory"]) / 1073741824), 2).ToString() + " GB";
            }

        }//end biosQuery

        private void diskQuery(String hostname)
        {
            ManagementObjectSearcher disk = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'");
            foreach (ManagementObject hd in disk.Get())
            {
                freeSpaceHDDText.Text = Math.Round((Convert.ToDecimal(hd["FreeSpace"]) / 1073741824), 2).ToString() + " GB";
                sizeHDDText.Text = Math.Round((Convert.ToDecimal(hd["Size"]) / 1073741824), 2).ToString() + " GB";
            }//end for each


        }//end diskQuery

        private void biosQuery(String hostname)
        {
            ManagementObjectSearcher bios = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT SerialNumber FROM Win32_Bios");
            foreach (ManagementObject sn in bios.Get())
            {
                serialNumberText.Text = sn["SerialNumber"].ToString();
            }//end foreach

        }//end biosQuery

        private void networkQuery(String hostname)
        {
            ManagementObjectSearcher netwrk = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
            foreach (ManagementObject net in netwrk.Get())
            {
                String[] IPsubnet = (String[])net["IPSubnet"];
                subnetText.Text = IPsubnet[0].ToString();

                String[] IPgateway = (String[])net["DefaultIPGateway"];
                gatewayText.Text = IPgateway[0].ToString();

                String[] DNSserver = (String[])net["DNSServerSearchOrder"];
                for (int i = 0; i < DNSserver.Length; i++)
                {
                    dnsText.Text += DNSserver[i].ToString() + " ";
                }//end for

                macText.Text = net["MACAddress"].ToString();
                dhcpText.Text = net["DHCPEnabled"].ToString();

            }//end foreach
        }//end networkQuery

        private void dcQuery(String hostname)
        {
            ManagementObjectSearcher domain = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT DomainControllerName FROM Win32_NTDomain WHERE DomainName = 'ghs'");
            foreach (ManagementObject dc in domain.Get())
            {
                logonServerText.Text = dc["DomainControllerName"].ToString();
            }//end foreach
        }//end dcQuery

        private void getPrograms(String hostname)
        {
            ManagementObjectSearcher program = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT Caption FROM Win32_Product");
            foreach (ManagementObject pro in program.Get())
            {
                programListView.Items.Add(pro["Caption"]);
            }//end foreach
        }//end getPrograms


        private void clearText()
        {
            ipAddressText.Text = "";
            osVersionText.Text = "";
            installDateText.Text = "";
            upTimeText.Text = "";
            architectureText.Text = "";
            ieVersionText.Text = "";
            ieSvcVersionText.Text = "";
            userText.Text = "";
            manufacturerText.Text = "";
            modelText.Text = "";
            ramText.Text = "";
            freeSpaceHDDText.Text = "";
            sizeHDDText.Text = "";
            serialNumberText.Text = "";
            subnetText.Text = "";
            gatewayText.Text = "";
            dnsText.Text = "";
            macText.Text = "";
            dhcpText.Text = "";
            logonServerText.Text = "";
        }//end clearText

    }//end MainWindow

}//end namespace
