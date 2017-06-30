using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MirrorConfigClient.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        #region ctor
        public AboutView()
        {
            InitializeComponent();
            DataContext = this;
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        #endregion

        #region Hyperlink_RequestNavigate
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        #endregion

        #region Version
        public string Version
        {
            get; private set;
        }
        #endregion
    }
}
