using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kov.NET.Protections
{
    internal class HideMethods
    {
        public static void Execute()
        {
            TypeRef attrRef = Program.Module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
            var ctorRef = new MemberRefUser(Program.Module, ".ctor", MethodSig.CreateInstance(Program.Module.CorLibTypes.Void), attrRef);
            var attr = new CustomAttribute(ctorRef);

            TypeRef attrRef2 = Program.Module.CorLibTypes.GetTypeRef("System", "EntryPointNotFoundException");
            var ctorRef2 = new MemberRefUser(Program.Module, ".ctor", MethodSig.CreateInstance(Program.Module.CorLibTypes.Void,Program.Module.CorLibTypes.String), attrRef2);

            foreach (var type in Program.Module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method.IsRuntimeSpecialName || method.IsSpecialName) continue;
                    method.CustomAttributes.Add(attr);
                    method.Name = "<Kov.NET>" + method.Name;
                }
            }

            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            var meth1 = new MethodDefUser("Main",
                        MethodSig.CreateStatic(Program.Module.CorLibTypes.Void, Program.Module.CorLibTypes.String),
                        methImplFlags, methFlags);
            Program.Module.EntryPoint.DeclaringType.Methods.Add(meth1);
            var body = new CilBody();
            meth1.Body = body;
            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "Protected by Kov.NET"));
            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, ctorRef2));
            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Throw));
        }

        void test()
        {
            throw new EntryPointNotFoundException("Protected by Kov.NET");
        }
    }
}
