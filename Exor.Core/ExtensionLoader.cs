using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Common.Logging;

namespace Exor.Core
{
    /// <summary>
    /// Reflection-based loader that scans a set of supplied assemblies for extension classes tagged
    /// with a specified attribute and key nane.
    /// </summary>
    public class ExtensionLoader : IExtensionLoader
    {
        // TODO: the deep find methods probably do a double copy; my LINQ is not being helpful.

        private readonly ILog _logger;

        public static readonly Object[] EmptyParams = { };
        public static readonly Type[] NoParameters = { };

        private readonly Dictionary<TypeInfo, ExtensionTypeRecord> _typeRecords;
        private readonly Dictionary<Type, ExtensionTypeRecord> _attributeMap; 
        public IEnumerable<Type> SupportedTypes { get { return _typeRecords.Values.Select(etr => etr.ExtensionType); } }

        private readonly Dictionary<TypeInfo, Dictionary<String, List<ConstructorInfo>>> _lookupTable;
        private readonly bool _throwIfConstructorMissing;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assemblies">
        /// The set of assemblies, in descending order of importance, to scan for this loader.
        /// </param>
        /// <param name="typeRecords">
        /// The set of type records to use in this loader. Each must have a unique ExtensionType.
        /// </param>
        /// <param name="throwIfConstructorMissing">
        /// Determines whether an exception should be thrown (and, by implication, extension loading
        /// halted) if a constructor with the given signature is not found.
        /// </param>
        public ExtensionLoader(IEnumerable<Assembly> assemblies, IEnumerable<ExtensionTypeRecord> typeRecords, Boolean throwIfConstructorMissing = true)
        {
            _logger = LogManager.GetLogger(GetType());

            _throwIfConstructorMissing = throwIfConstructorMissing;
            var typeRecordsList = typeRecords.ToList();
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("ExtensionLoader records:");
                foreach (var record in typeRecordsList) _logger.DebugFormat("{0} -> {1} [{2}]", 
                    record.AttributeType.FullName, record.ExtensionType.FullName,
                    String.Join(", ", record.ConstructorParametersArray.Select(t => t.FullName)));
            }

            var dupeTypeRecord =
                typeRecordsList.FirstOrDefault(
                    etr1 => typeRecordsList.Any(etr2 => etr1 != etr2 && etr1.ExtensionType == etr2.ExtensionType));
            if (dupeTypeRecord != null)
            {
                throw new ExorLoaderException("Duplicate ExtensionTypeRecords with ExtensionType of '{0}'.", dupeTypeRecord.ExtensionType.FullName);
            }


            _typeRecords = typeRecordsList.ToDictionary(tr => tr.ExtensionType.GetTypeInfo());
            _attributeMap = typeRecordsList.ToDictionary(etr => etr.AttributeType, etr => etr);
            _lookupTable = BuildLookupTable(assemblies);
        }



        private Dictionary<TypeInfo, Dictionary<String, List<ConstructorInfo>>> BuildLookupTable(
                IEnumerable<Assembly> assemblies)
        {
            var assemblyEnumerable = assemblies as IList<Assembly> ?? assemblies.ToList();
            if (_logger.IsDebugEnabled) 
            {
                _logger.DebugFormat("Assemblies in extension loader:");
                foreach (String name in assemblyEnumerable.Select(a => a.FullName))
                    _logger.DebugFormat(" - {0}", name);
            }

            var ctorDict = new Dictionary<TypeInfo, Dictionary<String, List<ConstructorInfo>>>(_typeRecords.Count);

            foreach (var t in _typeRecords.Values) ctorDict.Add(t.ExtensionType.GetTypeInfo(), 
                                                                   new Dictionary<String, List<ConstructorInfo>>());

            // ensures that every type satisfies at least one entry in _typeRecords and can be purely
            // checked against ExtensionTypeRecord.AttributeType.
            var assemblyTypeLists = assemblyEnumerable.Select(
                a => a.DefinedTypes.Where(ti => ctorDict.Keys.Any(tk => tk.IsAssignableFrom(ti)))
            );

            foreach (var typeList in assemblyTypeLists)
            {
                foreach (var type in typeList)
                {
                    var record = type.CustomAttributes.Where(
                        a => _attributeMap.ContainsKey(a.AttributeType))
                            .Select(a => _attributeMap[a.AttributeType])
                            .OrderBy(a => a.AttributeType.FullName)
                            .FirstOrDefault();

                    if (record == null) continue;

                    var attr = (ExtensionAttribute)type.GetCustomAttribute(record.AttributeType);

                    Dictionary<String, List<ConstructorInfo>> typeCtorDict;
                    if (!ctorDict.TryGetValue(record.ExtensionType.GetTypeInfo(), out typeCtorDict))
                    {
                        typeCtorDict = new Dictionary<String, List<ConstructorInfo>>();
                        ctorDict[record.ExtensionType.GetTypeInfo()] = typeCtorDict;
                    }

                    var ctor = type.DeclaredConstructors.FirstOrDefault(
                        c => record.ConstructorParametersArray.SequenceEqual(c.GetParameters().Select(p => p.ParameterType))
                    );

                    if (ctor == null)
                    {
                        if (_throwIfConstructorMissing)
                        {
                            throw new ExorLoaderException("Could not find appropriate constructor ({0}) for extension type '{1}'.",
                                                                String.Join(", ", record.ConstructorParametersArray.Select(t => t.FullName)),
                                                                type.FullName);
                        }
                        continue;
                    }

                    List<ConstructorInfo> ctorList;
                    if (!typeCtorDict.TryGetValue(attr.Key, out ctorList))
                    {
                        ctorList = new List<ConstructorInfo>();
                        typeCtorDict[attr.Key] = ctorList;
                    }
                    ctorList.Add(ctor);
                }
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("{0} extension types found across all assemblies:", ctorDict.Keys.Count);
                foreach (var type in ctorDict.Keys)
                {
                    var extensions = ctorDict[type];
                    _logger.DebugFormat(" - {0} ({1})", type.FullName, extensions.Sum(kvp => kvp.Value.Count));
                    if (_logger.IsTraceEnabled)
                    {
                        foreach (var ctorMapping in extensions)
                        {
                            _logger.TraceFormat("   - '{0}':", ctorMapping.Key);
                            foreach (var ctor in ctorMapping.Value)
                            {
                                _logger.TraceFormat("     - {0}", ctor.DeclaringType.FullName);
                            }
                        }
                    }
                }
            }

            return ctorDict;
        }

        public ConstructorInfo FindConstructor<TExtensionType>(String key)
            where TExtensionType : class
        {
            var retval = DeepFindConstructor<TExtensionType>(key).FirstOrDefault();
            if (retval == null)
            {
                throw new ExorLoaderException("No extension for type '{0}' and key '{1}' found.", 
                                                    typeof(TExtensionType).FullName, key);
            }
            return retval;
        }

        public IReadOnlyDictionary<String, ConstructorInfo> FindAllConstructors<TExtensionType>()
            where TExtensionType : class
        {
            var info = typeof(TExtensionType).GetTypeInfo();
            _logger.Debug(m => m("wide constructor lookup: {0}", info.FullName));

            Dictionary<String, List<ConstructorInfo>> ctorDict;
            if (_lookupTable.TryGetValue(info, out ctorDict))
            {
                return ctorDict.Select(kvp => Tuple.Create(kvp.Key, kvp.Value.FirstOrDefault()))
                               .Where(t => t.Item2 != null)
                               .ToDictionary(t => t.Item1, t => t.Item2);
            }

            return new Dictionary<String, ConstructorInfo>();
        }

        public IReadOnlyDictionary<String, IReadOnlyList<ConstructorInfo>> DeepFindAllConstructors<TExtensionType>()
            where TExtensionType : class
        {
            var info = typeof(TExtensionType).GetTypeInfo();
            _logger.Debug(m => m("deep wide constructor lookup: {0}", info.FullName));

            Dictionary<String, List<ConstructorInfo>> ctorDict;
            if (_lookupTable.TryGetValue(info, out ctorDict))
            {
                var retDict = new Dictionary<String, IReadOnlyList<ConstructorInfo>>();
                foreach (var kvp in ctorDict)
                {
                    retDict[kvp.Key] = kvp.Value.Select(ctor => ctor).ToList();
                }
                return retDict;
            }
            return new Dictionary<String, IReadOnlyList<ConstructorInfo>>();
        }

        public IEnumerable<ConstructorInfo> DeepFindConstructor<TExtensionType>(String key)
            where TExtensionType : class
        {
            var info = typeof(TExtensionType).GetTypeInfo();
            _logger.Debug(m => m("key/constructor lookup: {0} -> {1}", key, info.FullName));

            Dictionary<String, List<ConstructorInfo>> ctorDict;
            if (_lookupTable.TryGetValue(info, out ctorDict))
            {
                List<ConstructorInfo> ctors;
                if (ctorDict.TryGetValue(key, out ctors))
                {
                    if (_logger.IsDebugEnabled)
                    {
                        foreach (var c in ctors)
                            _logger.DebugFormat(" - {0}", c.DeclaringType.FullName);
                    }
                    return ctors;
                }
                return Enumerable.Empty<ConstructorInfo>();
            }

            throw new ExorLoaderException("Extension type '{0}' unrecognized.", info.FullName);
        }

        /// <summary>
        /// Loads the highest priority extension for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A instance of an extension type's subclass.</returns>
        public TExtensionType Load<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class
        {
            EnsureArgTypes<TExtensionType>(args);

            var ctor = FindConstructor<TExtensionType>(key);
            return (TExtensionType) ctor.Invoke(args);
        }

        /// <summary>
        /// Lazily loads all extensions for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A lazy enumerable of instances of the specified type/key's extensions.</returns>
        public IEnumerable<TExtensionType> DeepLoad<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class
        {
            EnsureArgTypes<TExtensionType>(args);

            var ctors = DeepFindConstructor<TExtensionType>(key);
            return ctors.Select(ctor => (TExtensionType) ctor.Invoke(args));
        }

        /// <summary>
        /// Eagerly loads all extensions for the given key.
        /// </summary>
        /// <typeparam name="TExtensionType">The extension type to load.</typeparam>
        /// <param name="key">The named key of the descendant of ExtensionAttribute you wish to load.</param>
        /// <param name="args">Arguments to be passed to the extended type's constructor.</param>
        /// <returns>A list of instances of the specified type/key's extensions.</returns>
        public List<TExtensionType> DeepEagerLoad<TExtensionType>(String key, params Object[] args)
            where TExtensionType : class
        {
            return DeepLoad<TExtensionType>(key, args).ToList();
        }

        public IReadOnlyDictionary<String, TExtensionType> LoadAll<TExtensionType>(params Object[] args)
            where TExtensionType : class
        {
            EnsureArgTypes<TExtensionType>(args);

            var ctors = FindAllConstructors<TExtensionType>();
            return ctors.ToDictionary(kvp => kvp.Key, kvp => (TExtensionType)kvp.Value.Invoke(args));
        }

        public IReadOnlyDictionary<String, IReadOnlyList<TExtensionType>> DeepLoadAll<TExtensionType>(params Object[] args)
            where TExtensionType : class
        {
            EnsureArgTypes<TExtensionType>(args);

            var ctors = DeepFindAllConstructors<TExtensionType>();
            var dict = new Dictionary<String, IReadOnlyList<TExtensionType>>(ctors.Count);
            foreach (var kvp in ctors)
            {
                // TODO: figure out how to LINQ this, my mess of selects was not awesome.
                dict.Add(kvp.Key, kvp.Value.Select(c => (TExtensionType)c.Invoke(args)).ToList());
            }
            return dict;
        }

        private void EnsureArgTypes<TExtensionType>(IEnumerable<Object> args)
            where TExtensionType : class
        {
            var argTypes = args.Select(o => o.GetType()).ToList();
            var extTypeInfo = typeof(TExtensionType).GetTypeInfo();
            var etr = _typeRecords[extTypeInfo];
            if (!etr.ConstructorParametersArray.SequenceEqual(argTypes))
            {
                throw new ExorLoaderException("Invalid arg types when attempting to load object of type '{0}': expected [{1}], got [{2}].",
                                                    extTypeInfo.FullName,
                                                    String.Join(", ", etr.ConstructorParametersArray.Select(t => t.FullName)),
                                                    String.Join(", ", argTypes.Select(t => t.FullName)));
            }
        }
    }
}
