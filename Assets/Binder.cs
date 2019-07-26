using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    /// <summary>
    /// to deserailize dynamically the message you received into the correct object
    /// </summary>
    class Binder : SerializationBinder
    {
        // all the types
        public Type[] types;

        // constructor
        public Binder(Type[] Types)
        {
            types = Types;
        }

        /// <summary>
        /// bind the object to the correct type existing in the assembly you gave
        /// </summary>
        /// <param name="assemblyName">the assembly to look into for the type</param>
        /// <param name="typeName">name of the type</param>
        /// <returns></returns>
        public override Type BindToType(string assemblyName, string typeName)
        {

            if (assemblyName == "BaseElement") // maybe this is useless, just so u know everything is in this assembly
            {

                var type = types.Where(t => t.Name == typeName).FirstOrDefault();

                if (type != null)
                    return type;

            }
            return Type.GetType(typeName + ", " + assemblyName);
        }

        
    }
} // credit: https://stackoverflow.com/questions/27138303/deserialize-runtime-created-class
