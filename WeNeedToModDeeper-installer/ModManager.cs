using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WeNeedToModDeeper_installer
{
    public class ModManager : Form
    {
        Dictionary<string, string> namedesc = new Dictionary<string, string>();
        Dictionary<string, string> namedownload = new Dictionary<string, string>();
        Dictionary<string, string> nameprefix = new Dictionary<string, string>();
        Dictionary<string, string> nameauthor = new Dictionary<string, string>();
        private Button button3;
        string pluginspath;

        public ModManager(string path)
        {
            try
            {
                pluginspath = Path.Combine(path, "Plugins");
                InitializeComponent();
                GetList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("MM Error: " + ex.Message
                    + Environment.NewLine
                    + ex.StackTrace
                    + Environment.NewLine
                    + ex.InnerException);
            }
        }
        private ListBox listBox1;
        private Button button1;
        private Button button2;
        private Label label1;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModManager));
            this.label1 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mod Manager";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 51);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(227, 147);
            this.listBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 204);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(227, 36);
            this.button1.TabIndex = 2;
            this.button1.Text = "Install selected";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(12, 246);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(227, 36);
            this.button2.TabIndex = 3;
            this.button2.Text = "View selected";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(12, 288);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(227, 36);
            this.button3.TabIndex = 4;
            this.button3.Text = "Uninstall selected";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ModManager
            // 
            this.ClientSize = new System.Drawing.Size(251, 332);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModManager";
            this.Text = "Mod manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void button1_Click(object sender, System.EventArgs e) //Install
        {
            if (!Directory.Exists(pluginspath))
            {
                MessageBox.Show("Please install the mod engine first");
                return;
            }
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item");
                return;
            }
            string selected = listBox1.SelectedItem.ToString();
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(namedownload[selected], Path.Combine(pluginspath, selected + ".dll"));
                }
                MessageBox.Show("Plugin " + selected + " installed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button2_Click(object sender, System.EventArgs e) //View details
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item");
                return;
            }
            string selected = listBox1.SelectedItem.ToString();
            MessageBox.Show(selected + ": "
                + Environment.NewLine
                + "Description: " + namedesc[selected]
                + Environment.NewLine
                + "Author: " + nameauthor[selected]
                + Environment.NewLine
                + "Command Prefix: " + nameprefix[selected]);
        }

        private void GetList()
        {
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/NateKomodo/WeNeedToModDeeper-Plugins/master/index.txt");
                StreamReader reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (line == " " || line == "" || line == null) continue;
                    string[] entry = line.Split('&');
                    if (entry[0].StartsWith("#")) continue;
                    listBox1.Items.Add(entry[0]);
                    namedesc.Add(entry[0], entry[1]);
                    namedownload.Add(entry[0], entry[2]);
                    nameprefix.Add(entry[0], entry[3]);
                    nameauthor.Add(entry[0], entry[4]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message
                    + Environment.NewLine
                    + ex.StackTrace
                    + Environment.NewLine
                    + ex.Data);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item");
                return;
            }
            string selected = listBox1.SelectedItem.ToString();
            string pathtoplugin = Path.Combine(pluginspath, selected + ".dll");
            if (File.Exists(pathtoplugin))
            {
                File.Delete(pathtoplugin);
                MessageBox.Show("Plugin uninstalled");
            }
            else
            {
                MessageBox.Show("Plugin is not installed");
            }
        }
    }
}