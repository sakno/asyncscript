using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IExpressionFactorySlots
    {
        IRuntimeSlot Parse { get; }

        IRuntimeSlot Reduce { get; }

        IRuntimeSlot Compile { get; }

        IRuntimeSlot Constant { get; }

        IRuntimeSlot NmToken { get; }

        IRuntimeSlot BinOp { get; }

        IRuntimeSlot UnOp { get; }

        IRuntimeSlot AsyncDef { get; }

        IRuntimeSlot Ca { get; }    //reference to the current action

        IRuntimeSlot Equ { get; }   //value equality action

        IRuntimeSlot REqu { get; }  //reference equality action

        IRuntimeSlot Arcon { get; } //array contract

        IRuntimeSlot ThisRef { get; }   //this expression

        IRuntimeSlot Array { get; } //array expression

        IRuntimeSlot ForkDef { get; }   //fork expression

        IRuntimeSlot Cond { get; }  //conditional

        IRuntimeSlot Inv { get; }   //invocation expression.

        IRuntimeSlot Indexer { get; }   //indexer expression

        IRuntimeSlot Init { get; }

        IRuntimeSlot ForEach { get; }   //for-each loop expression

        IRuntimeSlot ForLoop { get; }   //for loop expression

        IRuntimeSlot WhileLoop { get; } //while loop expression

        IRuntimeSlot Obj { get; }   //complex object expression

        IRuntimeSlot Signature { get; } //action contract expression.

        IRuntimeSlot Action { get; }    //action implementation

        IRuntimeSlot Seh { get; }       //structured exception handler

        IRuntimeSlot Selection { get; } //selection

        IRuntimeSlot Clone { get; }

        IRuntimeSlot Visit { get; }

        IRuntimeSlot Placeholder { get; }   //placeholder expression

        IRuntimeSlot Deduce { get; }    //replace all placeholders in the expression

        IRuntimeSlot Cplx { get; }  //complex expression

        IRuntimeSlot Expand { get; }
    }
}
