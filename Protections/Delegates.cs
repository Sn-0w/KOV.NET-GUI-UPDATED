using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
