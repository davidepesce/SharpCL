using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL kernel.
    /// </summary>
    /// <remarks>
    /// Kernels are the OpenCL functions that run on OpenCL devices. 
    /// </remarks>
    public class Kernel : OpenCLObject
    {
        #region Properties
        /// <summary>
        /// The <see cref="Context"/> associated with this <see cref="Kernel"/>.
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// The name of this <see cref="Kernel"/> function.
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Private Enums
        private enum KernelInfo : UInt32
        {
            CL_KERNEL_FUNCTION_NAME     = 0x1190,
            CL_KERNEL_NUM_ARGS          = 0x1191,
            CL_KERNEL_REFERENCE_COUNT   = 0x1192,
            CL_KERNEL_CONTEXT           = 0x1193,
            CL_KERNEL_PROGRAM           = 0x1194,
            CL_KERNEL_ATTRIBUTES        = 0x1195
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Kernel"/> object. Don't use this constructor, use <see cref="Context"/> methods to create a <see cref="Kernel"/>. /> method instead.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="context"></param>
        public Kernel(IntPtr handle, Context context)
        {
            Handle = handle;
            Context = context;
            Name = GetStringInfo(KernelInfo.CL_KERNEL_FUNCTION_NAME);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set a value argument for <see cref="Kernel"/> execution.
        /// </summary>
        /// <typeparam name="ValueType">The type of the value.</typeparam>
        /// <param name="index">Is the argument index. Arguments to the kernel are referred by indices that go from 0 for the leftmost argument to n - 1, where n is the total number of arguments declared by a kernel. </param>
        /// <param name="data">The value to pass to the kernel.</param>
        /// <returns></returns>
        public bool SetArgument<ValueType>(UInt32 index, ValueType data) where ValueType : struct
        {
            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            error = clSetKernelArg(Handle, index, new UIntPtr((UInt64)Marshal.SizeOf<ValueType>()), gcHandle.AddrOfPinnedObject());
            gcHandle.Free();
            return error == ErrorCode.Success;
        }

        /// <summary>
        /// Set a <see cref="MemoryObject"/> argument (<see cref="Buffer"/> or <see cref="Image"/>) for <see cref="Kernel"/> execution.
        /// </summary>
        /// <param name="index">Is the argument index. Arguments to the kernel are referred by indices that go from 0 for the leftmost argument to n - 1, where n is the total number of arguments declared by a kernel. </param>
        /// <param name="memoryObject">The memory object to set as argument.</param>
        /// <returns></returns>
        public bool SetArgument(UInt32 index, MemoryObject memoryObject)
        {
            GCHandle gcHandle = GCHandle.Alloc(memoryObject.Handle, GCHandleType.Pinned);
            error = clSetKernelArg(Handle, index, new UIntPtr((UInt64)Marshal.SizeOf<IntPtr>()), gcHandle.AddrOfPinnedObject());
            gcHandle.Free();
            return error == ErrorCode.Success;
        }

        /// <summary>
        /// Set a <see cref="Sampler"/> argument for <see cref="Kernel"/> execution.
        /// </summary>
        /// <param name="index">Is the argument index. Arguments to the kernel are referred by indices that go from 0 for the leftmost argument to n - 1, where n is the total number of arguments declared by a kernel. </param>
        /// <param name="sampler">The sampler to set as argument.</param>
        /// <returns></returns>
        public bool SetArgument(UInt32 index, Sampler sampler)
        {
            GCHandle gcHandle = GCHandle.Alloc(sampler.Handle, GCHandleType.Pinned);
            error = clSetKernelArg(Handle, index, new UIntPtr((UInt64)Marshal.SizeOf<IntPtr>()), gcHandle.AddrOfPinnedObject());
            gcHandle.Free();
            return error == ErrorCode.Success;
        }

        /// <summary>
        /// Set an argument of <see cref="Kernel"/> as a local variable.
        /// </summary>
        /// <param name="index">Is the argument index. Arguments to the kernel are referred by indices that go from 0 for the leftmost argument to n - 1, where n is the total number of arguments declared by a kernel. </param>
        /// <param name="size">Size of the argument in bytes.</param>
        /// <returns></returns>
        public bool SetLocalArgument(UInt32 index, UInt64 size)
        {
            error = clSetKernelArg(Handle, index, new UIntPtr(size), IntPtr.Zero);
            return error == ErrorCode.Success;
        }
        #endregion

        #region Private Methods
        private string GetStringInfo(KernelInfo param)
        {
            error = clGetKernelInfo(Handle, param, UIntPtr.Zero, IntPtr.Zero, out UIntPtr bufferSizeRet);
            if (error != ErrorCode.Success)
                return "Error: Can't read Kernel Info!";
            byte[] buffer = new byte[bufferSizeRet.ToUInt64()];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            error = clGetKernelInfo(Handle, param, bufferSizeRet, gcHandle.AddrOfPinnedObject(), out _);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return "Error: Can't read Kernel Info!";
            char[] chars = Encoding.ASCII.GetChars(buffer, 0, buffer.Length - 1);
            return new string(chars);
        }
        #endregion

        #region OpenCL Release
        /// <summary>
        /// Release the memory allocated by the OpenCL library.
        /// </summary>
        protected override void Release()
        {
            if (Handle != IntPtr.Zero)
            {
                clReleaseKernel(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseKernel(IntPtr kernel);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetKernelInfo(
            IntPtr kernel,
            KernelInfo param_name,
            UIntPtr param_value_size,
            IntPtr param_value,
            out UIntPtr param_value_size_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clSetKernelArg(
            IntPtr kernel,
            UInt32 arg_index,
            UIntPtr arg_size,
            IntPtr arg_value);
        #endregion
    }
}
