using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailMessageSender
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SendMessageButton_Click(object sender, EventArgs e)
        {
            MailAddress FromAddress = new MailAddress(FromTextBox.Text);
            MailAddress ToAddress = new MailAddress(ForTextBox.Text);
            MailMessage Message = new MailMessage(FromAddress, ToAddress);
            Message.Subject = SubjectTextBox.Text;
            Message.Body = MessageRichTextBox.Text;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(FromAddress.Address, InputMailPassword.Text);
            smtpClient.Send(Message);
        }
    }
}
