using dnlib.DotNet;
using dnlib.DotNet.Emit;
using kov.NET.Protections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kov.NET
{
    class ProxyInts
    {
        public static Random rand = new Random();
        private static int Amount { get; set; }
        public static void Execute()
        {
            var ManifestModule = Program.Module;
            foreach (TypeDef type in ManifestModule.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (method.Body.Instructions[i].IsLdcI4())
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(L2F.RandomString(+30),
                                        MethodSig.CreateStatic(ManifestModule.CorLibTypes.Int32),
                                        methImplFlags, methFlags);
                            ManifestModule.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(ManifestModule.CorLibTypes.Int32));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instr[i].GetLdcI4Value()));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = meth1;
                            Amount++;
                        }
                        else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R4)
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(L2F.RandomString(+30),
                                        MethodSig.CreateStatic(ManifestModule.CorLibTypes.Double),
                                        methImplFlags, methFlags);
                            ManifestModule.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(ManifestModule.CorLibTypes.Double));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (float)method.Body.Instructions[i].Operand));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = meth1;
                            Amount++;
                        }
                    }
                }
            }

            var stringref = new TypeRefUser(ManifestModule, "System", "String", ManifestModule.CorLibTypes.AssemblyRef);
            var stringlength = new MemberRefUser(ManifestModule, "get_Length", MethodSig.CreateInstance(ManifestModule.CorLibTypes.Int32), stringref);
            var mathref = new TypeRefUser(ManifestModule, "System", "Math", ManifestModule.CorLibTypes.AssemblyRef);
            var mathmin = new MemberRefUser(ManifestModule, "Min", MethodSig.CreateStatic(ManifestModule.CorLibTypes.Int32, ManifestModule.CorLibTypes.Int32, ManifestModule.CorLibTypes.Int32), mathref);
            var systemconvert = new TypeRefUser(ManifestModule, "System", "Convert", ManifestModule.CorLibTypes.AssemblyRef);
            var toint32 = new MemberRefUser(ManifestModule, "ToInt32", MethodSig.CreateStatic(ManifestModule.CorLibTypes.Int32, ManifestModule.CorLibTypes.String), systemconvert);

            foreach (var type in ManifestModule.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null) continue;

                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        if (instr[i].IsLdcI4() && instr[i].GetLdcI4Value() >= 0 && instr[i].GetLdcI4Value() <= 1000)
                        {
                            int amount = instr[i].GetLdcI4Value();
                            instr[i].OpCode = OpCodes.Ldstr;
                            instr[i].Operand = L2F.RandomString(amount);
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Call, stringlength));
                            instr.Insert(i + 2, Instruction.Create(OpCodes.Ldstr, L2F.RandomString(amount + 1)));
                            instr.Insert(i + 3, Instruction.Create(OpCodes.Call, stringlength));
                            instr.Insert(i + 4, Instruction.Create(OpCodes.Call, mathmin));
                            i += 4;
                        }else if (instr[i].IsLdcI4())
                        {
                            int amount = instr[i].GetLdcI4Value();
                            instr[i].OpCode = OpCodes.Ldstr;
                            instr[i].Operand = amount.ToString();
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Call, toint32));
                        }
                    }
                }

            }


            Console.WriteLine("   " + Amount + " ints proxied!");
        }
    }
}
