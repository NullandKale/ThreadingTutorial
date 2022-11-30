using System;
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
                """
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

                byte[] bitmapBytes = new byte[4096 * 4096 * 3];

                Now lets create a function that writes each pixel in the bitmap:

                public void writeBitmapPixel(byte[] bitmapBytes, int x, int y)
                {
                    int index = (y * 4096 + x) * 3;
                    bitmapBytes[index] = (byte)((y / 4096.0) * 255);
                    bitmapBytes[index + 1] = (byte)((x / 4096.0) * 255);
                    bitmapBytes[index + 2] = 0;
                }

                We can now call this for each pixel. 

                for(int y = 0; y < 4096; y++)
                {
                    for (int x = 0; x < 4096; x++)
                    {
                        writeBitmapPixel(bitmapBytes, x, y);
                    }
                }

                The simplest (and kinda naive) way is to use parallel.for() instead
                of the for loops but this can be rather slow, because there is a 
                TON of overhead for each call to writeBitmapPixel. 

                We can be smarter about the nested for loops, and maybe save some speed.

                If you still want to use parallel.for() you can call parallel.for()
                for each core in your system, manually spreading out the load. 

                """);
        }

        public void writeBitmapPixel(byte[] bitmapBytes, int x, int y)
        {
            int index = (y * 4096 + x) * 3;
            bitmapBytes[index] = (byte)((y / 4096.0) * 255);
            bitmapBytes[index + 1] = (byte)((x / 4096.0) * 255);
            bitmapBytes[index + 2] = 0;
        }

        public void Main()
        {
            Console.WriteLine("Creating the bitmap data");
            byte[] bitmapBytes = new byte[4096 * 4096 * 3];

            Console.WriteLine("Timing how long the non-threaded bitmap generation takes");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            for(int y = 0; y < 4096; y++)
            {
                for (int x = 0; x < 4096; x++)
                {
                    writeBitmapPixel(bitmapBytes, x, y);
                }
            }

            timer.Stop();

            TimeSpan nonThreadedTime = timer.Elapsed;

            Console.WriteLine($"Non-threaded bitmap generation took: {nonThreadedTime.TotalMilliseconds} MS");

            Console.WriteLine("Timing how long the naive-threaded bitmap generation takes");

            timer.Start();


            // NOTE: this feels really dumb to do
            Parallel.For(0, 4096, (y) =>
            {
                Parallel.For(0, 4096, (x) =>
                {
                    writeBitmapPixel(bitmapBytes, x, y);
                });
            });

            timer.Stop();

            TimeSpan naiveThreadedTime = timer.Elapsed;

            Console.WriteLine($"Naive-threaded bitmap generation took: {naiveThreadedTime.TotalMilliseconds} MS");


            Console.WriteLine("Timing how long the better-threaded bitmap generation takes");

            timer.Start();

            // NOTE: each pixel is 3 bytes, so we do not multiply this by 3, unlike the 
            // bitmapBytes length.
            int totalPixels = 4096 * 4096;

            // NOTE: this is better
            Parallel.For(0, totalPixels , (index) =>
            {
                int y = index / 4096;
                int x = index % 4096;

                writeBitmapPixel(bitmapBytes, x, y);
            });

            timer.Stop();

            TimeSpan betterThreadedTime = timer.Elapsed;

            Console.WriteLine($"Better-threaded bitmap generation took: {betterThreadedTime.TotalMilliseconds} MS");

            Console.WriteLine("Timing how long the better-er-threaded bitmap generation takes");

            timer.Start();

            // NOTE: this is better

            int processorCount = Environment.ProcessorCount;
            int perCorePixelCount = totalPixels / processorCount;

            // because we split the load over many threads we MIGHT have leftovers due to the need to use integer division.
            // this is guarenteed 
            int leftovers = totalPixels - (perCorePixelCount * processorCount);

            Parallel.For(0, processorCount, (index) =>
            {
                int y = index / 4096;
                int x = index % 4096;

                writeBitmapPixel(bitmapBytes, x, y);
            });

            timer.Stop();

            TimeSpan bettererThreadedTime = timer.Elapsed;

            Console.WriteLine($"Better-er-threaded bitmap generation took: {bettererThreadedTime.TotalMilliseconds} MS");

        }
    }
}
