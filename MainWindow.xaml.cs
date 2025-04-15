using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace Menu_Start_Position_Switch_win11
{
    public partial class MainWindow : Window
    {
        private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string ValueName = "TaskbarAl";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            
            // Sprawdź czy aplikacja jest już podpisana
            if (!IsApplicationSigned())
            {
                SignApplication();
            }
        }

        private bool IsApplicationSigned()
        {
            try
            {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(Process.GetCurrentProcess().MainModule.FileName);
                return cert != null;
            }
            catch
            {
                return false;
            }
        }

        private void SignApplication()
        {
            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript(@"
                        $cert = New-SelfSignedCertificate `
                            -Subject 'CN=Menu Start Position Switch' `
                            -Type CodeSigningCert `
                            -KeyUsage DigitalSignature `
                            -KeyAlgorithm RSA `
                            -KeyLength 2048 `
                            -CertStoreLocation 'Cert:\CurrentUser\My';
                        
                        $certPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName([System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName), 'MenuStartSwitchCert.cer');
                        Export-Certificate -Cert $cert -FilePath $certPath;
                        
                        Set-AuthenticodeSignature -FilePath ([System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName) -Certificate $cert;
                        
                        $trustedPublisher = 'Cert:\CurrentUser\TrustedPublisher';
                        Move-Item -Path $cert.PSPath -Destination $trustedPublisher;
                    ");
                    ps.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error signing application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentAlignment = GetCurrentAlignment();
                UpdateUI(currentAlignment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading registry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private int GetCurrentAlignment()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                return (int)(key?.GetValue(ValueName, 0) ?? 0);
            }
        }

        private void UpdateUI(int alignment)
        {
            AlignmentToggle.IsChecked = alignment == 1;
            StatusLabel.Text = $"Currently: {(alignment == 1 ? "Centered" : "Left")}";
        }

        private void SetAlignment(int alignment)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key?.SetValue(ValueName, alignment, RegistryValueKind.DWord);
            }
        }

        private void AlignmentToggle_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetAlignment(1);
                UpdateUI(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating registry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AlignmentToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetAlignment(0);
                UpdateUI(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating registry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
