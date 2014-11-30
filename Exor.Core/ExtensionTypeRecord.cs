using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Exor.Core
{
    public class ExtensionTypeRecord
    {
        public readonly Type ExtensionType;
        public readonly Type AttributeType;
        internal readonly Type[] ConstructorParametersArray;

        public IReadOnlyCollection<Type> ConstructorParameters
        {
            get { return new ReadOnlyCollection<Type>(ConstructorParametersArray); }
        }

        private ExtensionTypeRecord(Type extensionType, Type attributeType, 
                                   IEnumerable<Type> constructorParameters)
        {
            ExtensionType = extensionType;
            AttributeType = attributeType;
            ConstructorParametersArray = constructorParameters as Type[] ?? constructorParameters.ToArray();
        }

        public static ExtensionTypeRecord Create<TExtensionType, TExtensionAttributeType>(
                IEnumerable<Type> constructorParameters)
            where TExtensionType : class 
            where TExtensionAttributeType : ExtensionAttribute
        {
            return new ExtensionTypeRecord(typeof(TExtensionType), typeof(TExtensionAttributeType), constructorParameters);
        }

        public static ExtensionTypeRecord Create<TExtensionType, TExtensionAttributeType>(
                params Type[] constructorParameters)
            where TExtensionType : class 
            where TExtensionAttributeType : ExtensionAttribute
        {
            return new ExtensionTypeRecord(typeof(TExtensionType), typeof(TExtensionAttributeType), constructorParameters);
        }
    }
}
