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

            using (TokenReader tr = new TokenReader(File.ReadAllText(file)))
            {
                CompileFieldsAndMethods(samFile, tr);
            }

            return false;
        }

        private enum CompileFieldAndMethodsEnum
        {
            NeedType,
            NeedName
        }

        private bool CompileMethodBody(SamFile file, TokenReader tr, SamFileMethod method)
        {
            int level = 0;
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
                        return CompileMethodBody(file, tr, method);
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
