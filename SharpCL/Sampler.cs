using System;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL sampler.
    /// </summary>
    public class Sampler : OpenCLObject
    {
        #region Properties
        /// <summary>
        /// The <see cref="Context"/> associated with this <see cref="Sampler"/>.
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Specifies how out-of-range image coordinates are handled when reading from an image.
        /// </summary>
        public ImageAddressing AddressingMode { get; private set; }

        /// <summary>
        /// Specifies the type of filter that must be applied when reading an image.
        /// </summary>
        public ImageFiltering FilteringMode { get; private set; }

        /// <summary>
        /// Determines if the image coordinates specified are normalized or not.
        /// </summary>
        public bool NormalizedCoordinates { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Sampler"/> object. Don't use this constructor, use <see cref="Context.CreateSampler(ImageAddressing, ImageFiltering, bool)"/> method instead.
        /// </summary>
        public Sampler(IntPtr handle, Context context, ImageAddressing addr, ImageFiltering filt, bool norm)
        {
            Handle = handle;
            Context = context;
            AddressingMode = addr;
            FilteringMode = filt;
            NormalizedCoordinates = norm;
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
                clReleaseSampler(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseSampler(IntPtr sampler);
        #endregion
    }
}
