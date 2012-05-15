using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a delegate that implements a switch case in the parallel scenario.
    /// </summary>
    /// <typeparam name="TValue">Type of the value to compare.</typeparam>
    /// <typeparam name="TResult">Type of the result returned by handler.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="result"><see langword="true"/> if the specified value is satisfied to the switch case;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>Type of the computation result.</returns>
    [ComVisible(false)]
    delegate TResult ParallelSwitchCase<in TValue, out TResult>(TValue value, out bool result);
}
