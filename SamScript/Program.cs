using SamScript.Compile;
using System;
using System.IO;

namespace SamScript
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0) // create test project link
                args = new [] { @"C:\Users\samuel grahame\Documents\Visual Studio 2017\Projects\SamScript\SamScript\Demo" };

            if(Directory.Exists(args[0]))
            {
                var compiler = new Compiler(args[0]);
                if(compiler.Compile())
                {
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
