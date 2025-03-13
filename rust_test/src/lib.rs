#[unsafe(no_mangle)]
pub extern "C" fn write_to_memory(ptr: *mut u8, size: i32) {
    if ptr.is_null() {
        return;
    }

    let slice = unsafe { std::slice::from_raw_parts_mut(ptr, size as usize) };
    slice.fill(b' ');

    let message = b"Hello from Rust!";
    let len = message.len().min(size as usize);

    for i in 0..len {
        slice[i] = 0;
    }
    
    slice[..len].copy_from_slice(&message[..len]);

    println!("Rust wrote to memory-mapped file!");
}

#[repr(C)]
#[derive(Debug)]
pub struct MyStruct {
    id: i32,
    value: f32,
}

#[unsafe(no_mangle)]
pub extern "C" fn read_structs(ptr: *const MyStruct, count: i32) {
    if ptr.is_null() || count <= 0 {
        return;
    }

    // Convert raw pointer into a slice
    let structs = unsafe { std::slice::from_raw_parts(ptr, count as usize) };

    for s in structs {
        println!("Rust received: id = {}, value = {}", s.id, s.value);
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct MyClass {
    id: i32,
    value: f32,
}

#[unsafe(no_mangle)]
pub extern "C" fn read_class(ptr: *const MyClass) {
    if ptr.is_null() {
        println!("Received null pointer!");
        return;
    }

    // Dereference the pointer to read the class data
    let obj = unsafe { &*ptr };

    println!("Rust received: id = {}, value = {}", obj.id, obj.value);
}