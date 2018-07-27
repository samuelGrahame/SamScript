using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SamScript.Compile;
using SamScript.Compile.Tokens;

namespace SamScript.Makers
{
    public class VbNetMaker : IMaker
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
            builder.AppendLine($"Public Class {file.Name}");

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

            builder.AppendLine("End Class");

            _compiler.ToOutput(file, builder, ".vb");            
        }

        public void MakeMethod(SamFileMethod method, StringBuilder builder)
        {
            builder.Append("\tPublic ");
            if (method.IsStatic)
                builder.Append("Shared ");
            string methodType = (method.Type == "void" ? "Sub" : "Function");
            builder.Append($"{methodType} {method.Name}(");

            foreach (var item in method.Arguments)
            {
                builder.Append($"{item.Name} As {item.Type}, ");
            }
            if (method.Arguments.Count > 0)
                builder.Length -= 2; // remove ", "

            builder.Append(")");
            if(method.Type != "void")
                builder.Append($" As {method.Type}");
            //builder.AppendLine();
            builder.AppendLine();
            builder.Append("\t\t");
            // build body.
            MakeMethodBody(method, builder);

            builder.AppendLine($"\tEnd {methodType}");
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
                    if (func.FuncName == "Write" || func.FuncName == "WriteLine")
                    {
                        builder.Append($"System.Console.{func.FuncName}(");
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
                    //builder.Append(";");
                    builder.AppendLine();
                    if (item != lastToken)
                        builder.Append(tabs);
                }
            }
        }
    }
}
