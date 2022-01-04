using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace FishNativeV2
{
    class Program
    {
        static string readFileFromPath(string path)
        {
            return File.ReadAllText(path);
        }
        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                string path = args[0];
                string source = readFileFromPath(path);
                parser Parser = new parser(source);
                interpreter Interpreter = new interpreter(Parser);
                Console.ReadKey();
            }
            else 
            {
                Console.WriteLine("No source file!");
            }
        }
    }
}
