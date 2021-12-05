using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kov.NET.CFHelper;

namespace kov.NET.Protections
{
    internal class Delegates
    {
        public static void Execute()
        {
            var cctor = Program.Module.GlobalType.FindOrCreateStaticConstructor();
            cctor.Body = new CilBody();
            for (int typec = 0; typec < Program.Module.Types.Count(); typec++)
            {
                var type = Program.Module.Types[typec];
                if (type == Program.Module.GlobalType) continue;
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null || method.IsSpecialName) continue;

                    int max = 0;


                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            IMethod operandmeth = instr[i].Operand as IMethod;
                            if (operandmeth == null) continue;

                            TypeDef delegate_ = MakeDelegate(Program.Module, operandmeth.MethodSig);
                            var delegatefield = new FieldDefUser(L2F.RandomString(5),
                            new FieldSig(delegate_.ToTypeSig()),
                            FieldAttributes.Public | FieldAttributes.Static);
                            Program.Module.GlobalType.Fields.Add(delegatefield);
                            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, operandmeth));
                            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, delegate_.FindInstanceConstructors().First()));
                            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, delegatefield));


                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = makeproxy(delegate_, delegatefield);

                        }
                    }
                }
            }
            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));


            //obf cctor
            for (int i = 0; i < cctor.Body.Instructions.Count(); i++)
            {
                var instr = cctor.Body.Instructions;
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

                        Program.Module.GlobalType.Methods.Add(proxymethod);

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
                    cctor.DeclaringType.Methods.Add(proxymethod);
                    instr[i].OpCode = OpCodes.Call;
                    instr[i].Operand = proxymethod;

                }
            }

            CFHelper cfhelper = new CFHelper();

            if (ControlFlow.Simplify(cctor))
            {
                Blocks blocks = cfhelper.GetBlocks(cctor);
                if (blocks.blocks.Count != 1)
                {
                    ControlFlow.toDoBody(cfhelper, cctor, blocks, cctor.DeclaringType);
                }
            }
            ControlFlow.Optimize(cctor);

        }

        static MethodDef makeproxy(TypeDef delegate_, FieldDef delegatefield)
        {

            var invokemeth = delegate_.FindMethods("Invoke").First();

            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            var proxymethod = new MethodDefUser("ProxyMethod_" + delegate_.Name,
                        MethodSig.CreateStatic(invokemeth.MethodSig.RetType, invokemeth.GetParams().ToArray()),
                        methImplFlags, methFlags);
            proxymethod.Body = new CilBody();
            proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, delegatefield));
            foreach (var arg in proxymethod.Parameters)
            {
                arg.CreateParamDef();
                proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_S, arg));
            }
            if (invokemeth is MethodDef)
            {
                proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, delegate_.FindMethods("Invoke").First()));
            }

            proxymethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            Program.Module.GlobalType.Methods.Add(proxymethod);

            return proxymethod;
        }

        protected static TypeDef MakeDelegate(ModuleDefMD md, MethodSig sig)
        {
            TypeDef ret = new TypeDefUser("delegates", L2F.RandomString(5), md.CorLibTypes.GetTypeRef("System", "MulticastDelegate"));
            ret.Attributes = TypeAttributes.NotPublic | TypeAttributes.Sealed;

            var ctor = new MethodDefUser(".ctor", MethodSig.CreateInstance(md.CorLibTypes.Void, md.CorLibTypes.Object, md.CorLibTypes.IntPtr));
            ctor.Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
            ctor.ImplAttributes = MethodImplAttributes.Runtime;
            ret.Methods.Add(ctor);

            var invoke = new MethodDefUser("Invoke", sig.Clone());
            invoke.MethodSig.HasThis = true;
            invoke.IsSpecialName = true;
            invoke.IsRuntimeSpecialName = true;
            invoke.Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            invoke.ImplAttributes = MethodImplAttributes.Runtime;
            ret.Methods.Add(invoke);

            md.Types.Add(ret);

            return ret;
        }


    }
}
