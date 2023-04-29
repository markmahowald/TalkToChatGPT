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

namespace ChatWithChatGpt
{
    /// <summary>
    /// Interaction logic for NewConversationMessageBox.xaml
    /// </summary>
    public partial class NewConversationMessageBox : Window
    {
        public NewConversationMessageBox()
        {
            InitializeComponent();
            this.InputTextBox.Text = "Assistant";
        }
        public string UserInput => InputTextBox.Text;

        public string RoleInput { get; private set; }

        private void StartChattingButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.RoleInput = this.InputTextBox.Text;
        }
    }
}

