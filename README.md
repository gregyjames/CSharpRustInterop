![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/gregyjames/CSharpRustInterop/makefile.yml?style=for-the-badge&logo=githubactions&logoColor=white&label=Build)
![Rust](https://img.shields.io/badge/rust-%23000000.svg?style=for-the-badge&logo=rust&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
  

# CSharpRustInterop

Some examples I found useful calling Rust from CSharp, I'm sure some of these could be further optimized but this is just a starting ground. The ILogger is particularly useful for maintaining logging settings between Rust and C# code contexts. 

  

# Examples

 1. Writing to a memory mapped file by getting a `SafeMemoryMappedViewHandle` and passing its pointer to Rust.
 2. Passing a List of structs to Rust by using `CollectionMarshall` to read as a span and pinning the reference to the first element using `MemoryMarshal.GetReference`
 3. Passing a heap allocated class to Rust by using `GCHandle.Alloc(obj, GCHandleType.Pinned)` to acquire a handler to a fixed handler and using `AddrOfPinnedObject()` to retrieve the pointer to that object's memory location.
 4. Writing to a C# ILogger using delegate function stored as global function pointer in Rust

# Running
```sh
make full_run
```
# License
MIT License

Copyright (c) 2025 Greg James

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
