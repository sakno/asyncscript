using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Collections.Generic;

namespace DynamicScript.Modules.TypeSystem
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents custom type factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class CustomTypeFactory : ScriptContract, IScriptMetaContract
    {
        public static IScriptCustomContract CreateContract(ScriptActionBase.ICombination constructor)
        {
            return new ScriptCustomContract(constructor);
        }

        public static IScriptCustomContract CreateContract(IScriptAction constructor)
        {
            return new ScriptCustomContract(constructor);
        }

        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            var ctor = args.Count == 1 ? args[0] : null;
            if (ctor is IScriptAction)
                return CreateContract((IScriptAction)ctor);
            else if (ctor is ScriptActionBase.ICombination)
                return CreateContract((ScriptActionBase.ICombination)ctor);
            else return Void;
        }

        /// <summary>
        /// Returns custom type factory.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Run(InterpreterState state)
        {
            return new CustomTypeFactory();
        }

        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            throw new NotImplementedException();
        }

        public override IScriptContract GetContractBinding()
        {
            throw new NotImplementedException();
        }
    }
}
