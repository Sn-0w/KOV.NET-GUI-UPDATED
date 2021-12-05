using dnlib.DotNet;
using dnlib.DotNet.Writer;
using kov.NET.Protections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kov.NET
{
    // CREDITS DO NOT REMOVE OR GAY
    // YULLY 1337 + DUCK
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void siticoneButton7_Click(object sender, EventArgs e)
        {
            if(siticoneCustomCheckBox1.Checked)
            {
                Console.WriteLine("Encrypting strings...");
                StringEncryption.Execute();
            }
            if (siticoneCustomCheckBox4.Checked)
            {
                Console.WriteLine("Encoding ints...");
                IntEncoding.Execute();
            }
            if (siticoneCustomCheckBox2.Checked)
            {
                Console.WriteLine("Injecting ControlFlow...");
                ControlFlow.Execute();
            }
            if (siticoneCustomCheckBox5.Checked)
            {
                Console.WriteLine("Injecting local to fields...");
                L2F.Execute();
            }
            if (siticoneCustomCheckBox6.Checked)
            {
                Console.WriteLine("Adding Proxys...");
                ProxyInts.Execute();
                MethodProxy.Execute();
            }
            if (siticoneCustomCheckBox7.Checked)
            {
                Console.WriteLine("Moving Variables...");
                VariableMover.Execute();
            }
            if (siticoneCustomCheckBox8.Checked)
            {
                Console.WriteLine("Converting Strings into Arrays...");
                StringToArray.Execute();
            }
            if (siticoneCustomCheckBox3.Checked)
            {
                Console.WriteLine("Renaming...");
                Renamer.Execute();
            }

            var pathez = $"{Program.FilePath}-kov.exe";
            ModuleWriterOptions opts = new ModuleWriterOptions(Program.Module) { Logger = DummyLogger.NoThrowInstance };
            Program.Module.Write(pathez, opts);
            Console.Write("Obfuscated!");

        }

        private void siticoneButton8_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Program.FilePath = openFileDialog.FileName;
                }
            }
            Program.Module = ModuleDefMD.Load(Program.FilePath);
            Program.FileExtension = Path.GetExtension(Program.FilePath);
            label1.Text = "Current file directory:" + Program.FilePath;

            Console.Write("Selected File "+ Program.FilePath);
        }

        private void siticoneButton9_Click(object sender, EventArgs e)
        {
            Program.FilePath = "N/A";
            label1.Text = "Current file directory:" + Program.FilePath;
        }

        private void siticoneButton3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        private void siticoneButton2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void siticoneButton1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

    }
}
