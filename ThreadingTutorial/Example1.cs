using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadingTutorial
{
    public class Example1
    {
        public Example1()
        {
            Console.WriteLine(
                """
                Threading Example 1:
                
                NOTE: It is intended that you just read the code, and running the code
                is more for an example, and so that you can set break points and see 
                what is happening in the code.

                Press any key to continue.
                """);

            Console.ReadKey();

            Console.WriteLine(
                """
                Without doing anything we are already using threads. The Program.Main()
                function is processed by the "Main" thread. This thread is automatically
                created by the Kernel when it launches your program.

                If you want to create a new thread you use the "Thread" object. This is 
                a slow operation and should be done with care. 

                The following function creates a new thread, sets it as a "Background" 
                thread, and then returns the new thread.

                public Thread CreateNewThread()
                {
                    Thread t = new Thread(PrintThreadID);
                    t.IsBackground = true;
                    return t;
                }

                As you can see we pass something into the thread constructor. Just like
                how the Main thread processes the Program.Main() function, the new thread
                we create also needs a function. We pass in the PrintThreadID function.
                It prints "Hello from Thread # {Thread.CurrentThread.ManagedThreadId}"

                If we run the CreateNewThread() function it will return the new thread,
                but it will not start it. 

                To create a new thread we must call the Start() function that belongs 
                to the thread.

                See the Example1.Main() Function to see this in action.

                NOTE: Background Threads.
                A background thread is one which is attatched to the Main thread. This
                background thread will be destroyed by the kernel when the main thread
                exits.
                https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.isbackground?view=net-7.0

                """);
        }

        public void PrintThreadID()
        {
            Console.WriteLine($"Hello from Thread # {Thread.CurrentThread.ManagedThreadId}");
        }

        public Thread CreateNewThread()
        {
            Thread t = new Thread(PrintThreadID);
            t.IsBackground = true;
            return t;
        }

        public void Main()
        {
            Console.WriteLine("Creating thread:");
            Console.WriteLine("Thread t = CreateNewThread();");
            
            Thread t = CreateNewThread();
            
            Console.WriteLine("Calling PrintThreadID from Main thread");
            Console.WriteLine("PrintThreadID();");

            PrintThreadID();
            
            Console.WriteLine("Starting Thread t");
            Console.WriteLine("t.Start();");
            
            t.Start();
        }
    }
}
