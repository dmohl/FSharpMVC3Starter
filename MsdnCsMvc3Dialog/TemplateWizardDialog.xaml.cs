using System.Windows;

namespace MsdnCsMvc3Dialog
{
    public partial class TemplateWizardDialog : Window
    {
        public TemplateWizardDialog()
        {
            InitializeComponent();
        }

        public bool IncludeTestsProject { get; protected set; }
        public string SelectedViewEngine { get; protected set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedViewEngine = cbViewEngine.SelectionBoxItem.ToString();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
