using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL platform.
    /// </summary>
    /// <remarks>
    /// A platform is a specific OpenCL implementation, for instance AMD APP, NVIDIA or Intel OpenCL.
    /// </remarks>
    public class Platform : OpenCLObject
    {
        #region Properties
        public string Profile { get; private set; }
        public string Version { get; private set; }
        public string Name { get; private set; }
        public string Vendor { get; private set; }
        public string Extensions { get; private set; }
        #endregion

        #region Private Enums
        private enum PlatformInfo : UInt32
        {
            CL_PLATFORM_PROFILE     = 0x0900,
            CL_PLATFORM_VERSION     = 0x0901,
            CL_PLATFORM_NAME        = 0x0902,
            CL_PLATFORM_VENDOR      = 0x0903,
            CL_PLATFORM_EXTENSIONS  = 0x0904,
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Get the list of all platforms available.
        /// </summary>
        /// <returns>The list of the available <see cref="Platform"/>s.</returns>
        public static List<Platform> GetPlatforms()
        {
            ErrorCode error = clGetPlatformIDs(0, null, out uint numberOfPlatforms);
            if ((error != ErrorCode.Success) || (numberOfPlatforms == 0))
                return null;

            IntPtr[] handles = new IntPtr[numberOfPlatforms];
            error = clGetPlatformIDs(numberOfPlatforms, handles, out _);
            if ((error != ErrorCode.Success) || (numberOfPlatforms == 0))
                return null;

            List<Platform> platforms = new List<Platform>((int)numberOfPlatforms);
            foreach (IntPtr handle in handles)
            {
                platforms.Add(new Platform(handle));
            }

            return platforms;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Platform"/> object. Don't use this constructor, use <see cref="GetPlatforms"/> method instead.
        /// </summary>
        /// <param name="handle"></param>
        public Platform(IntPtr handle)
        {
            Handle = handle;
            Extensions = GetStringInfo(PlatformInfo.CL_PLATFORM_EXTENSIONS);
            Name = GetStringInfo(PlatformInfo.CL_PLATFORM_NAME);
            Profile = GetStringInfo(PlatformInfo.CL_PLATFORM_PROFILE);
            Vendor = GetStringInfo(PlatformInfo.CL_PLATFORM_VENDOR);
            Version = GetStringInfo(PlatformInfo.CL_PLATFORM_VERSION);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the list of <see cref="Device"/>s available on this <see cref="Platform"/>.
        /// </summary>
        /// <param name="type">The type of device</param>
        /// <returns></returns>
        public List<Device> GetDevices(DeviceType type = DeviceType.All)
        {
            error = clGetDeviceIDs(Handle, type, 0, null, out uint numberOfDevices);
            if ((error != ErrorCode.Success) || (numberOfDevices == 0))
                return null;

            IntPtr[] handles = new IntPtr[numberOfDevices];
            error = clGetDeviceIDs(Handle, type, numberOfDevices, handles, out _);
            if ((error != ErrorCode.Success) || (numberOfDevices == 0))
                return null;

            List<Device> devices = new List<Device>((int)numberOfDevices);
            foreach (IntPtr handle in handles)
            {
                devices.Add(new Device(handle, this));
            }

            return devices;
        }

        /// <summary>
        /// Create a <see cref="Context"/> using all the <see cref="Device"/>s of this <see cref="Platform"/>.
        /// </summary>
        /// <param name="notifyFunction">The callback used to report errors during context creation and at runtime.</param>
        /// <param name="userData">Pointer to user data passed to the callback.</param>
        /// <returns></returns>
        public Context CreateContext(ContextNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            Context context = new Context(this, notifyFunction, userData);
            if (context.Error)
                return null;
            return context;
        }

        /// <summary>
        /// Create a <see cref="Context"/> using the <see cref="Device"/>s of the specified type in this <see cref="Platform"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="notifyFunction"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public Context CreateContext(DeviceType type, ContextNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            Context context = new Context(this, type, notifyFunction, userData);
            if (context.Error)
                return null;
            return context;
        }
        #endregion

        #region Private Methods
        private string GetStringInfo(PlatformInfo param)
        {
            error = clGetPlatformInfo(Handle, param, UIntPtr.Zero, IntPtr.Zero, out UIntPtr bufferSizeRet);
            if (error != ErrorCode.Success)
                return "Error: Can't read platform info!";
            byte[] buffer = new byte[bufferSizeRet.ToUInt64()];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            error = clGetPlatformInfo(Handle, param, new UIntPtr((UInt64)buffer.Length), gcHandle.AddrOfPinnedObject(), out _);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return "Error: Can't read platform info!";
            char[] chars = Encoding.ASCII.GetChars(buffer, 0, buffer.Length - 1);
            return new string(chars);
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetPlatformIDs(
            UInt32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] platforms,
            out UInt32 num_platforms
            );

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetDeviceIDs(
            IntPtr platform,
            DeviceType device_type,
            UInt32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] devices,
            out UInt32 num_device
            );

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetPlatformInfo(
            IntPtr platform,
            PlatformInfo param_name,
            UIntPtr param_value_size,
            IntPtr param_value,
            out UIntPtr param_value_size_ret
            );
        #endregion
    }
}