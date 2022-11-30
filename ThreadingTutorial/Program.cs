namespace ThreadingTutorial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Example1 example1 = new Example1();
            example1.Main();

            Console.WriteLine("For the next tutorial press any key");
            Console.WriteLine("\n\n\n\n\n\n");
            Console.ReadKey();

            Example2 example2 = new Example2();
            example2.Main();

            Console.WriteLine("Maybe I will write more tutorials about this");
            Console.ReadKey();
        }
    }
}