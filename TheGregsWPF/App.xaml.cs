using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TheGregsWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Called whenever an excpetion goes unhandled to avoid crashing the
        // app:
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            _ = MessageBox.Show(
                e.Exception.Message,
                "Error: " + e.Exception.GetType().Name,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Stop the exception from crashing the app:
            e.Handled = true;
        }
    }
}
