using System;
using System.IO;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using kov.NET.Protections;
using kov.NET.Utils;

namespace kov.NET
{
    class Program
    {
        public static ModuleDefMD Module { get; set; }
        public ModuleDef ManifestModule;

        public static string FileExtension { get; set; }

        public static bool DontRename { get; set; }

        public static bool ForceWinForms { get; set; }

        public static string FilePath { get; set; }

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Kov.net / Debug Logs";
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
