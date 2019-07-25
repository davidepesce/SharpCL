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

        public Size(UInt64 width, UInt64 height, UInt64 depth)
        {
            Width = width; Height = height; Depth = depth;
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

        public Offset(UInt64 x, UInt64 y, UInt64 z)
        {
            X = x; Y = y; Z = z;
        }
    }

    public struct ColorFloat
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ColorFloat(float r, float g, float b, float a = 1.0f)
        {
            R = r; G = g; B = b; A = a;
        }
    }

    public struct ColorInt
    {
        public Int32 R;
        public Int32 G;
        public Int32 B;
        public Int32 A;

        public ColorInt(Int32 r, Int32 g, Int32 b, Int32 a)
        {
            R = r; G = g; B = b; A = a;
        }
    }

    public struct ColorUInt
    {
        public UInt32 R;
        public UInt32 G;
        public UInt32 B;
        public UInt32 A;

        public ColorUInt(UInt32 r, UInt32 g, UInt32 b, UInt32 a)
        {
            R = r; G = g; B = b; A = a;
        }
    }
}
