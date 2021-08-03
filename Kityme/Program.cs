using System;

namespace Kityme
{
    class Program
    {
        static void Main(string[] args)
            => new Startup().RunAsync().GetAwaiter().GetResult();
    }
}