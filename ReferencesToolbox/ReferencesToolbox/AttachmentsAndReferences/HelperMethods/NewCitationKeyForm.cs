using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace ReferencesToolbox
{
    class ReferenceForms
    {
        public static string NewCitationKeyForm(string title, out string ret)
        {
            Form dc = new Form();
            dc.Text = title;

            dc.HelpButton = dc.MinimizeBox = dc.MaximizeBox = false;
            dc.ShowIcon = dc.ShowInTaskbar = false;
            dc.TopMost = true;

            dc.Height = 100;
            dc.Width = 300;
            dc.MinimumSize = new Size(dc.Width, dc.Height);

            int margin = 5;
            Size size = dc.ClientSize;

            TextBox tb = new TextBox();
            tb.TextAlign = HorizontalAlignment.Right;
            tb.Height = 20;
            tb.Width = size.Width - 2 * margin;
            tb.Location = new Point(margin, margin);
            tb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dc.Controls.Add(tb);

            Button ok = new Button();
            ok.Text = "Ok";
            ok.Click += new EventHandler(OK_Click);
            ok.Height = 23;
            ok.Width = 75;
            ok.Location = new Point(size.Width / 2 - ok.Width / 2, size.Height / 2);
            ok.Anchor = AnchorStyles.Bottom;
            dc.Controls.Add(ok);
            dc.AcceptButton = ok;

            dc.ShowDialog();

            ret = tb.Text;

            return ret;
        }
        private static void OK_Click(object sender, EventArgs e)
        {
            Form form = (sender as Control).Parent as Form;
            form.DialogResult = DialogResult.OK;
            form.Close();
        }
    }
}
