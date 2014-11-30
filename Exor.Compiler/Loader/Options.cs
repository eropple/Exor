using System;

namespace Exor.Compiler.Loader
{
    public class Options
    {
        public readonly InclusionStrategy InclusionStrategy;
        public readonly Boolean ThrowOnConstructorMissing;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inclusionStrategy">
        /// Defines the behavior of adding dependencies into the ExtensionLoader's load order.
        /// The code from a dependency will always be made available to the depending code source
        /// when compiling, but will not be added to the ExtensionLoader for accessibility (i.e.,
        /// to load when attributed objects with a key are found) unless this is set to a non-None
        /// value.
        /// </param>
        /// <param name="throwOnConstructorMissing">
        /// If true, an exception will be thrown when a type does not include a constructor that
        /// matches the constructor in the ExtensionTypeRecord. If false, it will be silently
        /// discarded.
        /// </param>
        public Options(InclusionStrategy inclusionStrategy = InclusionStrategy.None, Boolean throwOnConstructorMissing = true)
        {
            InclusionStrategy = inclusionStrategy;
            ThrowOnConstructorMissing = throwOnConstructorMissing;
        }
    }

    public enum InclusionStrategy
    {
        /// <summary>
        /// Dependencies are not added to the ExtensionLoader's load order.
        /// </summary>
        None,
        /// <summary>
        /// Dependencies are added to the ExtensionLoader's load order after the first
        /// ICodeSource that depends on that particular dependency.
        /// </summary>
        AfterFirst,
        /// <summary>
        /// Dependencies are added to the ExtensionLoader's load order after the last
        /// ICodeSource that depends on that particular dependency.
        /// </summary>
        AfterLast
    }
}
