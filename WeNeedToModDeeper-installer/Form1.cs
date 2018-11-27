using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Mono.Cecil;
using System.Linq;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Text;

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
        MethodDefinition refrenceType;

        public Form1()
        {
            InitializeComponent();

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
            button1.Enabled = false;
            if (File.Exists(@"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe"))
            {
                DoMerge(path);
                Thread.Sleep(5000); //TODO cange to wait until file exists via while loop
                UpdateRef("Assembly-CSharp.dll");
                ReWriteMerge();
                //TODO overwrite
                File.Copy(Path.Combine(Application.StartupPath, "Assembly-CSharp.dll"), Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll"));
                //TODO delete DLLs/cleanup
                MessageBox.Show("Installation complete");
            }
            else
            {
                MessageBox.Show("Please install ILMerge!");
                Process.Start("https://www.microsoft.com/en-us/download/confirmation.aspx?id=17630");
            }
        }

        private void DoMerge(string path)
        {
            var quote = "\"";
            var exe = @"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe";
            var args = "/ndebug /copyattrs /targetplatform:4.0," + quote + @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" + quote;
            var outfile = "/out:" + quote + Application.StartupPath + @"\Assembly-CSharp.dll" + quote;
            var asm2 = quote + Path.Combine(Application.StartupPath, "ModEngine.dll") + quote;
            var asm1 = quote + Path.Combine(path, @"WeNeedToGoDeeper_Data\Managed\Assembly-CSharp.dll") + quote;
            var command = args + " " + outfile + " " + asm1 + " " + asm2;
            Process.Start(exe, command);
        }

        private void ReWriteMerge()
        {
            var path = Path.Combine(Application.StartupPath, "Assembly-CSharp.dll");
            var assembly = AssemblyDefinition.ReadAssembly(path);
            var toInspect = assembly.MainModule.GetTypes().SelectMany(t => t.Methods.Select(m => new { t, m })).Where(x => x.m.HasBody);
            toInspect = toInspect.Where(x => x.t.Name.EndsWith("MainMenuManagerBehavior") && x.m.Name == "PlayButtonFunction");
            foreach (var method in toInspect)
            {
                var processor = method.m.Body.GetILProcessor();
                var call = processor.Create(OpCodes.Call, method.m.Module.Import(typeof(WeNeedToModDeeperEngine.ModEngine).GetMethod("Main")));
                var lastInstruction = method.m.Body.Instructions[method.m.Body.Instructions.Count - 1];

                processor.InsertBefore(lastInstruction, call);
                
            }
            assembly.Write("Assembly-CSharp-patched.dll");
        }
        private void UpdateRef(string file)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(Application.StartupPath, file));
                var moduleDefinition = assembly.MainModule;
                var objectIdDefinition = moduleDefinition.GetType("WeNeedToModDeeperEngine.ModEngine");
                foreach (var method in objectIdDefinition.Methods)
                {
                    if (method.Name == "Main")
                    {
                        refrenceType = method;
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                MessageBox.Show(sb.ToString());
            }
        }
    }
}
