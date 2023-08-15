using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace SportsNext.Shared.Binders
{
    internal class TypeNameSerializationBinder : ISerializationBinder

    {

        public List<string> TypeFormats { get; private set; }

        public TypeNameSerializationBinder(List<string> typeFormats)
        {
            TypeFormats = typeFormats;

        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            foreach (var typeFormat in TypeFormats)
            {
                try
                {
                    string resolvedTypeName = string.Format(typeFormat, typeName);
                    return Type.GetType(resolvedTypeName, true);
                }
                catch (TypeLoadException)
                {
                    // continue on
                }
            }

            return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName)); ;
        }
    }
}