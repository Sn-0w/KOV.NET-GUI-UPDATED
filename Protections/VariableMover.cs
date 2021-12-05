﻿using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kov.NET.Protections
{
    internal class VariableMover
    {
        public static void Execute()
        {
            var renamedvartype = new TypeDefUser("n", "Type",
                                Program.Module.CorLibTypes.Object.TypeDefOrRef);
            renamedvartype.Attributes = TypeAttributes.Public | TypeAttributes.AutoLayout |
                                TypeAttributes.Class | TypeAttributes.AnsiClass;
            Program.Module.Types.Add(renamedvartype);
            foreach (var type in Program.Module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null) continue;
                    Dictionary<string, Local> strings = new Dictionary<string, Local>();
                    Dictionary<int, Local> ints = new Dictionary<int, Local>();
                    int addedstrings = 0;
                    int addedints = 0;
                    // just trolling
                    var fucked_typesig = Program.Module.ImportAsTypeSig(typeof(int************************************************************************************************************************************************************************************));

                    fucked_typesig = renamedvartype.ToTypeSig();

                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        // strings
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            if (!strings.ContainsKey(instr[i].Operand.ToString()))
                            {
                                var local1 = new Local(Program.Module.CorLibTypes.String);
                                method.Body.Variables.Add(local1);
                                instr.Insert(0, Instruction.Create(OpCodes.Ldstr, instr[i].Operand.ToString()));
                                addedstrings++;
                                instr.Insert(1, Instruction.Create(OpCodes.Stloc_S, local1));
                                i += 2;
                                strings.Add(instr[i].Operand.ToString(), local1);
                            }
                        }
                        // ints
                        if (instr[i].IsLdcI4())
                        {
                            if (!ints.ContainsKey(instr[i].GetLdcI4Value()))
                            {
                                var local1 = new Local(Program.Module.CorLibTypes.Int32);
                                method.Body.Variables.Add(local1);
                                instr.Insert(0, Instruction.Create(OpCodes.Ldc_I4, instr[i].GetLdcI4Value()));
                                addedints++;
                                instr.Insert(1, Instruction.Create(OpCodes.Stloc_S, local1));
                                i += 2;

                                ints.Add(instr[i].GetLdcI4Value(), local1);
                            }
                        }


                    }
                    method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                    int s = 0;
                    int iss = 0;
                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        //strings
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            if (s < addedstrings)
                            {
                                s += 1;
                            }
                            else
                            {
                                instr[i].OpCode = OpCodes.Ldloc_S;
                                instr[i].Operand = strings[instr[i].Operand.ToString()];
                            }
                        }
                        //ints
                        if (instr[i].IsLdcI4())
                        {
                            if (iss < addedints)
                            {
                                iss += 1;
                            }
                            else
                            {
                                int localldc = instr[i].GetLdcI4Value();
                                instr[i].OpCode = OpCodes.Ldloc_S;
                                instr[i].Operand = ints[localldc];
                            }
                        }
                    }

                    foreach (var local in method.Body.Variables)
                    {
                        local.Type = fucked_typesig;
                    }

                }
            }
        }
    }
}
