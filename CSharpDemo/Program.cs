using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

class Program
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MyStruct
    {
        public int id;
        public float value;
    }
    
    
    [StructLayout(LayoutKind.Sequential)]
    public class MyClass
    {
        public int id;
        public float value;
    }
    
    private const string LIB_NAME = "librust_test.dylib";
    
    [DllImport(LIB_NAME)]
    private static extern void write_to_memory(IntPtr ptr, int size);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void read_structs(IntPtr ptr, int count);


    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void read_class(IntPtr ptr);
    
    // Delegate that matches Rust's function signature
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(IntPtr message);

    // Import Rust functions
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void set_info_logger(LogCallback callback);

    // Import Rust functions
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void set_err_logger(LogCallback callback);
    
    static unsafe void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Output logs to console
            .MinimumLevel.Debug() // Set minimum log level
            .CreateLogger();
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(); // Use Serilog
        });

        var logger = loggerFactory.CreateLogger<Program>();
        
        logger.LogInformation("[{ctx}]: Program starting...", "CSharp");
        
        // Define the C# function that will be called from Rust
        void LogInfoCallback(IntPtr messagePtr)
        {
            var message = Marshal.PtrToStringAnsi(messagePtr);
            logger.LogInformation("[{ctx}]: {msg}", "RUST", message);
        }
        
        void LogErrorCallback(IntPtr messagePtr)
        {
            var message = Marshal.PtrToStringAnsi(messagePtr);
            logger.LogError("[{ctx}]: {msg}", "RUST", message);
        }
        
        // Pass the delegate function pointer to Rust
        set_info_logger(LogInfoCallback);
        set_err_logger(LogErrorCallback);
        
        var size = 1024; // 1 KB memory-mapped file
        using var mmf = MemoryMappedFile.CreateFromFile("file.txt", FileMode.OpenOrCreate, null, size);
        using var accessor = mmf.CreateViewAccessor();
        
        byte* ptr = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

        try
        {
            logger.LogInformation("[{ctx}]: Passing pointer of ViewAccessor to Rust...", "CSharp");
            write_to_memory((IntPtr)ptr, size);
        }
        finally
        {
            logger.LogInformation("[{ctx}]: Releasing ViewAccessor pointer.", "CSharp");
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
        
        List<MyStruct> structs = new List<MyStruct>
        {
            new() { id = 1, value = 10.5f },
            new() { id = 2, value = 20.3f },
            new() { id = 3, value = 30.1f }
        };
        
        var span = CollectionsMarshal.AsSpan(structs);
        ref var searchSpace = ref MemoryMarshal.GetReference(span);

        logger.LogInformation("[{ctx}]: Passing pointer of first span element to Rust...", "CSharp");
        fixed (MyStruct* readPtr = &searchSpace)
        {
            read_structs((IntPtr)readPtr, structs.Count);
        }
        
        MyClass obj = new MyClass { id = 42, value = 99.5f };

        // Prevent GC from moving the object
        GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
        IntPtr ptr3 = handle.AddrOfPinnedObject(); // Get pointer to object memory
        
        try
        {
            logger.LogInformation("[{ctx}]: Passing pointer of Pinned Heap object to Rust...", "CSharp");
            read_class(ptr3);
        }
        finally
        {
            logger.LogInformation("[{ctx}]: Freed pinned handle.", "CSharp");
            handle.Free(); // Release the handle after Rust is done
        }
        
        logger.LogInformation("[{ctx}]: Program ending...", "CSharp");
    }
}