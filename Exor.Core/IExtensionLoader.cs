using System;
using System.Collections.Generic;

namespace Exor.Core
{
    public interface IExtensionLoader
    {
        /// <summary>
        /// All extension types supported by this loader.
        /// </summary>
        IEnumerable<Type> SupportedTypes { get; }

        /// <summary>
        /// Loads the highest priority extension for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A instance of an extension type's subclass.</returns>
        TExtensionType Load<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class;

        /// <summary>
        /// Lazily loads all extensions for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A lazy enumerable of instances of the specified type/key's extensions.</returns>
        IEnumerable<TExtensionType> DeepLoad<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class;

        /// <summary>
        /// Eagerly loads all extensions for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A list of instances of the specified type/key's extensions.</returns>
        List<TExtensionType> DeepEagerLoad<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class;
    }
}
