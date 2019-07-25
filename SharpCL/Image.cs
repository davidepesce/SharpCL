using System;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL image.
    /// </summary>
    public class Image : MemoryObject
    {
        #region Properties
        /// <summary>
        /// The size of this <see cref="Image"/>.
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// The number of images in the <see cref="Image"/> array.
        /// </summary>
        public UInt64 ArraySize { get; private set; }

        /// <summary>
        /// The number and order of image channels.
        /// </summary>
        public ImageChannelOrder ChannelOrder { get; private set; }

        /// <summary>
        /// The type of image channels.
        /// </summary>
        public ImageChannelType ChannelType { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Image"/> object. Don't use this constructor, use CreateImage* methods of <see cref="Context"/> object instead.
        /// </summary>
        public Image(IntPtr handle, Context context, MemoryObjectType objectType, MemoryFlags memoryFlags, Size size, UInt64 arraySize, ImageChannelOrder channelOrder, ImageChannelType channelType) :
            base(handle, context, objectType, memoryFlags)
        {
            Size = size;
            ArraySize = arraySize;
            ChannelOrder = channelOrder;
            ChannelType = channelType;
        }
        #endregion
    }
}
