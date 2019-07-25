using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL Device.
    /// </summary>
    /// <remarks>
    /// Computational unit which is accessed via OpenCL library.
    /// </remarks>
    public class Device : OpenCLObject
    {
        #region Properties
        public Platform Platform { get; private set; }
        public UInt32 AddressBits { get; private set; }
        public bool Available { get; private set; }
        public bool CompilerAvailable { get; private set; }
        public FloatingPointConfig DoubleFloatingPointConfig { get; private set; }
        public bool LittleEndian { get; private set; }
        public bool ErrorCorrectionSupport { get; private set; }
        public ExecutionCapabilities ExecutionCapabilities { get; private set; }
        public string Extensions { get; private set; }
        public UInt32 GlobalMemoryCachelineSize { get; private set; }
        public UInt64 GlobalMemoryCacheSize { get; private set; }
        public MemoryCacheType GlobalMemoryCacheType { get; private set; }
        public UInt64 GlobalMemorySize { get; private set; }
        public UInt64 Image2DMaxHeight { get; private set; }
        public UInt64 Image2DMaxWidth { get; private set; }
        public UInt64 Image3DMaxDepth { get; private set; }
        public UInt64 Image3DMaxHeight { get; private set; }
        public UInt64 Image3DMaxWidth { get; private set; }
        public bool ImageSupport { get; private set; }
        public UInt64 LocalMemorySize { get; private set; }
        public LocalMemoryType LocalMemoryType { get; private set; }
        public UInt32 MaxClockFrequency { get; private set; }
        public UInt32 MaxComputeUnits { get; private set; }
        public UInt32 MaxConstantArguments { get; private set; }
        public UInt64 MaxConstantBufferSize { get; private set; }
        public UInt64 MaxMemoryAllocationSize { get; private set; }
        public UInt64 MaxParameterSize { get; private set; }
        public UInt32 MaxReadImageArguments { get; private set; }
        public UInt32 MaxSamplers { get; private set; }
        public UInt64 MaxWorkGroupSize { get; private set; }
        public UInt32 MaxWorkItemDimensions { get; private set; }
        public UInt64[] MaxWorkItemSizes { get; private set; }
        public UInt32 MaxWriteImageArguments { get; private set; }
        public UInt32 MemoryBaseAddressAlignment { get; private set; }
        public UInt32 MinDataTypeAlignmentSize { get; private set; }
        public string Name { get; private set; }
        public UInt32 PreferredVectorWidthChar { get; private set; }
        public UInt32 PreferredVectorWidthDouble { get; private set; }
        public UInt32 PreferredVectorWidthFloat { get; private set; }
        public UInt32 PreferredVectorWidthHalf { get; private set; }
        public UInt32 PreferredVectorWidthInt { get; private set; }
        public UInt32 PreferredVectorWidthLong { get; private set; }
        public UInt32 PreferredVectorWidthShort { get; private set; }
        public string Profile { get; private set; }
        public UInt64 ProfilingTimerResolution { get; private set; }
        public CommandQueueProperties QueueProperties { get; private set; }
        public FloatingPointConfig SingleFloatingPointConfig { get; private set; }
        public DeviceType Type { get; private set; }
        public string Vendor { get; private set; }
        public UInt32 VendorID { get; private set; }
        public string Version { get; private set; }
        public string DriverVersion { get; private set; }
        public bool HostUnifiedMemory { get; private set; }
        public UInt32 NativeVectorWidthChar { get; private set; }
        public UInt32 NativeVectorWidthDouble { get; private set; }
        public UInt32 NativeVectorWidthFloat { get; private set; }
        public UInt32 NativeVectorWidthHalf { get; private set; }
        public UInt32 NativeVectorWidthInt { get; private set; }
        public UInt32 NativeVectorWidthLong { get; private set; }
        public UInt32 NativeVectorWidthShort { get; private set; }
        public string OpenCLCVersion { get; private set; }
        #endregion

        #region Private Enums
        private enum DeviceInfo : UInt32
        {
            CL_DEVICE_TYPE                          = 0x1000,
            CL_DEVICE_VENDOR_ID                     = 0x1001,
            CL_DEVICE_MAX_COMPUTE_UNITS             = 0x1002,
            CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS      = 0x1003,
            CL_DEVICE_MAX_WORK_GROUP_SIZE           = 0x1004,
            CL_DEVICE_MAX_WORK_ITEM_SIZES           = 0x1005,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_CHAR   = 0x1006,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_SHORT  = 0x1007,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_INT    = 0x1008,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_LONG   = 0x1009,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_FLOAT  = 0x100A,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_DOUBLE = 0x100B,
            CL_DEVICE_MAX_CLOCK_FREQUENCY           = 0x100C,
            CL_DEVICE_ADDRESS_BITS                  = 0x100D,
            CL_DEVICE_MAX_READ_IMAGE_ARGS           = 0x100E,
            CL_DEVICE_MAX_WRITE_IMAGE_ARGS          = 0x100F,
            CL_DEVICE_MAX_MEM_ALLOC_SIZE            = 0x1010,
            CL_DEVICE_IMAGE2D_MAX_WIDTH             = 0x1011,
            CL_DEVICE_IMAGE2D_MAX_HEIGHT            = 0x1012,
            CL_DEVICE_IMAGE3D_MAX_WIDTH             = 0x1013,
            CL_DEVICE_IMAGE3D_MAX_HEIGHT            = 0x1014,
            CL_DEVICE_IMAGE3D_MAX_DEPTH             = 0x1015,
            CL_DEVICE_IMAGE_SUPPORT                 = 0x1016,
            CL_DEVICE_MAX_PARAMETER_SIZE            = 0x1017,
            CL_DEVICE_MAX_SAMPLERS                  = 0x1018,
            CL_DEVICE_MEM_BASE_ADDR_ALIGN           = 0x1019,
            CL_DEVICE_MIN_DATA_TYPE_ALIGN_SIZE      = 0x101A,
            CL_DEVICE_SINGLE_FP_CONFIG              = 0x101B,
            CL_DEVICE_GLOBAL_MEM_CACHE_TYPE         = 0x101C,
            CL_DEVICE_GLOBAL_MEM_CACHELINE_SIZE     = 0x101D,
            CL_DEVICE_GLOBAL_MEM_CACHE_SIZE         = 0x101E,
            CL_DEVICE_GLOBAL_MEM_SIZE               = 0x101F,
            CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE      = 0x1020,
            CL_DEVICE_MAX_CONSTANT_ARGS             = 0x1021,
            CL_DEVICE_LOCAL_MEM_TYPE                = 0x1022,
            CL_DEVICE_LOCAL_MEM_SIZE                = 0x1023,
            CL_DEVICE_ERROR_CORRECTION_SUPPORT      = 0x1024,
            CL_DEVICE_PROFILING_TIMER_RESOLUTION    = 0x1025,
            CL_DEVICE_ENDIAN_LITTLE                 = 0x1026,
            CL_DEVICE_AVAILABLE                     = 0x1027,
            CL_DEVICE_COMPILER_AVAILABLE            = 0x1028,
            CL_DEVICE_EXECUTION_CAPABILITIES        = 0x1029,
            CL_DEVICE_QUEUE_PROPERTIES              = 0x102A,
            CL_DEVICE_NAME                          = 0x102B,
            CL_DEVICE_VENDOR                        = 0x102C,
            CL_DRIVER_VERSION                       = 0x102D,
            CL_DEVICE_PROFILE                       = 0x102E,
            CL_DEVICE_VERSION                       = 0x102F,
            CL_DEVICE_EXTENSIONS                    = 0x1030,
            CL_DEVICE_PLATFORM                      = 0x1031,
            CL_DEVICE_DOUBLE_FP_CONFIG              = 0x1032,
            //CL_DEVICE_HALF_FP_CONFIG                = 0x1033,
            CL_DEVICE_PREFERRED_VECTOR_WIDTH_HALF   = 0x1034,
            CL_DEVICE_HOST_UNIFIED_MEMORY           = 0x1035,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_CHAR      = 0x1036,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_SHORT     = 0x1037,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_INT       = 0x1038,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_LONG      = 0x1039,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_FLOAT     = 0x103A,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_DOUBLE    = 0x103B,
            CL_DEVICE_NATIVE_VECTOR_WIDTH_HALF      = 0x103C,
            CL_DEVICE_OPENCL_C_VERSION              = 0x103D
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Device"/> object. Don't use this constructor, use <see cref="Platform.GetDevices(DeviceType)"/> method instead.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="platform"></param>
        public Device(IntPtr handle, Platform platform)
        {
            Handle = handle;
            Platform = platform;
            AddressBits = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_ADDRESS_BITS);
            Available = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_AVAILABLE) != 0;
            CompilerAvailable = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_COMPILER_AVAILABLE) != 0;
            DoubleFloatingPointConfig = (FloatingPointConfig)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_DOUBLE_FP_CONFIG);
            LittleEndian = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_ENDIAN_LITTLE) != 0;
            ErrorCorrectionSupport = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_ERROR_CORRECTION_SUPPORT) != 0;
            ExecutionCapabilities = (ExecutionCapabilities)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_EXECUTION_CAPABILITIES);
            Extensions = GetStringInfo(DeviceInfo.CL_DEVICE_EXTENSIONS);
            GlobalMemoryCachelineSize = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_GLOBAL_MEM_CACHELINE_SIZE);
            GlobalMemoryCacheSize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_GLOBAL_MEM_CACHE_SIZE);
            GlobalMemoryCacheType = (MemoryCacheType)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_GLOBAL_MEM_CACHE_TYPE);
            GlobalMemorySize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_GLOBAL_MEM_SIZE);
            Image2DMaxHeight = 0;
            if (AddressBits == 32)
                Image2DMaxHeight = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE2D_MAX_HEIGHT);
            else if (AddressBits == 64)
                Image2DMaxHeight = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_IMAGE2D_MAX_HEIGHT);
            Image2DMaxWidth = 0;
            if (AddressBits == 32)
                Image2DMaxWidth = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE2D_MAX_WIDTH);
            else if (AddressBits == 64)
                Image2DMaxWidth = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_IMAGE2D_MAX_WIDTH);
            Image3DMaxDepth = 0;
            if (AddressBits == 32)
                Image3DMaxDepth = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_DEPTH);
            else if (AddressBits == 64)
                Image3DMaxDepth = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_DEPTH);
            Image3DMaxHeight = 0;
            if (AddressBits == 32)
                Image3DMaxHeight = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_HEIGHT);
            else if (AddressBits == 64)
                Image3DMaxHeight = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_HEIGHT);
            Image3DMaxWidth = 0;
            if (AddressBits == 32)
                Image3DMaxWidth = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_WIDTH);
            else if (AddressBits == 64)
                Image3DMaxWidth = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_IMAGE3D_MAX_WIDTH);
            ImageSupport = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_IMAGE_SUPPORT) != 0;
            LocalMemorySize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_LOCAL_MEM_SIZE);
            LocalMemoryType = (LocalMemoryType)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_LOCAL_MEM_TYPE);
            MaxClockFrequency = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_CLOCK_FREQUENCY);
            MaxComputeUnits = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_COMPUTE_UNITS);
            MaxConstantArguments = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_CONSTANT_ARGS);
            MaxConstantBufferSize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE);
            MaxMemoryAllocationSize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_MAX_MEM_ALLOC_SIZE);
            MaxParameterSize = 0;
            if (AddressBits == 32)
                MaxParameterSize = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_PARAMETER_SIZE);
            else if (AddressBits == 64)
                MaxParameterSize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_MAX_PARAMETER_SIZE);
            MaxReadImageArguments = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_READ_IMAGE_ARGS);
            MaxSamplers = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_SAMPLERS);
            MaxWorkGroupSize = 0;
            if (AddressBits == 32)
                MaxWorkGroupSize = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_WORK_GROUP_SIZE);
            else if (AddressBits == 64)
                MaxWorkGroupSize = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_MAX_WORK_GROUP_SIZE);
            MaxWorkItemDimensions = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS);
            MaxWorkItemSizes = new UInt64[0];
            if (AddressBits == 32)
            {
                UInt32[] sizes = GetArrayInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_WORK_ITEM_SIZES);
                MaxWorkItemSizes = new UInt64[MaxWorkItemDimensions];
                for (int i = 0; i < MaxWorkItemSizes.Length; i++)
                    MaxWorkItemSizes[i] = sizes[i];
            }
            else if (AddressBits == 64)
            {
                UInt64[] sizes = GetArrayInfo<UInt64>(DeviceInfo.CL_DEVICE_MAX_WORK_ITEM_SIZES);
                MaxWorkItemSizes = new UInt64[MaxWorkItemDimensions];
                for (int i = 0; i < MaxWorkItemSizes.Length; i++)
                    MaxWorkItemSizes[i] = sizes[i];
            }
            MaxWriteImageArguments = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MAX_WRITE_IMAGE_ARGS);
            MemoryBaseAddressAlignment = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MEM_BASE_ADDR_ALIGN);
            MinDataTypeAlignmentSize = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_MIN_DATA_TYPE_ALIGN_SIZE);
            Name = GetStringInfo(DeviceInfo.CL_DEVICE_NAME);
            PreferredVectorWidthChar = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_CHAR);
            PreferredVectorWidthDouble = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_DOUBLE);
            PreferredVectorWidthFloat = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_FLOAT);
            PreferredVectorWidthInt = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_INT);
            PreferredVectorWidthLong = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_LONG);
            PreferredVectorWidthShort = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_SHORT);
            PreferredVectorWidthHalf = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PREFERRED_VECTOR_WIDTH_HALF);
            Profile = GetStringInfo(DeviceInfo.CL_DEVICE_PROFILE);
            ProfilingTimerResolution = 0;
            if (AddressBits == 32)
                ProfilingTimerResolution = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_PROFILING_TIMER_RESOLUTION);
            else if (AddressBits == 64)
                ProfilingTimerResolution = GetInfo<UInt64>(DeviceInfo.CL_DEVICE_PROFILING_TIMER_RESOLUTION);
            QueueProperties = (CommandQueueProperties)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_QUEUE_PROPERTIES);
            SingleFloatingPointConfig = (FloatingPointConfig)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_SINGLE_FP_CONFIG);
            Type = (DeviceType)GetInfo<UInt32>(DeviceInfo.CL_DEVICE_TYPE);
            Vendor = GetStringInfo(DeviceInfo.CL_DEVICE_VENDOR);
            VendorID = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_VENDOR_ID);
            Version = GetStringInfo(DeviceInfo.CL_DEVICE_VERSION);
            DriverVersion = GetStringInfo(DeviceInfo.CL_DRIVER_VERSION);
            HostUnifiedMemory = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_HOST_UNIFIED_MEMORY) != 0;
            NativeVectorWidthChar = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_CHAR);
            NativeVectorWidthDouble = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_DOUBLE);
            NativeVectorWidthFloat = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_FLOAT);
            NativeVectorWidthInt = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_INT);
            NativeVectorWidthLong = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_LONG);
            NativeVectorWidthShort = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_SHORT);
            NativeVectorWidthHalf = GetInfo<UInt32>(DeviceInfo.CL_DEVICE_NATIVE_VECTOR_WIDTH_HALF);
            OpenCLCVersion = GetStringInfo(DeviceInfo.CL_DEVICE_OPENCL_C_VERSION);
        }
        #endregion

        #region Private Methods
        private InfoType GetInfo<InfoType>(DeviceInfo param) where InfoType : struct
        {
            InfoType value = new InfoType();
            GCHandle gcHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
            error = clGetDeviceInfo(Handle, param, UIntPtr.Zero, IntPtr.Zero, out UIntPtr bufferSize);
            if (error != ErrorCode.Success)
                return default;
            error = clGetDeviceInfo(Handle, param, bufferSize, gcHandle.AddrOfPinnedObject(), out _);
            value = (InfoType)gcHandle.Target;
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return default;
            return value;
        }

        private string GetStringInfo(DeviceInfo param)
        {
            error = clGetDeviceInfo(Handle, param, UIntPtr.Zero, IntPtr.Zero, out UIntPtr bufferSize);
            if (error != ErrorCode.Success)
                return "Error: Can't read device info!";
            byte[] buffer = new byte[bufferSize.ToUInt64()];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            error = clGetDeviceInfo(Handle, param, bufferSize, gcHandle.AddrOfPinnedObject(), out _);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return "Error: Can't read device info!";
            char[] chars = Encoding.ASCII.GetChars(buffer, 0, buffer.Length - 1);
            return new string(chars);
        }

        private InfoType[] GetArrayInfo<InfoType>(DeviceInfo param)
        {
            error = clGetDeviceInfo(Handle, param, UIntPtr.Zero, IntPtr.Zero, out UIntPtr bufferSize);
            if (error != ErrorCode.Success)
                return null;
            InfoType[] buffer = new InfoType[bufferSize.ToUInt64()];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            error = clGetDeviceInfo(Handle, param, bufferSize, gcHandle.AddrOfPinnedObject(), out _);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;
            return buffer;
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetDeviceInfo(
            IntPtr device,
            DeviceInfo param_name,
            UIntPtr param_value_size,
            IntPtr param_value,
            out UIntPtr param_value_size_ret
            );
        #endregion
    }
}