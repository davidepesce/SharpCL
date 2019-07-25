using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL context.
    /// </summary>
    /// <remarks>
    /// Contexts are used by the OpenCL runtime for managing objects such as command-queues, memory, program and kernel objects and for executing kernels on one or more devices specified in the context.
    /// </remarks>
    public class Context : OpenCLObject
    {
        #region Properties
        /// <summary>The list of all the devices in this context.</summary>
        public List<Device> Devices { get; private set; }
        #endregion

        #region Private Enums
        private enum ContextProperty : UInt32
        {
            Platform        = 0x1084,
            InteropUserSync = 0x1085
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates a <see cref="Context"/> using the first available <see cref="Platform"/> that has <see cref="Device"/>s of the specified type.
        /// </summary>
        /// <param name="type">The type of devices to use for this context.</param>
        /// <returns>A new OpenCL context</returns>
        public static Context AutomaticContext(DeviceType type = DeviceType.All)
        {
            List<Platform> platforms = Platform.GetPlatforms();
            if (platforms?.Count == 0)
                return null;

            List<Device> devices = null;
            foreach (Platform platform in platforms)
            {
                devices = platform.GetDevices(type);
                if (devices?.Count > 0)
                    break;
            }

            if (devices.Count > 0)
            {
                Context context = new Context(devices);
                if (context.Error)
                    return null;
                return context;
            }

            return null;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a <see cref="Context"/> using all the <see cref="Device"/>s of the specified <see cref="Platform"/>.
        /// </summary>
        /// <param name="platform">The platform to use.</param>
        /// <param name="notifyFunction">The callback used to report errors during context creation and at runtime.</param>
        /// <param name="userData">Pointer to user data passed to the callback.</param>
        public Context(Platform platform, ContextNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            IntPtr[] properties = new IntPtr[3];
            properties[0] = new IntPtr((int)ContextProperty.Platform);
            properties[1] = platform.Handle;
            properties[2] = IntPtr.Zero;

            Devices = platform?.GetDevices();
            if (Devices != null && Devices.Count > 0)
            {
                IntPtr[] deviceHandles = new IntPtr[Devices.Count];
                for(int i = 0; i < Devices.Count; i++)
                {
                    deviceHandles[i] = Devices[i].Handle;
                }

                Handle = clCreateContext(properties, Devices.Count, deviceHandles, notifyFunction, userData, out error);
            }
            else
            {
                error = ErrorCode.InvalidDevice;
            }
        }

        /// <summary>
        /// Creates a <see cref="Context"/> using a list of <see cref="Device"/>s.
        /// </summary>
        /// <param name="devices">The list of devices to use.</param>
        /// <param name="notifyFunction">The callback used to report errors during context creation and at runtime.</param>
        /// <param name="userData">Pointer to user data passed to the callback.</param>
        public Context(List<Device> devices, ContextNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            if (devices != null && devices.Count > 0)
            {
                Devices = devices;
                IntPtr[] properties = new IntPtr[3];
                properties[0] = new IntPtr((int)ContextProperty.Platform);
                properties[1] = devices[0].Platform.Handle;
                properties[2] = IntPtr.Zero;

                IntPtr[] deviceHandles = new IntPtr[devices.Count];
                for (int i = 0; i < devices.Count; i++)
                {
                    deviceHandles[i] = devices[i].Handle;
                }

                Handle = clCreateContext(properties, devices.Count, deviceHandles, notifyFunction, userData, out error);
            }
            else
            {
                error = ErrorCode.InvalidDevice;
            }
        }

        /// <summary>
        /// Creates a <see cref="Context"/> using the <see cref="Device"/>s of the selected type in the specified <see cref="Platform"/>.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="type"></param>
        /// <param name="notifyFunction"></param>
        /// <param name="userData"></param>
        public Context(Platform platform, DeviceType type, ContextNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            IntPtr[] properties = new IntPtr[3];
            properties[0] = new IntPtr((int)ContextProperty.Platform);
            properties[1] = platform.Handle;
            properties[2] = IntPtr.Zero;

            Handle = clCreateContextFromType(properties, type, notifyFunction, userData, out error);
            if (error == ErrorCode.Success)
                Devices = platform.GetDevices(type);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a <see cref="CommandQueue"/> on a specific <see cref="Device"/>.
        /// </summary>
        /// <param name="device">Is a device associated with this Context. If null, the first available Device will be used.</param>
        /// <param name="properties">A list of properties for the CommandQueue.</param>
        /// <returns>A new <see cref="CommandQueue"/>.</returns>
        public CommandQueue CreateCommandQueue(Device device = null, CommandQueueProperties properties = CommandQueueProperties.None)
        {
            if (device == null)
                device = Devices[0];
            IntPtr commandQueueHandle = clCreateCommandQueue(Handle, device.Handle, properties, out error);
            if (error != ErrorCode.Success)
                return null;

            return new CommandQueue(commandQueueHandle, this, device);
        }

        /// <summary>
        /// Builds the source code and creates all <see cref="Kernel"/>s.
        /// </summary>
        /// <param name="source">The source code of the OpenCL program.</param>
        /// <param name="buildOptions">The build options to be used for building the program executable.</param>
        /// <param name="notifyFunction">The callback method which will be called when the program executable has been built (successfully or unsuccessfully).</param>
        /// <param name="userData">User data passed to the callback.</param>
        /// <returns>A dictionary of <see cref="Kernel"/>s. The key is the kernel name.</returns>
        public Dictionary<string, Kernel> BuildAllKernels(string source, string buildOptions="", BuildProgramNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            IntPtr programHandle = clCreateProgramWithSource(Handle, 1, new string[] { source }, null, out error);
            if (error != ErrorCode.Success)
                return null;

            IntPtr[] deviceHandles = new IntPtr[Devices.Count];
            for (int i = 0; i < Devices.Count; i++)
            {
                deviceHandles[i] = Devices[i].Handle;
            }

            error = clBuildProgram(programHandle, (uint)Devices.Count, deviceHandles, buildOptions, notifyFunction, userData);
            if (error != ErrorCode.Success)
                return null;

            error = clCreateKernelsInProgram(programHandle, 0, null, out uint kernelsCount);
            if (error != ErrorCode.Success)
                return null;

            IntPtr[] kernelHandles = new IntPtr[kernelsCount];
            error = clCreateKernelsInProgram(programHandle, kernelsCount, kernelHandles, out _);
            if (error != ErrorCode.Success)
                return null;

            error = clReleaseProgram(programHandle);
            if (error != ErrorCode.Success)
                return null;

            var kernels = new Dictionary<string, Kernel>();
            for (int i = 0; i < kernelsCount; i++)
            {
                Kernel kernel = new Kernel(kernelHandles[i], this);
                kernels.Add(kernel.Name, kernel);
            }

            return kernels;
        }

        /// <summary>
        /// Builds the source code and creates the specified <see cref="Kernel"/>.
        /// </summary>
        /// <param name="source">The source code of the OpenCL program.</param>
        /// <param name="functionName">The name of the Kernel to create.</param>
        /// <param name="buildOptions">The build options to be used for building the program executable.</param>
        /// <param name="notifyFunction">The callback method which will be called when the program executable has been built (successfully or unsuccessfully).</param>
        /// <param name="userData">User data passed to the callback.</param>
        /// <returns>The <see cref="Kernel"/> associated to the function name.</returns>
        public Kernel BuildKernel(string source, string functionName, string buildOptions = "", BuildProgramNotifyFunction notifyFunction = null, IntPtr userData = default)
        {
            IntPtr programHandle = clCreateProgramWithSource(Handle, 1, new string[] { source }, null, out error);
            if (error != ErrorCode.Success)
                return null;

            IntPtr[] deviceHandles = new IntPtr[Devices.Count];
            for (int i = 0; i < Devices.Count; i++)
            {
                deviceHandles[i] = Devices[i].Handle;
            }

            error = clBuildProgram(programHandle, (uint)Devices.Count, deviceHandles, buildOptions, notifyFunction, userData);
            if (error != ErrorCode.Success)
                return null;

            IntPtr kernelHandle = clCreateKernel(programHandle, functionName, out error);
            if (error != ErrorCode.Success)
                return null;

            error = clReleaseProgram(programHandle);
            if (error != ErrorCode.Success)
                return null;

            return new Kernel(kernelHandle, this);
        }

        /// <summary>
        /// Creates a <see cref="Buffer"/> of the selected type and size.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="size">The number of items in the buffer.</param>
        /// <param name="flags">Is used to specify allocation and usage information such as the memory arena that should be used to allocate the buffer object and how it will be used.</param>
        /// <returns>A new buffer.</returns>
        public Buffer CreateBuffer<DataType>(UInt64 size, MemoryFlags flags = MemoryFlags.None)
        {
            IntPtr bufferHandle = clCreateBuffer(Handle, flags, new UIntPtr(size * (UInt64)Marshal.SizeOf<DataType>()), IntPtr.Zero, out error);
            if (error != ErrorCode.Success)
                return null;
            return new Buffer(bufferHandle, this, size, (UInt64)Marshal.SizeOf<DataType>(), MemoryObjectType.Buffer, flags);
        }

        /// <summary>
        /// Creates <see cref="Buffer"/> from an array of data.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="data">An array of data used to determine type and size of the buffer.</param>
        /// <param name="flags">Is used to specify allocation and usage information such as the memory arena that should be used to allocate the buffer object and how it will be used.</param>
        /// <returns>A new buffer.</returns>
        public Buffer CreateBuffer<DataType>(DataType[] data, MemoryFlags flags = MemoryFlags.None)
        {
            if(data == null)
            {
                error = ErrorCode.InvalidHostPointer;
                return null;
            }

            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr bufferHandle = clCreateBuffer(Handle, flags, new UIntPtr((UInt64)(data.Length * Marshal.SizeOf<DataType>())), gcHandle.AddrOfPinnedObject(), out error);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;
            return new Buffer(bufferHandle, this, (UInt64)data.Length, (UInt64)Marshal.SizeOf<DataType>(), MemoryObjectType.Buffer, flags);
        }

        /// <summary>
        /// Creates a monodimensional <see cref="Image"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1D<DataType>(DataType[] data, UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image1D,
                Width = new UIntPtr(width),
                Height = new UIntPtr(1),
                Depth = new UIntPtr(1),
                ImageArraySize = UIntPtr.Zero,
                ImageRowPitch = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels = 0,
                NumSamples = 0,
                Buffer = IntPtr.Zero
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates a monodimensional <see cref="Image"/>.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1D(UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage1D<dynamic>(null, width, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Creates an array of monodimensional <see cref="Image"/>s.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="arraySize">The number of images in the image array.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1DArray<DataType>(DataType[] data, UInt64 arraySize, UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image1DArray,
                Width = new UIntPtr(width),
                Height = new UIntPtr(1),
                Depth = new UIntPtr(1),
                ImageArraySize = new UIntPtr(arraySize),
                ImageRowPitch = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels = 0,
                NumSamples = 0,
                Buffer = IntPtr.Zero
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates an array of monodimensional <see cref="Image"/>s.
        /// </summary>
        /// <param name="arraySize">The number of images in the image array.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1DArray(UInt64 arraySize, UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage1DArray<dynamic>(null, arraySize, width, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Creates a monodimensional <see cref="Image"/> using the data from a buffer.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="buffer">The buffer from where the image pixels are taken.</param>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1DBuffer<DataType>(MemoryObject buffer, DataType[] data, UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image1DBuffer,
                Width = new UIntPtr(width),
                Height = new UIntPtr(1),
                Depth = new UIntPtr(1),
                ImageArraySize = UIntPtr.Zero,
                ImageRowPitch = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels = 0,
                NumSamples = 0,
                Buffer = buffer.Handle
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates a monodimensional <see cref="Image"/> using the data from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer from where the image pixels are taken.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage1DBuffer(MemoryObject buffer, UInt64 width, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage1DBuffer<dynamic>(buffer, null, width, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Creates a 2D <see cref="Image"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns></returns>
        public Image CreateImage2D<DataType>(DataType[] data, UInt64 width, UInt64 height, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image2D,
                Width           = new UIntPtr(width),
                Height          = new UIntPtr(height),
                Depth           = new UIntPtr(1),
                ImageArraySize  = UIntPtr.Zero,
                ImageRowPitch   = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels    = 0,
                NumSamples      = 0,
                Buffer          = IntPtr.Zero
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates a 2D <see cref="Image"/>.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns></returns>
        public Image CreateImage2D(UInt64 width, UInt64 height, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage2D<dynamic>(null, width, height, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Creates an array of 2D <see cref="Image"/>s.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="arraySize">The number of images in the image array.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage2DArray<DataType>(DataType[] data, UInt64 arraySize, UInt64 width, UInt64 height, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image2DArray,
                Width = new UIntPtr(width),
                Height = new UIntPtr(height),
                Depth = new UIntPtr(1),
                ImageArraySize = new UIntPtr(arraySize),
                ImageRowPitch = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels = 0,
                NumSamples = 0,
                Buffer = IntPtr.Zero
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates an array of 2D <see cref="Image"/>s.
        /// </summary>
        /// <param name="arraySize">The number of images in the image array.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage2DArray(UInt64 arraySize, UInt64 width, UInt64 height, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage2DArray<dynamic>(null, arraySize, width, height, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Creates a 3D <see cref="Image"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="depth">The depth of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage3D<DataType>(DataType[] data, UInt64 width, UInt64 height, UInt64 depth, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            ImageFormat format = new ImageFormat
            {
                ChannelOrder = channelOrder,
                ChannelType = channelType
            };
            ImageDescriptor descriptor = new ImageDescriptor
            {
                ImageType = MemoryObjectType.Image3D,
                Width = new UIntPtr(width),
                Height = new UIntPtr(height),
                Depth = new UIntPtr(depth),
                ImageArraySize = UIntPtr.Zero,
                ImageRowPitch = UIntPtr.Zero,
                ImageSlicePitch = UIntPtr.Zero,
                NumMipLevels = 0,
                NumSamples = 0,
                Buffer = IntPtr.Zero
            };
            return CreateImage<DataType>(data, memoryFlags, format, descriptor);
        }

        /// <summary>
        /// Creates a 3D <see cref="Image"/>.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="depth">The depth of the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="channelOrder">Specifies the number of channels and the channel layout.</param>
        /// <param name="channelType">Describes the size of the channel data type.</param>
        /// <returns>A new image.</returns>
        public Image CreateImage3D(UInt64 width, UInt64 height, UInt64 depth, MemoryFlags memoryFlags, ImageChannelOrder channelOrder, ImageChannelType channelType)
        {
            return CreateImage3D<dynamic>(null, width, height, depth, memoryFlags, channelOrder, channelType);
        }

        /// <summary>
        /// Return the list of the supported <see cref="Image"/> formats as a list of <see cref="ImageFormat"/>.
        /// </summary>
        /// <param name="type">The type of image.</param>
        /// <param name="flags">A bit-field that is used to specify allocation and usage information about the image.</param>
        /// <returns></returns>
        public List<ImageFormat> SupportedImageFormats(MemoryObjectType type, MemoryFlags flags)
        {
            error = clGetSupportedImageFormats(Handle, flags, type, 0, null, out UInt32 formatCount);
            if (error != ErrorCode.Success)
                return null;

            ImageFormat[] formats = new ImageFormat[formatCount];
            error = clGetSupportedImageFormats(Handle, flags, type, formatCount, formats, out _);
            if (error != ErrorCode.Success)
                return null;

            return new List<ImageFormat>(formats);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> with the properties contained in <see cref="ImageFormat"/> and <see cref="ImageDescriptor"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="data">An array of data to use with the image.</param>
        /// <param name="memoryFlags">Is used to specify allocation and usage information about the image memory object being created.</param>
        /// <param name="format">A structure that define the type and number of image channels.</param>
        /// <param name="descriptor">A structure that describes type and dimensions of the image to be allocated.</param>
        /// <returns></returns>
        public Image CreateImage<DataType>(DataType[] data, MemoryFlags memoryFlags, ImageFormat format, ImageDescriptor descriptor)
        {
            IntPtr imageHandle = IntPtr.Zero;
            if (data == null)
            {
                imageHandle = clCreateImage(Handle, memoryFlags, ref format, ref descriptor, IntPtr.Zero, out error);
            }
            else
            {
                GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                imageHandle = clCreateImage(Handle, memoryFlags, ref format, ref descriptor, gcHandle.AddrOfPinnedObject(), out error);
                gcHandle.Free();
            }
            if (error != ErrorCode.Success)
                return null;

            Size size = new Size
            {
                Width = descriptor.Width.ToUInt64(),
                Height = descriptor.Height.ToUInt64(),
                Depth = (UInt64)descriptor.Depth.ToUInt64()
            };
            return new Image(imageHandle, this, descriptor.ImageType, memoryFlags, size, descriptor.ImageArraySize.ToUInt64(), format.ChannelOrder, format.ChannelType);
        }

        /// <summary>
        /// Creates a <see cref="Sampler"/> object.
        /// </summary>
        /// <param name="addressingMode">Specifies how out-of-range image coordinates are handled when reading from an image.</param>
        /// <param name="filteringMode">Specifies the type of filter that must be applied when reading an image.</param>
        /// <param name="normalizedCoordinates">Determines if the image coordinates specified are normalized or not.</param>
        /// <returns>A new sampler.</returns>
        public Sampler CreateSampler(ImageAddressing addressingMode, ImageFiltering filteringMode, bool normalizedCoordinates)
        {
            IntPtr samplerHandle = clCreateSampler(Handle, normalizedCoordinates, addressingMode, filteringMode, out error);
            if (error != ErrorCode.Success)
                return null;

            return new Sampler(samplerHandle, this, addressingMode, filteringMode, normalizedCoordinates);
        }

        /// <summary>
        /// Creates a <see cref="UserEvent"/> object. User events allow applications to enqueue commands that wait on a user event to finish before the command is executed by the device.  
        /// </summary>
        /// <returns>A new UserEvent.</returns>
        public UserEvent CreateUserEvent()
        {
            IntPtr eventHandle = clCreateUserEvent(Handle, out error);
            if (error != ErrorCode.Success)
                return null;

            return new UserEvent(eventHandle);
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
                clReleaseContext(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateContext(
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] properties,
            Int32 num_devices,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] devices,
            ContextNotifyFunction pfn_notify,
            IntPtr user_data,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateContextFromType(
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] properties,
            DeviceType device_type,
            ContextNotifyFunction pfn_notify,
            IntPtr user_data,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseContext(IntPtr context);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateCommandQueue(
            IntPtr context,
            IntPtr device,
            CommandQueueProperties properties,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateProgramWithSource(
            IntPtr context,
            UInt32 count,
            String[] strings,
            [MarshalAs(UnmanagedType.LPArray)] UIntPtr[] lengths,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseProgram(IntPtr program);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clBuildProgram(
            IntPtr program,
            UInt32 num_devices,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] device_list,
            String options,
            BuildProgramNotifyFunction pfn_notify,
            IntPtr user_data);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateKernel(
            IntPtr program,
            String kernel_name,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clCreateKernelsInProgram(
            IntPtr program,
            UInt32 num_kernels,
            [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] kernels,
            out UInt32 num_kernels_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateBuffer(
            IntPtr context,
            MemoryFlags flags,
            UIntPtr size,
            IntPtr host_ptr,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateImage(
            IntPtr context,
            MemoryFlags flags,
            ref ImageFormat image_format,
            ref ImageDescriptor image_desc,
            IntPtr host_ptr,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetSupportedImageFormats(
            IntPtr context,
            MemoryFlags flags,
            MemoryObjectType image_type,
            UInt32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] ImageFormat[] image_formats,
            out UInt32 num_image_formats);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateUserEvent(
            IntPtr context,
            out ErrorCode errcode_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static IntPtr clCreateSampler(
            IntPtr context,
            [MarshalAs(UnmanagedType.Bool)] bool normalized_coords,
            ImageAddressing addressing_mode,
            ImageFiltering filter_mode,
            out ErrorCode errcode_ret);
        #endregion
    }

    /// <summary>
    /// A callback used to report errors during context creation and at runtime.
    /// </summary>
    /// <param name="errorInfo">Description of the error.</param>
    /// <param name="privateInfo">Pointer to binary data that is returned by the OpenCL implementation that can be used to log additional information helpful in debugging the error.</param>
    /// <param name="size">Size of the privateInfo data.</param>
    /// <param name="userData">User data passed to the callback.</param>
    public delegate void ContextNotifyFunction(String errorInfo, IntPtr privateInfo, IntPtr size, IntPtr userData);

    /// <summary>
    /// A callback method which will be called when the program executable has been built (successfully or unsuccessfully).
    /// </summary>
    /// <param name="programHandle">Handle of the OpenCL program object.</param>
    /// <param name="userData">User data passed to the callback.</param>
    public delegate void BuildProgramNotifyFunction(IntPtr programHandle, IntPtr userData);
}
