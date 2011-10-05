using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents cache of the compiled scripts.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptCache
    {
        /// <summary>
        /// Returns compiled script loaded previously loaded from file or compiles script on-fly.
        /// </summary>
        /// <param name="scriptFile">The path to the script file. Cannot be <see langword="null"/>.</param>
        /// <param name="compiler">The delegate that implements script compilation. Cannot be <see langword="null"/>.</param>
        /// <returns>The compiled script.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="scriptFile"/> or <paramref name="compiler"/> is <see langword="null"/>.</exception>
        IScriptCookie Lookup(Uri scriptFile, Func<IEnumerator<char>, ScriptInvoker> compiler);

        /// <summary>
        /// Determines whether the specified script source is cached.
        /// </summary>
        /// <param name="scriptFile">The location of the script source.</param>
        /// <returns><see langword="true"/> if the specified script source is cached; otherwise, <see langword="false"/>.</returns>
        bool Cached(Uri scriptFile);

        /// <summary>
        /// Gets collection of cached modules.
        /// </summary>
        IEnumerable<Uri> Modules { get; }
    }
}
