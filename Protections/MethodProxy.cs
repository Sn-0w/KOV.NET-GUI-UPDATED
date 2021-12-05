using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kov.NET.Protections
{
    internal class MethodProxy
    {
        public static void Execute()
        {
            var proxytype = new TypeDefUser("nn", "tt",
                               Program.Module.CorLibTypes.Object.TypeDefOrRef);
            proxytype.Attributes = TypeAttributes.Public | TypeAttributes.AutoLayout |
                                TypeAttributes.Class | TypeAttributes.AnsiClass;
            Program.Module.Types.Add(proxytype);
            foreach (var type in Program.Module.GetTypes())
            {
                for (int mm = 0; mm < type.Methods.Count(); mm++)
                {
                    var method = type.Methods[mm];
                    if (!method.HasBody || method.Body == null || method.Name.StartsWith("ProxyMethod")) continue;

                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            IMethod operandmeth = instr[i].Operand as IMethod;
                            if (operandmeth != null && !operandmeth.MethodSig.HasThis)
                            {
                                var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                                var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                                var proxymethod = new MethodDefUser("ProxyMethod_" + operandmeth.Name,
                                            MethodSig.CreateStatic(operandmeth.MethodSig.RetType, operandmeth.GetParams().ToArray()),
                                            methImplFlags, methFlags);
                                proxymethod.Body = new CilBody();
                                if (operandmeth.ResolveMethodDef() != null)
                                {
                                    operandmeth = operandmeth.ResolveMethodDef();
                                }

                                foreach (var arg in proxymethod.Parameters)
                                {
                                    arg.CreateParamDef();
                                    proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_S, arg));
                                }
                                if (operandmeth is MethodDef)
                                {
                                    proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, operandmeth));
                                }
                                else if (operandmeth is MemberRef)
                                {
                                    proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, (MemberRef)operandmeth));
                                }
                                else if (operandmeth is MethodSpec)
                                {
                                    proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, (MethodSpec)operandmeth));
                                }
                                proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                                proxytype.Methods.Add(proxymethod);

                                instr[i].Operand = proxymethod;
                            }
                        }

                        if (instr[i].OpCode == OpCodes.Newobj)
                        {
                            IMethodDefOrRef methodDefOrRef = instr[i].Operand as IMethodDefOrRef;

                            if (methodDefOrRef.IsMethodSpec) continue;
                            if (methodDefOrRef == null) continue;
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem;
                            var proxymethod = new MethodDefUser("ProxyMethod_" + methodDefOrRef.Name,
                                        MethodSig.CreateStatic(Program.Module.Import(methodDefOrRef.DeclaringType.ToTypeSig()), methodDefOrRef.GetParams().ToArray()),
                                        methImplFlags, methFlags);
                            proxymethod.Body = new CilBody();
                            foreach (var arg in proxymethod.Parameters)
                            {
                                arg.CreateParamDef();
                                proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_S, arg));
                            }
                            proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, methodDefOrRef));
                            proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            if (proxymethod == null) continue;
                            method.DeclaringType.Methods.Add(proxymethod);
                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = proxymethod;

                        }
                    }
                }
            }
        }
    }
}
