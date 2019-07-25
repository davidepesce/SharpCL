using System;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// The base object for other OpenCL memory objects (Buffer, Image)
    /// </summary>
    public class MemoryObject : OpenCLObject
    {
        #region Properties
        /// <summary>
        /// The <see cref="Context"/> associated with this <see cref="MemoryObject"/>.
        /// </summary>
        public Context Context { get; protected set; }

        /// <summary>
        /// The memory object type.
        /// </summary>
        public MemoryObjectType MemoryObjectType { get; private set; }

        /// <summary>
        /// Allocation and usage information about the image memory object.
        /// </summary>
        public MemoryFlags MemoryFlags { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="MemoryObject"/> object. Don't use this constructor, use <see cref="SharpCL.Context"/> methods to create <see cref="Buffer"/>s and <see cref="Image"/>s.
        /// </summary>
        public MemoryObject(IntPtr handle, Context context, MemoryObjectType objectType, MemoryFlags memoryFlags)
        {
            Handle = handle;
            Context = context;
            MemoryObjectType = objectType;
            MemoryFlags = memoryFlags;
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
                clReleaseMemObject(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseMemObject(IntPtr memobj);
        #endregion
    }
}
