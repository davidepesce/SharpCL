using System;
using System.Runtime.InteropServices;

namespace SharpCL
{
    public struct ImageFormat
    {
        public ImageChannelOrder ChannelOrder;
        public ImageChannelType ChannelType;
    }

    public struct ImageDescriptor
    {
        public MemoryObjectType ImageType;
        public UIntPtr Width;
        public UIntPtr Height;
        public UIntPtr Depth;
        public UIntPtr ImageArraySize;
        public UIntPtr ImageRowPitch;
        public UIntPtr ImageSlicePitch;
        public UInt32 NumMipLevels;
        public UInt32 NumSamples;
        public IntPtr Buffer;
    }

    public struct Size
    {
        public UInt64 Width;
        public UInt64 Height;
        public UInt64 Depth;

        public static Size Null
        {
            get
            {
                return new Size
                {
                    Width = 0,
                    Height = 0,
                    Depth = 0
                };
            }
        }

        public bool IsNull
        {
            get
            {
                return (Width == 0) && (Height == 0) && (Depth == 0);
            }
        }
    }

    public struct Offset
    {
        public UInt64 X;
        public UInt64 Y;
        public UInt64 Z;

        public static Offset Zero
        {
            get
            {
                return new Offset
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                };
            }
        }
    }

}
