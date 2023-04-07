using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SearcherWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpWebRequest _appRequest;
        private HttpWebResponse _appResponse;
        private string _browserModel;
        private string _inputRequest;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (browserModel_ComboBox.SelectedIndex)
            {
                case 0:
                    _browserModel = "GOOGLE";
                    break;
                case 1:
                    _browserModel = "BING";
                    break;
                case 2:
                    _browserModel = "YANDEX";
                    break;
                default:
                    _browserModel = "GOOGLE";
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(searchURL_TextBox.Text.Length == 0)
            {
                searchURL_TextBox.Text = "Hello, World!";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            output_RichTextBox.Document = new FlowDocument(new Paragraph(new Run("")));

            switch(_browserModel)
            {
                case "GOOGLE":
                    _inputRequest = $"https://www.google.com/search?q={searchURL_TextBox.Text}";
                    break;
                case "BING":
                    _inputRequest = $"https://www.bing.com/search?q={searchURL_TextBox.Text}";
                    break;
                case "YANDEX":
                    _inputRequest = $"https://yandex.ru/search/?text={searchURL_TextBox.Text}";
                    break;
                default:
                    _inputRequest = $"https://www.google.com/search?q={searchURL_TextBox.Text}";
                    break;
            }

            _appRequest = (HttpWebRequest)HttpWebRequest.Create($"{_inputRequest}");
            _appResponse = (HttpWebResponse)_appRequest.GetResponse();

            FlowDocument document = ResponseToDocument(_appResponse);

            output_RichTextBox.Document = document;
        }

        private FlowDocument ResponseToDocument(HttpWebResponse response)
        {
            FlowDocument document = new FlowDocument();
            StreamReader SReader = new StreamReader(_appResponse.GetResponseStream(),
                         Encoding.Default);
            Paragraph paragraph = new Paragraph();

            string outputStr = SReader.ReadToEnd();

            htmlToStr(outputStr);

            paragraph.Inlines.Add(new Bold(new Run(outputStr)));
            document.Blocks.Add(paragraph);

            SReader.Close();

            return document;
        }

        private string htmlToStr(string htmlText)
        {
            return "";
        }

        private void output_RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
