using System;

namespace Seamstress
{
    class SeamCarverMain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            new SeamCarverTest().Run();
            Console.WriteLine("Ended");
        }
    }
}
