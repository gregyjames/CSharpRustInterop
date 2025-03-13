using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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
    
    [DllImport("librust_test.dylib")]
    private static extern void write_to_memory(IntPtr ptr, int size);

    [DllImport("librust_test.dylib", CallingConvention = CallingConvention.Cdecl)]
    private static extern void read_structs(IntPtr ptr, int count);


    [DllImport("librust_test.dylib", CallingConvention = CallingConvention.Cdecl)]
    private static extern void read_class(IntPtr ptr);
    
    static unsafe void Main()
    {
        var size = 1024; // 1 KB memory-mapped file
        using var mmf = MemoryMappedFile.CreateFromFile("file.txt", FileMode.OpenOrCreate, null, size);
        using var accessor = mmf.CreateViewAccessor();
        
        byte* ptr = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

        try
        {
            Console.WriteLine("Passing pointer to Rust...");
            write_to_memory((IntPtr)ptr, size);
        }
        finally
        {
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
            read_class(ptr3);
        }
        finally
        {
            handle.Free(); // Release the handle after Rust is done
        }
    }
}