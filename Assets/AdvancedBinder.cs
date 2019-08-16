using OpencascadePart.Elements;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Assets
{
    /// <summary>
    /// binder to bind BaseElement (theoretical objects -> Elements.dll) to BasicElement (real objects-> OpencascadePart.dll)
    /// </summary>
    public class AdvancedBinder : SerializationBinder
    {
        

        readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="typeNames">a dictionary containing names and types</param>
        public AdvancedBinder(Dictionary<Type, string> typeNames = null)
        {
            if (typeNames != null)
            {
                foreach (var typeName in typeNames)
                {
                    Map(typeName.Key, typeName.Value);
                }
            }
        }
        /// <summary>
        /// constructor with all the types, we use their name as Name
        /// </summary>
        /// <param name="types">an array of Type for our dictionary of Type</param>
        public AdvancedBinder(Type[] types)
        {
            foreach (var type in types)
            {
                Map(type, type.Name);
            }
        }
        /// <summary>
        /// constructor that gets all the types of the assembly and bind them to their name
        /// </summary>
        /// <param name="assemblyName"></param>
        public AdvancedBinder(string assemblyName)
        {
            List<Type> myTypes = new List<Type>();
            Assembly[] myAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < myAssemblies.Length; i++)
            {
                if (myAssemblies[i].GetName().Name == assemblyName)
                {
                    foreach (Type t in myAssemblies[i].GetTypes())
                    {
                        if (t.IsSubclassOf(typeof(BasicElement)))
                        {
                            myTypes.Add(t);
                        }
                    }
                    break;
                }
            }

            foreach (var type in myTypes)
            {
                Map(type, type.Name);
            }
        }
        /// <summary>
        /// constructor that gets OpencascadePart.dll and bind the types, that inherit from BasicElement, to their name
        /// </summary>
        public AdvancedBinder()
        {
            List<Type> myTypes = new List<Type>();
            Assembly[] myAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < myAssemblies.Length; i++)
            {
                if (myAssemblies[i].GetName().Name == "OpencascadePart")
                {
                    foreach (Type t in myAssemblies[i].GetTypes())
                    {
                        
                        if (t.IsSubclassOf(typeof(BasicElement)))
                        {
                            myTypes.Add(t);
                        }
                    }
                    break;
                }
            }

            foreach (var type in myTypes)
            {
                Map(type, "Elements."+type.Name); // Elements is the namespace for the elements' classes sent by the server
            }
        }

        /// <summary>
        /// map the name depending on the type, and the type depending on the name
        /// </summary>
        /// <param name="type">type of the object</param>
        /// <param name="name">name of the object's class</param>
        private void Map(Type type, string name)
        {
            this.typeToName.Add(type, name);
            this.nameToType.Add(name, type);
        }

        /// <summary>
        /// getter of Type with the name
        /// </summary>
        /// <param name="typeName">name of the Type</param>
        /// <returns>the type asked</returns>
        private Type Get(string typeName)
        {
            return nameToType[typeName];
        }

        /// <summary>
        /// getter to get the name of the Type in the dictionary
        /// </summary>
        /// <param name="type">The type you are wondering the name in the dictionary</param>
        /// <returns>the name of the type as it is in the dictionary</returns>
        private string Get(Type type)
        {
            return typeToName[type];
        }

        /// <summary>
        /// get the type and returns the name
        /// </summary>
        /// <param name="serializedType">the type of the object</param>
        /// <param name="assemblyName">the assembly containing the type</param>
        /// <param name="typeName">the name of the object's class</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            typeName = Get(serializedType);
            assemblyName = "OpencascadePart";
        }
       
        /// <summary>
        /// bind names to types
        /// </summary>
        /// <param name="assemblyName">the assembly's name</param>
        /// <param name="typeName">the name of the type</param>
        /// <returns>the type that is attached to the typeName</returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Get(typeName);
        }
    }
} // source: https://stackoverflow.com/questions/11099466/using-a-custom-type-discriminator-to-tell-json-net-which-type-of-a-class-hierarc
