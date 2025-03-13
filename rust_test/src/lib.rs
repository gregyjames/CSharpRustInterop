use std::ffi::{c_char, CString};

// Define a type for the logging function pointer
type LogCallback = extern "C" fn(*const c_char);

// Store the logging function globally (Unsafe, must be set before use)
static mut INFOLOGGER: Option<LogCallback> = None;
static mut ERRLOGGER: Option<LogCallback> = None;

// Function to set the logger from C#
#[unsafe(no_mangle)]
pub extern "C" fn set_info_logger(callback: LogCallback) {
    unsafe {
        INFOLOGGER = Some(callback);
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn set_err_logger(callback: LogCallback) {
    unsafe {
        ERRLOGGER = Some(callback);
    }
}

pub fn log_info_to_csharp(message: &str) {
    let c_message = CString::new(message).unwrap();
    
    unsafe {
        if let Some(logger) = INFOLOGGER {
            logger(c_message.as_ptr());
        }
    }
}

pub fn log_err_to_csharp(message: &str) {
    let c_message = CString::new(message).unwrap();
    
    unsafe {
        if let Some(logger) = ERRLOGGER {
            logger(c_message.as_ptr());
        }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn write_to_memory(ptr: *mut u8, size: i32) {
    if ptr.is_null() {
        log_err_to_csharp("Null ptr recieved from C#");
        return;
    }

    log_info_to_csharp("Getting slice to memory mapped file...");
    let slice = unsafe { std::slice::from_raw_parts_mut(ptr, size as usize) };
    slice.fill(b' ');

    let message = b"Hello from Rust!";
    let len = message.len().min(size as usize);

    for i in 0..len {
        slice[i] = 0;
    }
    
    log_info_to_csharp("Copying message to slice...");
    slice[..len].copy_from_slice(&message[..len]);

    log_info_to_csharp("Rust wrote to memory-mapped file!");
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
        log_err_to_csharp("Null ptr recieved from C#");
        return;
    }

    // Convert raw pointer into a slice
    let structs = unsafe { std::slice::from_raw_parts(ptr, count as usize) };

    for s in structs {
        log_info_to_csharp(&format!("Rust received: id = {}, value = {}", s.id, s.value))
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
        log_err_to_csharp("Null ptr recieved from C#");
        return;
    }

    // Dereference the pointer to read the class data
    let obj = unsafe { &*ptr };
    log_info_to_csharp(&format!("Rust received: id = {}, value = {} from the C# class.", obj.id, obj.value));
}