using SamScript.Compile;
using SamScript.Makers;
using System;
using System.IO;

namespace SamScript
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0) // create test project link
                args = new [] { @"C:\Projects\ConsoleTest" };
            else
            {
                if (File.Exists(args[0]))
                {
                    args[0] = Path.GetDirectoryName(args[0]);
                }
            }

            if(Directory.Exists(args[0]))
            {
                var compiler = new Compiler(args[0]);
                if(compiler.Compile())
                {
                    new JavaMaker().Make(compiler);
                    new CSharpMaker().Make(compiler);
                    new VbNetMaker().Make(compiler);
                    new JavaScriptMaker().Make(compiler);

                    Console.WriteLine("Build successfully...");
                }
                else
                {
                    Console.WriteLine("Build failed...");
                    // iterate through each error!
                }
            }            
        }
    }
}
