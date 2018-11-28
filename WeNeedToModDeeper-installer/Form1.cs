using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Mono.Cecil;
using System.Linq;
using Mono.Cecil.Cil;

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
            if (!File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll"))) { MessageBox.Show("Directory is invalid"); enableButtons(); return; } //Check path is valid
            if (sender == button2 && File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll"))) reinstall = true; //If the update button is pressed go to reinstall mode using backup
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
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void enableButtons()
        {
            //Set all buttons to enabled
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void Uninstall(string path)
        {
            //Check for a backup of the game code
            if (File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll")))
            {
                File.Delete(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll")); //Delete patched game code
                File.Copy(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll"), Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll")); //Copy backup to game code
                File.Delete(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll")); //Delete backup
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
            //Check ILMerge is installed
            if (File.Exists(@"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe"))
            {
                if (File.Exists("Assembly-CSharp.dll")) File.Delete("Assembly-CSharp.dll"); //If a merged file is present for whatever reason delete it
                if (reinstall) { DoMerge(path, "Assembly-CSharp-backup.dll"); } else { DoMerge(path, "Assembly-CSharp.dll"); } //Perform DLL merge, using either the backup or main game code
                Thread.Sleep(5000); //Wait for merge to complete
                if (!File.Exists("Assembly-CSharp.dll")) { MessageBox.Show("Either something went wrong or the engine is already installed, as ILMerge timeout has been exceeded and rewritten file was not found!"); enableButtons(); return; } //If it hasnt then prompt the user
                ReWriteMerge(); //Rewrite the merged file
                if (!File.Exists(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll"))) File.Copy(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll"), Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp-backup.dll")); //Backup the game code if it isnt already
                File.Delete(Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll")); //Delete the unpatched game code
                File.Copy(Path.Combine(Application.StartupPath, "Assembly-CSharp-patched.dll"), Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll")); //Write the patched
                //Cleanup
                File.Delete("Assembly-CSharp.dll");
                File.Delete("Assembly-CSharp-patched.dll");
                //Inform user and reenable buttons
                MessageBox.Show("Installation complete");
                enableButtons();
            }
            else
            {
                //Ask user to install ILMerge
                MessageBox.Show("Please install ILMerge!");
                Process.Start("https://www.microsoft.com/en-us/download/confirmation.aspx?id=17630");
            }
        }

        private void DoMerge(string path, string file)
        {
            //Assemble the command to merge the assemblies
            var quote = "\"";
            var exe = @"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe";
            var args = "/ndebug /copyattrs /targetplatform:4.0," + quote + @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" + quote;
            var outfile = "/out:" + quote + Application.StartupPath + @"\Assembly-CSharp.dll" + quote;
            var asm2 = quote + Path.Combine(Application.StartupPath, "ModEngine.dll") + quote;
            var asm1 = quote + Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\" + file) + quote;
            var command = args + " " + outfile + " " + asm1 + " " + asm2;
            //Run it
            Process.Start(exe, command);
        }

        private void ReWriteMerge() //TODO also write message handler
        {
            var path = Path.Combine(Application.StartupPath, "Assembly-CSharp.dll"); //Get path to asm
            var assembly = AssemblyDefinition.ReadAssembly(path); //Load asm
            //Get types that match criteria
            var toInspect = assembly.MainModule.GetTypes().SelectMany(t => t.Methods.Select(m => new { t, m })).Where(x => x.m.HasBody);
            toInspect = toInspect.Where(x => x.t.Name.EndsWith("MainMenuManagerBehavior") && x.m.Name == "PlayButtonFunction");
            foreach (var method in toInspect) //Get the type
            {
                var processor = method.m.Body.GetILProcessor(); //Get IL processor
                var call = processor.Create(OpCodes.Call, method.m.Module.Import(typeof(WeNeedToModDeeperEngine.ModEngine).GetMethod("Main"))); //Create a call opcode to the engine
                var lastInstruction = method.m.Body.Instructions[method.m.Body.Instructions.Count - 1]; //Get the last command (ret)

                processor.InsertBefore(lastInstruction, call); //Write the call before the last command
                
            }
            assembly.Write("Assembly-CSharp-patched.dll"); //Write the assembly
        }
    }
}
