using System;
using Library;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = ProvidedType.GetSample();
            foreach (ProvidedType.Root todo in data)
            {
                Console.WriteLine(todo.Title);
            }
        }

        static void add() {};
    }
}
