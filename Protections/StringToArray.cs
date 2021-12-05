using dnlib.DotNet;
using dnlib.DotNet.Emit;
using kov.NET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kov.NET.Protections
{
    internal class StringToArray
    {
        static Random rng = new Random();
        public static void Execute()
        {
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
                            if (!(method.Body.Instructions[i].OpCode == OpCodes.Ldstr)) continue;

                            //locals
                            List<Instruction> instrreal = new List<Instruction>();
                            int count = 0;

                            //add variable
                            Random dg = new Random();
                            var local = new Local(method.Module.Import(typeof(string[])).ToTypeSig(), Randomizer.GenerateRandomString(MemberRenamer.StringLength()));
                            method.Body.Variables.Add(local);

                            //add array constructor
                            instrreal.Add(new Instruction(OpCodes.Ldc_I4, 256));
                            instrreal.Add(new Instruction(OpCodes.Newarr, method.Module.Import(typeof(string))));
                            instrreal.Add(new Instruction(OpCodes.Stloc_S, local));

                            //add array items
                            foreach (char boi in instr[i].Operand.ToString())
                            {
                                instrreal.Add(new Instruction(OpCodes.Ldloc, local));
                                instrreal.Add(new Instruction(OpCodes.Ldc_I4, count));
                                switch (rng.Next(0,2))
                                {
                                    case 0:
                                        {
                                            instrreal.Add(new Instruction(OpCodes.Ldc_I8, (long)boi));
                                            instrreal.Add(Instruction.Create(OpCodes.Call, Program.Module.Import(typeof(System.Convert).GetMethod("ToChar", new Type[] { typeof(long) }))));
                                            break;
                                        }
                                    case 1:
                                        {
                                            instrreal.Add(new Instruction(OpCodes.Ldc_I4, (int)boi));
                                            instrreal.Add(Instruction.Create(OpCodes.Call, Program.Module.Import(typeof(System.Convert).GetMethod("ToChar", new Type[] { typeof(int) }))));
                                            break;
                                        }
                                   

                                }

                                instrreal.Add(Instruction.Create(OpCodes.Call, Program.Module.Import(typeof(System.Convert).GetMethod("ToString", new Type[] { typeof(char) }))));
                                instrreal.Add(new Instruction(OpCodes.Stelem_Ref));
                                count++;
                            }

                            //actually add stuff
                            int num4 = 0;
                            foreach (Instruction item in instrreal)
                            {
                                method.Body.Instructions.Insert(num4, item);
                                num4++;
                            }

                            //replace original reference
                            instr.Insert(i + num4, new Instruction(OpCodes.Ldloc, local));
                            instr[i + num4 + 1].OpCode = OpCodes.Call;
                            instr[i + num4 + 1].Operand = Program.Module.Import(typeof(System.String).GetMethod("Concat", new Type[] { typeof(string[]) }));


                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
        }
    }
}
