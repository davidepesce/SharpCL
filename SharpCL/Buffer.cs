using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL buffer object.
    /// </summary>
    public class Buffer : MemoryObject
    {
        #region Properties
        /// <summary>
        /// Length of the buffer (item count)
        /// </summary>
        public UInt64 Length { get; private set; }

        /// <summary>
        /// The size of an item in the buffer.
        /// </summary>
        public UInt64 ItemSize { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Buffer"/> object. Don't use this constructor, use <see cref="Context.CreateBuffer{DataType}(DataType[], MemoryFlags)"/> or <see cref="Context.CreateBuffer{DataType}(ulong, MemoryFlags)"/> instead.
        /// </summary>
        public Buffer(IntPtr handle, Context context, UInt64 length, UInt64 itemSize, MemoryObjectType objectType, MemoryFlags memoryFlags) : base(handle, context, objectType, memoryFlags)
        {
            Length = length;
            ItemSize = itemSize;
        }
        #endregion
    }
}
