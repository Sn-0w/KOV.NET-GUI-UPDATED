using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using kov.NET.Utils;

namespace kov.NET.Protections
{
    public class StringEncryption : Randomizer
    {
        private static int Amount { get; set; }

        public static void Execute()
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(StringDecoder).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(StringDecoder).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, Program.Module.GlobalType,
                Program.Module);
            MethodDef init = (MethodDef)members.Single(method => method.Name == "Decrypt");
            init.Rename(GenerateRandomString(MemberRenamer.StringLength()));

            foreach (MethodDef method in Program.Module.GlobalType.Methods)
                if (method.Name.Equals(".ctor"))
                {
                    Program.Module.GlobalType.Remove(method);
                    break;
                }

            foreach (TypeDef type in Program.Module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    var cryptoRandom = new CryptoRandom();
                    if (!method.HasBody) continue;
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            var key = method.Name.Length + Next();

                            var encryptedString =
                                Encrypt(new Tuple<string, int>(method.Body.Instructions[i].Operand.ToString(), key));

                            method.Body.Instructions[i].OpCode = OpCodes.Ldstr;
                            method.Body.Instructions[i].Operand = encryptedString;
                            method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(key));
                            method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(init));
                            Amount++;
                            i += 2;
                        }
                }
            }



            var getstringmethod = typeModule.EntryPoint;
            foreach (var type_ in typeModule.GetTypes())
            {
                foreach (var method__ in type_.Methods)
                {
                    if (method__.Name != "ExtractResource") continue;
                    getstringmethod = method__;
                }
            }
            getstringmethod.DeclaringType.Remove(getstringmethod);
            Program.Module.GlobalType.Methods.Add(getstringmethod);


            foreach (TypeDef type in Program.Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null) continue;

                    IList<Instruction> instr = method.Body.Instructions;

                    for (int i = 0; i < instr.Count; i++)
                    {
                        try
                        {
                            if (method.Body.Instructions[i].OpCode != OpCodes.Ldstr) continue;





                            var resourceName = GenerateRandomString(MemberRenamer.StringLength());
                            byte[] stringasbytes = Encoding.UTF8.GetBytes(method.Body.Instructions[i].Operand.ToString());


                            Program.Module.Resources.Add(new EmbeddedResource(resourceName, stringasbytes));
                            method.Body.Instructions[i].Operand = resourceName;
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, getstringmethod));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }



            Console.WriteLine($"  Encrypted {Amount} strings.");
        }

        public static string ExtractResource(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetCallingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return Encoding.UTF8.GetString(ba);
            }
        }

        public static int Next()
        {
            return BitConverter.ToInt32(RandomBytes(sizeof(int)), 0);
        }
        private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();
        private static byte[] RandomBytes(int bytes)
        {
            byte[] buffer = new byte[bytes];
            csp.GetBytes(buffer);
            return buffer;
        }
        public static string Encrypt(Tuple<string, int> values)
        {
            StringBuilder input = new StringBuilder(values.Item1);
            StringBuilder output = new StringBuilder(values.Item1.Length);
            char Textch;
            int key = values.Item2;
            for (int iCount = 0; iCount < values.Item1.Length; iCount++)
            {
                Textch = input[iCount];
                Textch = (char)(Textch ^ key);
                output.Append(Textch);
            }
            return output.ToString();
        }
    }
}
