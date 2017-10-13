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
        }


        private void queryBtn_Click(object sender, RoutedEventArgs e)
        {
            checkConnection(hostnameTextBox.Text);
            OSquery(hostnameTextBox.Text);
            compQuery(hostnameTextBox.Text);
        }

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
                    osVersionText.Text = os["Caption"].ToString() + " " + os["ServicePackMajorVersion"].ToString();
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

        private void compQuery(String hostname)
        {
            ManagementObjectSearcher comp = new ManagementObjectSearcher("\\\\" + hostname + "\\root\\cimv2", "SELECT * FROM Win32_ComputerSystem");
            foreach(ManagementObject pc in comp.Get())
            {
                manufacturerText.Text = pc["Manufacturer"].ToString();
                modelText.Text = pc["TotalPhysicalMemory"].ToString();
            }

        }//end biosQuery

    }//end MainWindow

}//end namespace
