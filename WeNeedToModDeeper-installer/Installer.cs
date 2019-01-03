using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.IO.Compression;
using System.Reflection;

namespace WeNeedToModDeeper_installer
{
    public partial class Installer : Form
    {
        //Common paths to game install
        const string path1 = @"E:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path2 = @"C:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path3 = @"D:\Program Files (x86)\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path4 = @"E:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path5 = @"C:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";
        const string path6 = @"D:\Program Files\Steam\steamapps\common\WeNeedtoGoDeeper";

        const string version = "2.2";

        string path;

        public Installer() //Entry point, init form
        {
            Debug.WriteLine("Installer started, version " + version);
            Debug.WriteLine("Init form");
            InitializeComponent();
            CheckForUpdates();
            Debug.WriteLine("Detecting paths");
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
                Debug.WriteLine("Could not find path, idalog box open");
                MessageBox.Show(@"Could not detect installation folder, you will now be prompted to choose the folder (Should be steamapps\common\WeNeedToGoDeeper)");
                using (var fbd = new FolderBrowserDialog()) //Simple file dialog
                {
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                    fbd.ShowNewFolderButton = false;
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        path = fbd.SelectedPath;
                        Debug.WriteLine("Dialog closed with: " + fbd.SelectedPath);
                    }
                }
            }
        }

        private void CheckForUpdates()
        {
            try
            {
                Debug.WriteLine("Checking for update");
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/NateKomodo/WeNeedToModDeeper-Plugins/master/installer-version.txt");
                StreamReader reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (line.StartsWith("#")) continue;
                    string ver = line.Trim();
                    Debug.WriteLine("Latest is " + ver);
                    if (ver != version)
                    {
                        Debug.WriteLine("Update required, prompting user");
                        MessageBox.Show("Installer is out of date, please update it. The download page will now open");
                        Process.Start("https://github.com/NateKomodo/WeNeedToModDeeper-installer/releases/latest");
                        return;
                    }
                    Debug.WriteLine("Installer is up to date");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message
                    + Environment.NewLine
                    + ex.InnerException);
                Debug.WriteLine("Error: " + ex.Message
                    + Environment.NewLine
                    + ex.InnerException);
            }
        }

        private void button1_Click(object sender, EventArgs e) //When a button is clicked
        {
            disableButtons(); //Dont want them to click it too many times
            Debug.WriteLine("Final path is: " + path);
            if (!File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\UnityEngine.CoreModule.dll"))) { MessageBox.Show("Directory is invalid"); enableButtons(); Debug.WriteLine("Directory invalid"); return; } //Check path is valid
            if (sender == button3) { Uninstall(); return; } //If uninstall button was pressed, goto uninstall
            if (sender == button2) { OpenModManager(); return; }
            if (File.Exists("ModEngine.dll")) //Check ModEngine is present
            {
                DoInstall(); //Install
            }
            else
            {
                //Inform user and prompt download
                MessageBox.Show("You do not appear to have ModEngine.dll downloaded and placed in the same folder as the installer, in case you did not download it, the download page will now open");
                Process.Start("https://github.com/NateKomodo/WeNeedToModDeeper-Engine/releases/latest");
                Debug.WriteLine("ModEngine.dll not present");
            }
        }

        private void disableButtons()
        {
            //Set all buttons to disabled
            button1.Enabled = false;
            button3.Enabled = false;
            Debug.WriteLine("Buttons disabled");
        }

        private void enableButtons()
        {
            //Set all buttons to enabled
            button1.Enabled = true;
            button3.Enabled = true;
            Debug.WriteLine("Buttons enabled");
        }

        private void Uninstall()
        {
            //Check for a backup of the game code
            if (File.Exists(Path.Combine(path, @"IPA.exe")))
            {
                Debug.WriteLine("IPA found, running unistall");
                string quote = "\"";
                Process.Start(Path.Combine(path, "IPA.exe"), quote + Path.Combine(path, "WeNeedToGoDeeper.exe") + quote + " --revert --nowait");
                Debug.WriteLine("Uninstall complete");
                //Inform user and reenable app
                MessageBox.Show("Uninstall complete");
                enableButtons();
            }
            else
            {
                //Backup not found, use steam instead
                Debug.WriteLine("Did not find IPA");
                MessageBox.Show("Could not find a backup file. Please use Steam's verify local files function to restore your game");
                enableButtons();
            }
        }

        private void DoInstall()
        {
            try
            {
                Debug.WriteLine("Installing");
                ContinueInstall();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message
                    + Environment.NewLine
                    + ex.InnerException);
                Debug.WriteLine("Error: " + ex.Message
                    + Environment.NewLine
                    + ex.InnerException);
            }
        }

        private void ContinueInstall()
        {
            AddModDll(); //Add the dll to the game data folder
            GetIPA(); //Get the modded IPA
            string quote = "\"";
            Process.Start(Path.Combine(path, "IPA.exe"), quote + Path.Combine(path, "WeNeedToGoDeeper.exe") + quote + " --nowait");
            Debug.WriteLine("Launched IPA");
            MessageBox.Show("Install complete, to install mods, download them (They should be a dll) and put them in the plugins folder at: " + Path.Combine(path, "Plugins"));
            File.Delete("ipa.zip");
            Debug.WriteLine("ipa.zip removed");
            enableButtons();
            Debug.WriteLine("Done install");
        }

        private void GetIPA()
        {
            if (!File.Exists(Path.Combine(path, "IPA.exe")))
            {
                Debug.WriteLine("IPA not found, installing");
                using (var client = new WebClient())
                {
                    Debug.WriteLine("Downloading IPA");
                    client.DownloadFile("https://github.com/NateKomodo/Modded-IPA/releases/download/v1/ipa.zip", "ipa.zip");
                    Debug.WriteLine("Downloaded");
                }
                ZipFile.ExtractToDirectory("ipa.zip", path);
                Debug.WriteLine("IPA extracted");
            }
            else
            {
                Debug.WriteLine("IPA is already present");
            }
        }
        private void AddModDll()
        {
            //Copies the ModEngine dll to the game data folder
            Debug.WriteLine("Adding the mod engine DLL to managed folder");
            if (File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"))) File.Delete(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"));
            File.Copy("ModEngine.dll", Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\ModEngine.dll"));
            Debug.WriteLine("Done adding mod engine to managed folder");
        }
        private void OpenModManager()
        {
            Debug.WriteLine("Opening mod manager");
            enableButtons();
            ModManager manager = new ModManager(path);
            manager.Show();
        }
    }
}
