using System;
using System.Collections.Generic;

namespace Readymade.Utils {
    /// <summary>
    /// Extensions methods for <see cref="ISet{T}"/>.
    /// </summary>
    public static class SetExtensions
    {
        /// <summary>
        /// Adds a disposable to a given set. Simplifies creation and ue of a disposable set together with <see cref="DisposeAll"/>.
        /// </summary>
        /// <param name="disposable">The disposable this extension method is called on.</param>
        /// <param name="set">The set to which the disposable instance should be added.</param>
        /// <returns>The disposable that the methods was called on. Allows for method chaining.</returns>
        public static IDisposable AddTo(this IDisposable disposable, ref ISet<IDisposable> set)
        {
            set ??= new HashSet<IDisposable>();
            set.Add(disposable);
            return disposable;
        }

        /// <summary>
        /// Calls <see cref="IDisposable.Dispose"/> on all members of a set. Simplifies creation and use of a disposable set together with <see cref="DisposeAll"/>.
        /// </summary>
        /// <param name="set">The disposable set.</param>
        public static void DisposeAll(this ISet<IDisposable> set)
        {
            if (set == null)
            {
                return;
            }

            set.ForEach(it => it.Dispose());
            set.Clear();
        }
    }
}