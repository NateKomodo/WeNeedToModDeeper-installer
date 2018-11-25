using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Threading;
using dnlib.DotNet;

namespace WeNeedToModDeeper_installer
{
    public partial class Form1 : Form
    {
        const string path1 = @"E:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path2 = @"C:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path3 = @"D:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path4 = @"E:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path5 = @"C:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path6 = @"D:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";

        public Form1()
        {
            InitializeComponent();

        }

        private void locationDialog_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = "";
            if (Directory.Exists(path1))
            {
                path = path1;
            }
            else if (Directory.Exists(path2))
            {
                path = path2;
            }
            else if (Directory.Exists(path3))
            {
                path = path3;
            }
            else if (Directory.Exists(path4))
            {
                path = path4;
            }
            else if (Directory.Exists(path5))
            {
                path = path5;
            }
            else if (Directory.Exists(path6))
            {
                path = path6;
            }
            else
            {
                MessageBox.Show(@"Could not detect installation folder, you will now be prompted to choose the folder (Should be steamapps\common\WeNeedToGoDeeper)");
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                    fbd.ShowNewFolderButton = false;
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        path = fbd.SelectedPath;
                    }
                }
            }
            if (File.Exists("ModEngine.dll"))
            {
                DoInstall(path);
            }
            else
            {
                MessageBox.Show("You do not appear to have ModEngine.dll downloaded and placed in the same folder as the installer, in case you did not download it, the download page will now open");
                Process.Start("https://github.com/NateKomodo/WeNeedToModDeeper-Engine/releases/latest");
            }
        }

        private void DoInstall(string path)
        {
            if (File.Exists(@"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe"))
            {
                var quote = "\"";
                var exe = @"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe";
                var args = "/ndebug /copyattrs /targetplatform:4.0," + quote + @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" + quote;
                var outfile = "/out:" + quote + @"C:\Users\natko\Desktop\merge.dll" + quote;
                var asm2 = quote + Path.Combine(Application.StartupPath, "ModEngine.dll") + quote;
                var asm1 = quote + Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll") + quote;
                var command = args + " " + outfile + " " + asm1 + " " + asm2;
                Process.Start(exe, command);
            }
            else
            {
                MessageBox.Show("Please install ILMerge!");
                Process.Start("https://www.microsoft.com/en-us/download/confirmation.aspx?id=17630");
            }
        }
    }
}
