using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ThreadingTutorial
{
    public class Example2
    {
        public int size = 8192;

        public Example2()
        {
            Console.WriteLine(
                """
                Threading Example 2:
                
                Do not forget: 
                
                It is intended that you just read the code, and running the code
                is more for an example, and so that you can set break points and see 
                what is happening in the code.

                Press any key to continue.
                """);

            Console.ReadKey();


            Console.WriteLine(
                $$"""
                When you are threading you must not write the same memory at the same 
                time as a different thread, and you cannot read memory that is being 
                written at the same time. This is incredibly important because it can
                lead to super complicated bugs called race conditions.

                When you have multiple threads trying to touch the same memory, there
                is no way to know which thread will “win” and reach the memory first.
                Essentially they “race” to the memory.

                However, if you are careful not to write memory that is being read, 
                you can read memory from multiple threads at the same time, with no 
                issue.

                There are however classes of problems where you only need to write 
                different data in each thread.

                An example of this is generating a bitmap. Lets say you wanted to 
                generate a 4k x 4k bitmap with a smooth gradient. 

                This can be done with as many threads as you have pixels, but it 
                will not perform very well.

                NOTE: For a primer an RGB bitmap is an array of bytes of size 
                w * h * 3. We will create this to store our bitmap.

                byte[] bitmapBytes = new byte[{{size}} * {{size}} * 3];

                Now lets create a function that writes each pixel in the bitmap:

                public void writeBitmapPixel(byte[] bitmapBytes, int x, int y)
                {
                    int index = (y * {{size}} + x) * 3;
                    bitmapBytes[index] = (byte)((y / (float){{size}}) * 255);
                    bitmapBytes[index + 1] = (byte)((x / (float){{size}}) * 255);
                    bitmapBytes[index + 2] = 0;
                }

                We can now call this for each pixel. 

                for(int y = 0; y < {{size}}; y++)
                {
                    for (int x = 0; x < {{size}}; x++)
                    {
                        writeBitmapPixel(bitmapBytes, x, y);
                    }
                }

                The simplest (and kinda naive) way is to use parallel.for() instead
                of the for loops but this can be rather slow, because there is a 
                TON of overhead for each call to writeBitmapPixel. 

                We can be smarter about the nested for loops, and maybe save some speed?

                If you still want to use parallel.for() you can call parallel.for()
                for each core in your system, manually spreading out the load. 

                But what if we create all the threads manually?

                If I run Example2.Main() in release mode without debugging I get the 
                following results:
                
                Non-threaded bitmap generation took: 312.319 MS
                
                Naive-threaded bitmap generation took: 67.398 MS
                
                Better-threaded bitmap generation took: 70.1223 MS
                
                Better-er-threaded bitmap generation took: 52.1302 MS

                Just threads thread creation time: 0.0401 MS
                Just threads bitmap generation took: 48.6003 MS
                Total Just threads bitmap generation took: 48.6404 MS

                Interestingly the nested parallel.for() calls isn't as slow as I expected.

                The just threads method is barely faster and not that much more complex.
                Plus you get to actually see what is happening, and what resources are allocated.
                If you wanted to you could even keep the threads alive and send more commands to
                them with a queue. 

                That will be the next example.
                """);
        }

        public void writeBitmapPixel(byte[] bitmapBytes, int x, int y)
        {
            int index = (y * size + x) * 3;

            bitmapBytes[index] = (byte)((y / (float)size) * 255f);
            bitmapBytes[index + 1] = (byte)((x / (float)size) * 255f);
            bitmapBytes[index + 2] = 0;
        }

        public void Main()
        {
            Console.WriteLine("Creating the bitmap data");
            byte[] bitmapBytes = new byte[size * size * 3];

            NonThreadedBitmapGeneration(bitmapBytes);

            NaiveThreadedBitmapGeneration(bitmapBytes);

            BetterThreadedBitmapGeneration(bitmapBytes);

            BettererThreadedBitmapGeneration(bitmapBytes);

            JustThreadsBitmapGeneration(bitmapBytes);

        }

        public void NonThreadedBitmapGeneration(byte[] bitmapBytes)
        {
            Console.WriteLine("Timing how long the non-threaded bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    writeBitmapPixel(bitmapBytes, x, y);
                }
            }

            timer.Stop();

            TimeSpan nonThreadedTime = timer.Elapsed;

            Console.WriteLine($"Non-threaded bitmap generation took: {nonThreadedTime.TotalMilliseconds} MS");
        }

        public void NaiveThreadedBitmapGeneration(byte[] bitmapBytes)
        {
            Console.WriteLine("Timing how long the naive-threaded bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            // NOTE: this feels really dumb to do
            Parallel.For(0, size, (y) =>
            {
                Parallel.For(0, size, (x) =>
                {
                    writeBitmapPixel(bitmapBytes, x, y);
                });
            });

            timer.Stop();

            TimeSpan naiveThreadedTime = timer.Elapsed;

            Console.WriteLine($"Naive-threaded bitmap generation took: {naiveThreadedTime.TotalMilliseconds} MS");

        }

        public void BetterThreadedBitmapGeneration(byte[] bitmapBytes)
        {
            Console.WriteLine("Timing how long the better-threaded bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            // NOTE: each pixel is 3 bytes, so we do not multiply this by 3, unlike the 
            // bitmapBytes length.
            int totalPixels = size * size;

            // NOTE: this is better
            Parallel.For(0, totalPixels, (index) =>
            {
                int y = index / size;
                int x = index % size;

                writeBitmapPixel(bitmapBytes, x, y);
            });

            timer.Stop();

            TimeSpan betterThreadedTime = timer.Elapsed;

            Console.WriteLine($"Better-threaded bitmap generation took: {betterThreadedTime.TotalMilliseconds} MS");

        }

        public void BettererThreadedBitmapGeneration(byte[] bitmapBytes)
        {
            Console.WriteLine("Timing how long the better-er-threaded bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            int totalPixels = size * size;
            int processorCount = Environment.ProcessorCount;
            int perCorePixelCount = totalPixels / processorCount;

            // because we split the load over many threads we MIGHT have leftovers due to the need to use integer division.
            // this is guarenteed to be the processorCount or less.
            int leftovers = totalPixels - (perCorePixelCount * processorCount);

            Parallel.For(0, processorCount, (core) =>
            {
                int start = core * perCorePixelCount;
                int end = (core + 1) * perCorePixelCount;

                for (int index = start; index < end; index++)
                {
                    int y = index / size;
                    int x = index % size;

                    writeBitmapPixel(bitmapBytes, x, y);
                }
            });

            // the main thread can do the leftovers
            for (int index = totalPixels - leftovers; index < totalPixels; index++)
            {
                int y = index / size;
                int x = index % size;

                writeBitmapPixel(bitmapBytes, x, y);
            }

            timer.Stop();

            TimeSpan bettererThreadedTime = timer.Elapsed;

            Console.WriteLine($"Better-er-threaded bitmap generation took: {bettererThreadedTime.TotalMilliseconds} MS");

        }

        public void JustThreadsBitmapGeneration(byte[] bitmapBytes)
        {
            Console.WriteLine("Timing how long the just threads bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            int totalPixels = size * size;
            int processorCount = Environment.ProcessorCount;
            int perCorePixelCount = totalPixels / processorCount;

            // because we split the load over many threads we MIGHT have leftovers due to the need to use integer division.
            // this is guarenteed to be the processorCount or less.
            int leftovers = totalPixels - (perCorePixelCount * processorCount);

            Stopwatch creationTimer = new Stopwatch();
            creationTimer.Start();

            Thread[] threads = new Thread[processorCount];

            //create threads
            for (int i = 0; i < processorCount; i++)
            {
                // this is captured by the lamda
                int core = i;

                threads[i] = new Thread(() =>
                {
                    int start = core * perCorePixelCount;
                    int end = (core + 1) * perCorePixelCount;

                    for (int index = start; index < end; index++)
                    {
                        int y = index / size;
                        int x = index % size;

                        writeBitmapPixel(bitmapBytes, x, y);
                    }
                });

                threads[i].IsBackground = true;
            }

            creationTimer.Stop();

            // start threads
            for(int i = 0; i < processorCount; i++)
            {
                threads[i].Start();
            }

            // the main thread can do the leftovers
            for (int index = totalPixels - leftovers; index < totalPixels; index++)
            {
                int y = index / size;
                int x = index % size;

                writeBitmapPixel(bitmapBytes, x, y);
            }

            // wait for threads to finish
            for (int i = 0; i < processorCount; i++)
            {
                Console.WriteLine($"Calling thread.Join() to wait for thread {i}");
                threads[i].Join();
            }

            timer.Stop();

            TimeSpan justThreadsTime = timer.Elapsed;
            TimeSpan threadCreationTime = creationTimer.Elapsed;

            Console.WriteLine($"Just threads thread creation time: {threadCreationTime.TotalMilliseconds} MS");
            Console.WriteLine($"Just threads bitmap generation took: {(justThreadsTime - threadCreationTime).TotalMilliseconds} MS");
            Console.WriteLine($"Total Just threads bitmap generation took: {justThreadsTime.TotalMilliseconds} MS");
        }
    }
}
