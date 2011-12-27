using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Collections.Generic;

namespace DynamicScript.Modules.TypeSystem
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    /*
     * Type definition
     * const ctor = @i: integer -> void: {
     *      this.initialize("conv")(2);    //call base constructor
     *      this.self.a = 10;
     *      this.conv.b = 10;
     * }
     * var ct = custom_type(${{a: integer}}).aggregates("conv", ${{b: integer}}).constructor(ctor);
     * ct.prototype - returns ${{a: integer}}
     * ct.constructor - returns reference to ctor
     */
    /// <summary>
    /// Represents custom type factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class CustomTypeFactory : ScriptContract, IScriptMetaContract
    {
        public static IScriptCustomContract CreateContract(ScriptFunctionBase.ICombination constructor)
        {
            return new ScriptCustomContract(constructor);
        }

        public static IScriptCustomContract CreateContract(IScriptFunction constructor)
        {
            return new ScriptCustomContract(constructor);
        }

        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            var ctor = args.Count == 1 ? args[0] : null;
            if (ctor is IScriptFunction)
                return CreateContract((IScriptFunction)ctor);
            else if (ctor is ScriptFunctionBase.ICombination)
                return CreateContract((ScriptFunctionBase.ICombination)ctor);
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
