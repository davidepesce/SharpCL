using System;

namespace SharpCL
{
    [Flags]
    public enum CommandQueueProperties : UInt64
    {
        None                        = 0,
        OutOfOrderExecModeEnable    = 1,
        ProfilingEnable             = 2
    }

    [Flags]
    public enum DeviceType : UInt64
    {
        Default     = 1,
        CPU         = 2,
        GPU         = 4,
        Accelerator = 8,
        All         = 0xFFFFFFFF
    }

    [Flags]
    public enum FloatingPointConfig : UInt64
    {
        Denorm                      = 1,
        InfNaN                      = 2,
        RoundToNearest              = 4,
        RoundToZero                 = 8,
        RoundToInf                  = 16,
        FMA                         = 32,
        SoftFloat                   = 64,
        CorrectlyRoundedDivideSqrt  = 128
    }

    public enum ErrorCode : Int32
    {
        Success                                 = 0,
        DeviceNotFound                          = -1,
        DeviceNotAvailable                      = -2,
        CompilerNotAvailable                    = -3,
        MemoryObjectAllocationFailure           = -4,
        OutOfResources                          = -5,
        OutOfHostMemory                         = -6,
        ProfilingInfoNotAvailable               = -7,
        MemoryCopyOverlap                       = -8,
        ImageFormatMismatch                     = -9,
        ImageFormatNotSupported                 = -10,
        BuildProgramFailure                     = -11,
        MapFailure                              = -12,
        MisalignedSubBufferOffset               = -13,
        ExecutionStatusErrorForEventsInWaitList = -14,
        CompileProgramFailure                   = -15,
        LinkerNotAvailable                      = -16,
        LinkProgramFailure                      = -17,
        DevicePartitionFailed                   = -18,
        KernelArgumentInfoNotAvailable          = -19,
        InvalidValue                            = -30,
        InvalidDeviceType                       = -31,
        InvalidPlatform                         = -32,
        InvalidDevice                           = -33,
        InvalidContext                          = -34,
        InvalidCommandQueueFlags                = -35,
        InvalidCommandQueue                     = -36,
        InvalidHostPointer                      = -37,
        InvalidMemoryObject                     = -38,
        InvalidImageFormatDescriptor            = -39,
        InvalidImageSize                        = -40,
        InvalidSampler                          = -41,
        InvalidBinary                           = -42,
        InvalidBuildOptions                     = -43,
        InvalidProgram                          = -44,
        InvalidProgramExecutable                = -45,
        InvalidKernelName                       = -46,
        InvalidKernelDefinition                 = -47,
        InvalidKernel                           = -48,
        InvalidArgumentIndex                    = -49,
        InvalidArgumentValue                    = -50,
        InvalidArgumentSize                     = -51,
        InvalidKernelArguments                  = -52,
        InvalidWorkDimension                    = -53,
        InvalidWorkGroupSize                    = -54,
        InvalidWorkItemSize                     = -55,
        InvalidGlobalOffset                     = -56,
        InvalidEventWaitList                    = -57,
        InvalidEvent                            = -58,
        InvalidOperation                        = -59,
        InvalidGLObject                         = -60,
        InvalidBufferSize                       = -61,
        InvalidMipLevel                         = -62,
        InvalidGlobalWorkSize                   = -63,
        InvalidProperty                         = -64,
        InvalidImageDescriptor                  = -65,
        InvalidCompilerOptions                  = -66,
        InvalidLinkerOptions                    = -67,
        InvalidDevicePartitionCount             = -68,
        InvalidPipeSize                         = -69,
        InvalidDeviceQueue                      = -70
    }

    [Flags]
    public enum ExecutionCapabilities : UInt32
    {
        Kernel          = 1,
        NativeKernel    = 2
    }

    public enum ExecutionStatus : UInt32
    {
        Complete    = 0,
        Running     = 1,
        Submitted   = 2,
        Queued      = 3
    }

    public enum ImageAddressing : UInt32
    {
        None            = 0x1130,
        ClampToEdge     = 0x1131,
        Clamp           = 0x1132,
        Repeat          = 0x1133,
        MirroredRepeat  = 0x1134
    }

    public enum ImageChannelOrder : UInt32
    {
        R           = 0x10B0,
        A           = 0x10B1,
        RG          = 0x10B2,
        RA          = 0x10B3,
        RGB         = 0x10B4,
        RGBA        = 0x10B5,
        BGRA        = 0x10B6,
        ARGB        = 0x10B7,
        Intensity   = 0x10B8,
        Luminance   = 0x10B9,
        Rx          = 0x10BA,
        RGx         = 0x10BB,
        RGBx        = 0x10BC
    }

    public enum ImageChannelType : UInt32
    {
        SNormInt8       = 0x10D0,
        SNormInt16      = 0x10D1,
        UNormInt8       = 0x10D2,
        UNormInt16      = 0x10D3,
        UNormShort565   = 0x10D4,
        UNormShort555   = 0x10D5,
        UNormInt101010  = 0x10D6,
        SignedInt8      = 0x10D7,
        SignedInt16     = 0x10D8,
        SignedInt32     = 0x10D9,
        UnsignedInt8    = 0x10DA,
        UnsignedInt16   = 0x10DB,
        UnsignedInt32   = 0x10DC,
        HalfFloat       = 0x10DD,
        Float           = 0x10DE,
    }

    public enum ImageFiltering : UInt32
    {
        Nearest = 0x1140,
        Linear  = 0x1141
    }

    [Flags]
    public enum LocalMemoryType : UInt32
    {
        Local   = 1,
        Global  = 2
    }

    [Flags]
    public enum MemoryCacheType : UInt32
    {
        None            = 0,
        ReadOnlyCache   = 1,
        ReadWriteCache  = 2
    }

    [Flags]
    public enum MemoryFlags : UInt64
    {
        None                = 0,
        ReadWrite           = 1,
        WriteOnly           = 2,
        ReadOnly            = 4,
        UseHostPointer      = 8,
        AllocateHostPointer = 16,
        CopyHostPointer     = 32,
        HostWriteOnly       = 128,
        HostReadOnly        = 256,
        HostNoAccess        = 512
    }

    public enum MemoryObjectType : UInt32
    {
        Buffer          = 0x10F0,
        Image1D         = 0x10F4,
        Image1DArray    = 0x10F5,
        Image1DBuffer   = 0x10F6,
        Image2D         = 0x10F1,
        Image3D         = 0x10F2,
        Image2DArray    = 0x10F3
    }
}