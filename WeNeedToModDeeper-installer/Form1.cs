using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;

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
            DownloadIPA(path);
            string args = Path.Combine(path, "IPA.exe");
            args = "/c " + "\"" + args + "\" WeNeedToGoDeeper.exe";
            Process.Start("cmd.exe", args);
            File.Copy("ModEngine.exe", Path.Combine(path, "Plugins"));
            MessageBox.Show("Install complete! You will need to start the game, then you should see a new folder called \"mods\" in your we need to go deeper directory");
        }
        private void DownloadIPA(string path)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/Eusth/IPA/releases/download/3.4.1/IPA_3.4.1.zip", "ipa.zip");
            }
            using (ZipArchive archive = ZipFile.OpenRead("ipa.zip"))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    entry.ExtractToFile(path);
                }
            }
            File.Delete("ipa.zip");
        }
    }
}
