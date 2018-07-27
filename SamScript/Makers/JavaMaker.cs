using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SamScript.Compile;
using SamScript.Compile.Tokens;

namespace SamScript.Makers
{
    public class JavaMaker : IMaker
    {
        public Compiler _compiler;
        public void Make(Compiler compiler)
        {
            _compiler = compiler;
            foreach (var item in compiler.Files)
            {
                MakeFile(item);
            }
        }

        public void MakeField(SamFileField field, StringBuilder builder)
        {
            throw new NotImplementedException();
        }

        public void MakeFile(SamFile file)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"public class {file.Name} " + "{");
          
            if (file.Fields != null)
            {
                foreach (var item in file.Fields)
                {
                    MakeField(item, builder);
                }
            }

            if (file.Methods != null)
            {
                var first = file.Methods.FirstOrDefault();

                foreach (var item in file.Methods)
                {
                    if (first != item)
                        builder.AppendLine();
                    MakeMethod(item, builder);
                }
            }

            builder.AppendLine("}");

            _compiler.ToOutput(file, builder, ".java");            
        }

        public void MakeMethod(SamFileMethod method, StringBuilder builder)
        {            
            builder.Append("\tpublic ");
            if (method.IsStatic)
                builder.Append("static ");
            if (method.Name == "Main")
                method.Name = "main";

            builder.Append($"{method.Type} {method.Name}(");

            foreach (var item in method.Arguments)
            {
                builder.Append($"{item.Type} {item.Name}, ");
            }
            if (method.Arguments.Count > 0)
                builder.Length -= 2; // remove ", "

            builder.Append(") {");
            builder.AppendLine();
            builder.Append("\t\t");
            // build body.
            MakeMethodBody(method, builder);

            builder.AppendLine("\t}");
        }

        public void MakeMethodBody(SamFileMethod method, StringBuilder builder)
        {
            string tabs = "\t\t";
            var tokens = method.Tokens;
            var lastToken = tokens.LastOrDefault();

            foreach (var item in tokens)
            {
                if (item is CallFuncToken func)
                {
                    if (func.FuncName == "Write")
                    {
                        builder.Append("System.out.print(");
                    }
                    else if (func.FuncName == "WriteLine")
                    {
                        builder.Append("System.out.println(");
                    }
                    else
                    {
                        builder.Append(func.FuncName);
                    }                    
                }
                else if (item is EndCallFuncToken)
                {
                    builder.Append(")");
                }
                else if (item is StringLiteralToken stringLiteral)
                {
                    builder.Append(stringLiteral.Value);
                }
                else if (item is EndOfLineToken)
                {
                    builder.Append(";");
                    builder.AppendLine();
                    if (item != lastToken)
                        builder.Append(tabs);
                }
            }
        }
    }
}
