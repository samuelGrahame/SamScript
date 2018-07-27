﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SamScript.Compile;
using SamScript.Compile.Tokens;

namespace SamScript.Makers
{
    public class CSharpMaker : IMaker
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
            builder.Append("\tpublic ");
            if (field.IsStatic)
                builder.Append("static ");

            builder.Append($"{field.Type} {field.Name};");
        }

        public void MakeFile(SamFile file)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"public class {file.Name}");
            builder.AppendLine("{");
            bool HasContent = false;

            if (file.Fields != null)
            {
                foreach (var item in file.Fields)
                {
                    if (HasContent)
                        builder.AppendLine();
                    MakeField(item, builder);
                    HasContent = true;
                }
            }
            if (HasContent)
                builder.AppendLine();
            if (file.Methods != null)
            {
                var first = file.Methods.FirstOrDefault();

                foreach (var item in file.Methods)
                {
                    if (HasContent)
                        builder.AppendLine();
                    MakeMethod(item, builder);
                    HasContent = true;
                }
            }

            builder.AppendLine("}");

            _compiler.ToOutput(file, builder, ".cs");
        }

        public void MakeMethod(SamFileMethod method, StringBuilder builder)
        {            
            builder.Append("\tpublic ");
            if (method.IsStatic)
                builder.Append("static ");

            builder.Append($"{method.Type} {method.Name}(");

            foreach (var item in method.Arguments)
            {
                builder.Append($"{item.Type} {item.Name}, ");
            }
            if (method.Arguments.Count > 0)
                builder.Length -= 2; // remove ", "

            builder.Append(")");
            builder.AppendLine();
            builder.AppendLine("\t{");
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
                    builder.Append(";");
                    builder.AppendLine();
                    if (item != lastToken)
                        builder.Append(tabs);
                }
                else if (item is LoadVariableToken varToken)
                {
                    builder.Append(varToken.Name);
                }else if (item is EqualToken)
                {
                    builder.Append("==");
                }
            }
        }
    }
}
