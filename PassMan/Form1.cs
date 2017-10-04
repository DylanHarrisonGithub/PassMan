using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Numerics;
using System.IO;

namespace PassMan
{
    public partial class Form1 : Form
    {

        BigInteger maintext;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text.Length < 1) || (textBox2.Text.Length > 129)) {
                System.Windows.Forms.MessageBox.Show("Text must consist of 1 to 129 characters");
            } else if (textBox1.Text.Length < 1)
            {
                System.Windows.Forms.MessageBox.Show("Password cannot be empty string");
            } else
            {
                string password = textBox1.Text;
                string text = textBox2.Text;
                BigInteger plaintext = new BigInteger(Encoding.ASCII.GetBytes(text));

                Cursor.Current = Cursors.WaitCursor;
                maintext = BigIntegerExtensions.Encrypt(plaintext, password, 128);
                string encryptedText = new string(Encoding.ASCII.GetChars(maintext.ToByteArray()));
                if (encryptedText.Length > 129)
                {
                    System.Windows.Forms.MessageBox.Show("Failed to encrypt message with that password. Try changing your password or message.");
                    maintext = new BigInteger(Encoding.ASCII.GetBytes(textBox2.Text));
                } else
                {
                    textBox2.Text = encryptedText;
                }
                
                Cursor.Current = Cursors.Default;
            }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text.Length < 1) || (textBox2.Text.Length > 129))
            {
                System.Windows.Forms.MessageBox.Show("Text must consist of 1 to 128 characters");
            }
            else if (textBox1.Text.Length < 1)
            {
                System.Windows.Forms.MessageBox.Show("Password cannot be empty string");
            } else
            {
                string password = textBox1.Text;

                Cursor.Current = Cursors.WaitCursor;
                maintext = BigIntegerExtensions.Decrypt(maintext, password, 128);
                textBox2.Text = new string(Encoding.ASCII.GetChars(maintext.ToByteArray()));
                Cursor.Current = Cursors.Default;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            label2.Text = (textBox2.Text.Length).ToString();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to start a new file?", "Start New", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                textBox1.Text = "";
                textBox2.Text = "";
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] text = File.ReadAllBytes(openDialog.FileName);
                textBox2.Text = Encoding.ASCII.GetString(text);
                maintext = new BigInteger(text);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            //saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, maintext.ToByteArray());
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form HelpForm = new Form();
            HelpForm.Width = 300;
            HelpForm.Height = 240;
            HelpForm.Text = "Help";
            HelpForm.Controls.Add(new Label()
            {
                Left = 20,
                Top = 20,
                AutoSize = true,
                MaximumSize = new Size(260, 0),
                Text = "Enter your password in the small text field and up to 128 characters in the large text field. Click Encrypt or Decrypt to carry out those operations on the text in the large text field. For more information about this application, please visit my blog at"
            });
            LinkLabel MyLink = new LinkLabel();
            MyLink.Text = "www.dylansamcsblog.wordpress.com";
            MyLink.LinkClicked += new LinkLabelLinkClickedEventHandler(
                (object p, LinkLabelLinkClickedEventArgs q) =>
                {
                    ((LinkLabel) p).LinkVisited = true;
                    System.Diagnostics.Process.Start("http://www.dylansamcsblog.wordpress.com");
                });
            MyLink.Top = 110;
            MyLink.Left = 50;
            MyLink.AutoSize = true;

            Button OkButton = new Button();
            OkButton.Click += new EventHandler(
                (object p, System.EventArgs q) =>
                {
                    Form parent = ((Button)p).FindForm();
                    parent.Close();
                });
            OkButton.Top = 150;
            OkButton.Left = 100;
            OkButton.Text = "Ok";

            HelpForm.Controls.Add(MyLink);
            HelpForm.Controls.Add(OkButton);

            HelpForm.ShowDialog();
        }

    }
}
