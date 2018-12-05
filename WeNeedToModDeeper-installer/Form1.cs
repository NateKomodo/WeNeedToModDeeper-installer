using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.IO.Compression;

namespace WeNeedToModDeeper_installer
{
    public partial class Form1 : Form
    {
        //Common paths to game install
        const string path1 = @"E:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path2 = @"C:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path3 = @"D:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path4 = @"E:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path5 = @"C:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path6 = @"D:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        //Check to see if the mod engine is being updated so we dont double-patch
        bool reinstall = false;

        public Form1() //Entry point, init form
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e) //When a button is clicked
        {
            disableButtons(); //Dont want them to click it too many times
            string path = "";
            //Check directory
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
                //If it cant find it, it will ask to browse to it
                MessageBox.Show(@"Could not detect installation folder, you will now be prompted to choose the folder (Should be steamapps\common\WeNeedToGoDeeper)");
                using (var fbd = new FolderBrowserDialog()) //Simple file dialog
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
            if (!File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\UnityEngine.CoreModule.dll"))) { MessageBox.Show("Directory is invalid"); enableButtons(); return; } //Check path is valid
            if (sender == button3) { Uninstall(path); return; } //If uninstall button was pressed, goto uninstall
            if (File.Exists("ModEngine.dll")) //Check ModEngine is present
            {
                DoInstall(path); //Install
            }
            else
            {
                //Inform user and prompt download
                MessageBox.Show("You do not appear to have ModEngine.dll downloaded and placed in the same folder as the installer, in case you did not download it, the download page will now open");
                Process.Start("https://github.com/NateKomodo/WeNeedToModDeeper-Engine/releases/latest");
            }
        }

        private void disableButtons()
        {
            //Set all buttons to disabled
            button1.Enabled = false;
            button3.Enabled = false;
        }

        private void enableButtons()
        {
            //Set all buttons to enabled
            button1.Enabled = true;
            button3.Enabled = true;
        }

        private void Uninstall(string path)
        {
            //Check for a backup of the game code
            if (File.Exists(Path.Combine(path, @"IPA.exe")))
            {
                string quote = "\"";
                Process.Start(Path.Combine(path, "IPA.exe"), quote + Path.Combine(path, "WeNeedToGoDeeper.exe") + quote + " --revert");
                //Inform user and reenable app
                MessageBox.Show("Uninstall complete");
                enableButtons();
            }
            else
            {
                //Backup not found, use steam instead
                MessageBox.Show("Could not find a backup file. Please use Steam's verify local files function to restore your game");
                enableButtons();
            }
        }

        private void DoInstall(string path)
        {
            ContinueInstall(path);
        }

        private void ContinueInstall(string path)
        {
            AddModDll(path); //Add the dll to the game data folder
            GetIPA(path); //Get the modded IPA
            string quote = "\"";
            Process.Start(Path.Combine(path, "IPA.exe"), quote + Path.Combine(path, "WeNeedToGoDeeper.exe") + quote);
            MessageBox.Show("Install complete");
            enableButtons();
        }

        private void GetIPA(string path)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/NateKomodo/Modded-IPA/releases/download/v1/ipa.zip", "ipa.zip");
            }
            if (!File.Exists(Path.Combine(path, "IPA.exe")))
            {
                ZipFile.ExtractToDirectory("ipa.zip", path);
            }
        }
        private void AddModDll(string path)
        {
            //Copies the ModEngine dll to the game data folder
            if (File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"))) File.Delete(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"));
            File.Copy("ModEngine.dll", Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"));
        }
    }
}
