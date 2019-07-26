using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    class Binder : SerializationBinder
    {

        public Type[] types;


        public Binder(Type[] Types)
        {
            types = Types;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {

            if (assemblyName == "BaseElement")
            {

                var type = types.Where(t => t.Name == typeName).FirstOrDefault();

                if (type != null)
                    return type;

            }
            return Type.GetType(typeName + ", " + assemblyName);
        }

        
    }
} // credit: https://stackoverflow.com/questions/27138303/deserialize-runtime-created-class
