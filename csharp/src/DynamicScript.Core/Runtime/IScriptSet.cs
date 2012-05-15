using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents finite unordered set of objects.
    /// </summary>
    /// <remarks>This contract is equal to FinSet category in Category Theory.</remarks>
    [ComVisible(false)]
    public interface IScriptSet: IScriptContract, IEnumerable<IScriptObject>, IEquatable<IEnumerable<IScriptObject>>
    {
        /// <summary>
        /// Gets count of elements in the set.
        /// </summary>
        long Count { get; }

        /// <summary>
        /// Gets contract binding of each object in the set.
        /// </summary>
        IScriptContract UnderlyingContract { get; }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">Other set to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="strict">Specifies proper(strict) subset detection.</param>
        /// <returns><see langword="true"/> if the current set is a subset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        bool IsSubsetOf(IEnumerable<IScriptObject> other, bool strict);

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <param name="strict">Specifies proper(strict) superset detection.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        bool IsSupersetOf(IEnumerable<IScriptObject> other, bool strict);

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the current set and other share at least one common element; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        bool Overlaps(IEnumerable<IScriptObject> other);

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        IScriptSet ExceptWith(IEnumerable<IScriptObject> other);

        /// <summary>
        /// Returns set with elements contained in the current set and the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        IScriptSet IntersectWith(IEnumerable<IScriptObject> other);
        
        /// <summary>
        /// Returns a new set so that it contains only elements that are present either in the current 
        /// set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        IScriptSet SymmetricExceptWith(IEnumerable<IScriptObject> other);

        /// <summary>
        /// Returns a new set so that it contains all elements that are present in both the current 
        /// set and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        IScriptSet UnionWith(IEnumerable<IScriptObject> other);
    }
}
