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
        private async void siticoneButton7_Click(object sender, EventArgs e)
        {
            if(siticoneCustomCheckBox1.Checked)
            {
                Console.WriteLine("Encrypting strings...");
                StringEncryption.Execute();
            }
            if (siticoneCustomCheckBox3.Checked)
            {
                Console.WriteLine("Renaming...");
                Renamer.Execute();
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
            }
            if (siticoneCustomCheckBox7.Checked)
            {
                Console.WriteLine("Injecting AntiDe4Dot...");
                AntiDe4Dot.Execute();

            }

            var pathez = $"{Program.FilePath}-kov.exe";
            ModuleWriterOptions opts = new ModuleWriterOptions(Program.Module) { Logger = DummyLogger.NoThrowInstance };
            Program.Module.Write(pathez, opts);
            Console.Clear();
            Console.Write("Obfuscated!");
            await Task.Delay(2000);
            Console.Clear();

        }

        private async void siticoneButton8_Click(object sender, EventArgs e)
        {
            Console.Write("Drag And Drop File Into Console: ");
            Program.FilePath = Console.ReadLine();
            Program.Module = ModuleDefMD.Load(Program.FilePath);
            Program.FileExtension = Path.GetExtension(Program.FilePath);
            label1.Text = "Current file directory:" + Program.FilePath;

            Console.Clear();
            Console.Write("Added!");
            await Task.Delay(2000);
            Console.Clear();
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
