using SamScript.Compile.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SamScript.Compile
{
    public class Compiler
    {
        public string ProjectRootDirectory;
        public CompilerOutput Output = CompilerOutput.Javascript;
        public List<SamFile> Files = new List<SamFile>();

        public void ToOutput(SamFile file, StringBuilder builder, string extension)
        {
            string plusOut = Path.Combine(ProjectRootDirectory, "Output");
            if (!Directory.Exists(plusOut))
            {
                Directory.CreateDirectory(plusOut);
            }
            string subFolders = Path.GetDirectoryName(file.File).Substring(ProjectRootDirectory.Length);
            string start;
            if (subFolders != "")
            {
                subFolders = subFolders.Substring(1);
                start = Path.Combine(plusOut, subFolders);

                if (!Directory.Exists(start))
                {
                    Directory.CreateDirectory(start);
                }
            }
            else
            {
                start = plusOut;
            }

            File.WriteAllText(Path.Combine(start, file.Name + extension), builder.ToString());

        }

        public Compiler(string projectRootDirectory)
        {
            ProjectRootDirectory = projectRootDirectory;
        }

        public bool Compile()
        {
            foreach (var file in GetCodeFiles(ProjectRootDirectory))
                if (!CompileFile(file))
                    return false;

            return true;
        }  
        
        private string[] GetCodeFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.sam", SearchOption.AllDirectories);
        }

        private bool CompileFile(string file)
        {
            var samFile = new SamFile()
            {
                File = file,
                Name = Path.GetFileNameWithoutExtension(file)
            };
            Files.Add(samFile);

            using (TokenReader tr = new TokenReader(File.ReadAllText(file)))
            {
                return CompileFieldsAndMethods(samFile, tr);
            }
        }

        private enum CompileFieldAndMethodsEnum
        {
            NeedType,
            NeedName
        }

        private bool IsStringLiteral(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.StartsWith('"') && value.EndsWith('"');
        }

        private bool CompileMethodBody(SamFile file, TokenReader tr, SamFileMethod method)
        {
            int level = 0;
            int softLevel = 0;
            do
            {
                if (tr.Current == "{")
                    level++;
                else if (tr.Current == "}")
                {
                    level--;
                    if (level == 0)
                    {
                        return true;
                    }
                    else if (level < 0)
                        return false;
                }
                else
                {
                    string value = tr.Current;

                    if(value == ";")
                    {
                        method.Tokens.Add(new EndOfLineToken());
                        continue;
                    }

                    if(IsStringLiteral(value))
                    {
                        // maybe remove quotes.
                        method.Tokens.Add(new StringLiteralToken() { Value = value });
                        continue;
                    }

                    // is a function
                    if(tr.CanMoveNext)
                    {
                        if(tr.Next == "(")
                        {
                            // this is a func call..
                            var callFunc = new CallFuncToken() { FuncName = value };

                            method.Tokens.Add(callFunc);
                            softLevel++;

                            tr.MoveNext();

                            continue;

                        }
                        else if (tr.Next == "=")
                        {
                            // this is a set call..
                            continue;
                        }
                    }

                    if(tr.Current == ")")
                    {
                        method.Tokens.Add(new EndCallFuncToken());

                        softLevel--;
                        if (softLevel < 0)
                            return false; // strange, why do we have a closing brace and no open one??
                    }else if(tr.Current == "==")
                    {
                        method.Tokens.Add(new EqualToken());
                    }
                    else
                    {
                        // just add...

                        method.Tokens.Add(new LoadVariableToken() { Name = tr.Current });
                    }

                    // code..
                }
            } while (tr.MoveNext());

            return true;
        }

        private bool CompileMethod(SamFile file, TokenReader tr, SamFileMethod method)
        {
            // type is return type
            List<SamFileField> arguments = new List<SamFileField>();

            do
            {
                if (tr.Current == ",")
                    continue;

                if(tr.Current == ")")
                {
                    // read body...
                    if (tr.MoveNext())
                    {
                        method.Arguments = arguments;
                        return CompileMethodBody(file, tr, method);
                    }                        
                    else
                        return false;
                }
                else
                {
                    string argType = tr.Current;
                    if (!tr.MoveNext())
                        return false;
                    string argName = tr.Current;
                    arguments.Add(new SamFileField() { Name = argName, Type = argType });
                }
            } while (tr.MoveNext());

            return true;
        }

        private bool CompileFieldsAndMethods(SamFile file, TokenReader tr)
        {            
            do
            {
                bool isStatic = false;
                if(tr.Current == "static")
                {
                    isStatic = true;

                    if (!tr.MoveNext())
                        return false;
                }

                string type = tr.Current;
                if (!tr.MoveNext())
                    return false;
                string name = tr.Current;
                if (!tr.MoveNext())
                    return false;

                if (tr.Current == ";")
                {
                    // a single declare field;
                    file.AddField(name, type, isStatic);
                }else
                {
                    if(tr.Current == "(" && tr.MoveNext())
                    {
                        // should be a func
                        if(!CompileMethod(file, tr, file.AddMethod(name, type, isStatic)))
                        {
                            return false;
                        }
                    }
                }
            } while (tr.MoveNext());


            return true;
        }

    }

    public enum CompilerOutput
    {
        Javascript,
        CSharp
    }
}
