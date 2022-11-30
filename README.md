# ThreadingTutorial

Example Output:

```
Hello, World!
Threading Example 1:

NOTE: It is intended that you just read the code, and running the code
is more for an example, and so that you can set break points and see
what is happening in the code.

Press any key to continue.
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

Creating thread:
Thread t = CreateNewThread();
Calling PrintThreadID from Main thread
PrintThreadID();
Hello from Thread # 1
Starting Thread t
t.Start();
For the next tutorial press any key







Hello from Thread # 4
Threading Example 2:

Do not forget:

It is intended that you just read the code, and running the code
is more for an example, and so that you can set break points and see
what is happening in the code.

Press any key to continue.
When you are threading you must not write the same memory at the same
time as a different thread, and you cannot read memory that is being
written at the same time. This is incredibly important because it can
lead to super complicated bugs called race conditions.

When you have multiple threads trying to touch the same memory, there
is no way to know which thread will "win" and reach the memory first.
Essentially they "race" to the memory.

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

byte[] bitmapBytes = new byte[8192 * 8192 * 3];

Now lets create a function that writes each pixel in the bitmap:

public void writeBitmapPixel(byte[] bitmapBytes, int x, int y)
{
    int index = (y * 8192 + x) * 3;
    bitmapBytes[index] = (byte)((y / (float)8192) * 255);
    bitmapBytes[index + 1] = (byte)((x / (float)8192) * 255);
    bitmapBytes[index + 2] = 0;
}

We can now call this for each pixel.

for(int y = 0; y < 8192; y++)
{
    for (int x = 0; x < 8192; x++)
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
Creating the bitmap data
Timing how long the non-threaded bitmap generation takes
Non-threaded bitmap generation took: 312.986 MS
Timing how long the naive-threaded bitmap generation takes
Naive-threaded bitmap generation took: 70.6187 MS
Timing how long the better-threaded bitmap generation takes
Better-threaded bitmap generation took: 72.8171 MS
Timing how long the better-er-threaded bitmap generation takes
Better-er-threaded bitmap generation took: 53.6654 MS
Timing how long the just threads bitmap generation takes
Calling thread.Join() to wait for thread 0
Calling thread.Join() to wait for thread 1
Calling thread.Join() to wait for thread 2
Calling thread.Join() to wait for thread 3
Calling thread.Join() to wait for thread 4
Calling thread.Join() to wait for thread 5
Calling thread.Join() to wait for thread 6
Calling thread.Join() to wait for thread 7
Calling thread.Join() to wait for thread 8
Calling thread.Join() to wait for thread 9
Calling thread.Join() to wait for thread 10
Calling thread.Join() to wait for thread 11
Calling thread.Join() to wait for thread 12
Calling thread.Join() to wait for thread 13
Calling thread.Join() to wait for thread 14
Calling thread.Join() to wait for thread 15
Just threads thread creation time: 0.0398 MS
Just threads bitmap generation took: 50.5749 MS
Total Just threads bitmap generation took: 50.6147 MS
```
