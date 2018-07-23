using System;
using System.Collections.Generic;
using System.Text;

namespace SamScript.Compile
{
    public class SamFile
    {
        public string Name;
        public string File;
        public List<SamFileField> Fields = new List<SamFileField>();
        public List<SamFileMethod> Methods = new List<SamFileMethod>();

        public void AddField(string name, string type, bool isStatic)
        {
            Fields.Add(new SamFileField() { Name = name, Type = type, IsStatic = isStatic });
        }

        public SamFileMethod AddMethod(string name, string type, bool isStatic)
        {
            var method = new SamFileMethod() { Name = name, Type = type, IsStatic = isStatic };
            Methods.Add(method);

            return method;
        }
    }
}
