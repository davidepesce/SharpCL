using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL command-queue
    /// </summary>
    /// <remarks>
    /// A command-queue contains all the commands to be executed on a specific device.
    /// </remarks>
    public class CommandQueue : OpenCLObject
    {
        #region Properties
        /// <summary>
        /// The <see cref="SharpCL.Context"/> object associated with this <see cref="CommandQueue"/>
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// The <see cref="SharpCL.Device"/> object associated with this <see cref="CommandQueue"/>
        /// </summary>
        public Device Device { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="CommandQueue"/> object. Don't use this constructor, use <see cref="Context.CreateCommandQueue(Device, CommandQueueProperties)"/> method instead.
        /// </summary>
        public CommandQueue(IntPtr handle, Context context, Device device)
        {
            Handle = handle;
            Context = context;
            Device = device;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enqueues a command to execute a <see cref="Kernel"/> on a device.
        /// </summary>
        /// <param name="kernel">The Kernel to be executed.</param>
        /// <param name="globalWorkSize">An array of unsigned values that describe the number of global work-items in work_dim dimensions that will execute the kernel function.</param>
        /// <param name="globalWorkOffset">
        /// Can be used to specify an array of work_dim unsigned values that describe the offset used to calculate the global ID of a work-item.
        /// If global_work_offset is NULL, the global IDs start at offset (0, 0, … 0).
        /// </param>
        /// <param name="localWorkSize">
        /// points to an array of work_dim unsigned values that describe the number of work-items that make up a work-group (also referred to as the size of the work-group)
        /// that will execute the kernel specified by kernel.
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular kernel execution instance.</returns>
        public Event EnqueueKernel(Kernel kernel, UInt64[] globalWorkSize, UInt64[] globalWorkOffset = null, UInt64[] localWorkSize = null, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            UIntPtr[] globalWorkSizePtr = new UIntPtr[globalWorkSize.Length];
            for (int i = 0; i < globalWorkSize.Length; i++)
                globalWorkSizePtr[i] = new UIntPtr(globalWorkSize[i]);

            UIntPtr[] globalWorkOffsetPtr = null;
            if (globalWorkOffset != null)
            {
                globalWorkOffsetPtr = new UIntPtr[globalWorkOffset.Length];
                for (int i = 0; i < globalWorkOffset.Length; i++)
                    globalWorkOffsetPtr[i] = new UIntPtr(globalWorkOffset[i]);
            }

            UIntPtr[] localWorkSizePtr = null;
            if (globalWorkOffset != null)
            {
                localWorkSizePtr = new UIntPtr[localWorkSize.Length];
                for (int i = 0; i < localWorkSize.Length; i++)
                    localWorkSizePtr[i] = new UIntPtr(localWorkSize[i]);
            }

            error = clEnqueueNDRangeKernel(Handle, kernel.Handle, (UInt32)globalWorkSize.Length, globalWorkOffsetPtr,
                globalWorkSizePtr, localWorkSizePtr, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to execute a <see cref="Kernel"/> on a device. The <see cref="Kernel"/> is executed using a single work-item.
        /// </summary>
        /// <param name="kernel">The Kernel to be executed.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular kernel execution instance.</returns>
        public Event EnqueueTask(Kernel kernel, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            error = clEnqueueTask(Handle, kernel.Handle, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueue a command to read from a <see cref="Buffer"/> object to host memory.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="buffer">The buffer to read.</param>
        /// <param name="destination">An array where data is to be read into.</param>
        /// <param name="blocking">
        /// Indicate if the read operation is blocking or nonblocking. If true, this method does not return until the buffer data has been read.
        /// If false, this method queues a non-blocking read command and returns. The contents of the destination array cannot be used until the read command has completed.
        /// The returned event can be used to query the execution status of the read command.
        /// </param>
        /// <param name="size">The number of element to read.</param>
        /// <param name="offset">The offset in the buffer object to read from.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular read command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueReadBuffer<DataType>(Buffer buffer, DataType[] destination, bool blocking = false, UInt64 size = 0, UInt64 offset = 0, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (size == 0)
                size = (UInt64)destination.Length;

            GCHandle gcHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            error = clEnqueueReadBuffer(Handle, buffer.Handle, blocking, new UIntPtr(offset * (UInt64)Marshal.SizeOf<DataType>()),
                new UIntPtr(size * (UInt64)Marshal.SizeOf<DataType>()), gcHandle.AddrOfPinnedObject(), eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueue a command to write to a <see cref="Buffer"/> object from host memory.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="source">An array where data is to be written from.</param>
        /// <param name="blocking">
        /// Indicate if the write operation is blocking or nonblocking. If true, this method copies the source data and enqueues the write operation in the command-queue.
        /// The source array can be reused by the application after the method returns. If false, this method will use source data to perform a nonblocking write. As the write is non-blocking,
        /// the method return immediately. The source array cannot be reused by the application immediately after the call returns. The method returns an event object which can be used
        /// to query the execution status of the write command.
        /// </param>
        /// <param name="size">The number of element to write.</param>
        /// <param name="offset">The offset in the buffer object to write to.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular write command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueWriteBuffer<DataType>(Buffer buffer, DataType[] source, bool blocking = false, UInt64 size = 0, UInt64 offset = 0, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (size == 0)
                size = (UInt64)source.Length;

            GCHandle gcHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            error = clEnqueueWriteBuffer(Handle, buffer.Handle, blocking, new UIntPtr(offset * (UInt64)Marshal.SizeOf<DataType>()),
                new UIntPtr(size * (UInt64)Marshal.SizeOf<DataType>()), gcHandle.AddrOfPinnedObject(), eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to copy a <see cref="Buffer"/> object to another one.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="source">The source buffer.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="size">The number of element to copy.</param>
        /// <param name="sourceOffset">The offset in the buffer object to read from.</param>
        /// <param name="destinationOffset">The offset in the buffer object to write to.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular copy command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueCopyBuffer<DataType>(Buffer source, Buffer destination, UInt64 size, UInt64 sourceOffset = 0, UInt64 destinationOffset = 0, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            error = clEnqueueCopyBuffer(Handle, source.Handle, destination.Handle, new UIntPtr((UInt32)sourceOffset), new UIntPtr((UInt32)destinationOffset),
                new UIntPtr(size * (UInt64)Marshal.SizeOf<DataType>()), eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueue a command to read from an <see cref="Image"/> object to host memory.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="image">The image to read.</param>
        /// <param name="destination">An array where data is to be read into.</param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="origin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="blocking">
        /// Indicate if the read operation is blocking or nonblocking. If true, this method does not return until the image data has been read.
        /// If false, this method queues a non-blocking read command and returns. The contents of the destination array cannot be used until the read command has completed.
        /// The returned event can be used to query the execution status of the read command.
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <param name="rowPitch">
        /// Is the length of each row in bytes. This value must be greater than or equal to the element size in bytes * width. If row_pitch (or input_row_pitch) is set to 0,
        /// the appropriate row pitch is calculated based on the size of each element in bytes multiplied by width.
        /// </param>
        /// <param name="slicePitch">
        /// Is the size in bytes of the 2D slice of the 3D region of a 3D image or each image of a 1D or 2D image array being read or written respectively. This must be 0 if image is
        /// a 1D or 2D image.  This value must be greater than or equal to row_pitch * height.  If slice_pitch (or input_slice_pitch) is set to 0, the appropriate slice pitch is calculated
        /// based on the row_pitch * height.
        /// </param>
        /// <returns>An event object that identifies this particular read command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueReadImage<DataType>(Image image, DataType[] destination, Size region = default, Offset origin = default, bool blocking = false, List<Event> eventsWaitList = null, UInt64 rowPitch = 0, UInt64 slicePitch = 0)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
            {
                region = image.Size;
                if (image.MemoryObjectType == MemoryObjectType.Image1DArray)
                    region.Height = image.ArraySize;
                else if (image.MemoryObjectType == MemoryObjectType.Image2DArray)
                    region.Depth = image.ArraySize;
            }
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] originArray = new UIntPtr[3] { new UIntPtr(origin.X), new UIntPtr(origin.Y), new UIntPtr(origin.Z) };

            GCHandle gcHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            error = clEnqueueReadImage(Handle, image.Handle, blocking, originArray, regionArray, new UIntPtr(rowPitch), new UIntPtr(slicePitch), gcHandle.AddrOfPinnedObject(), eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueue a command to write to an <see cref="Image"/> object from host memory.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="image">The image to write.</param>
        /// <param name="source">The source image.</param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="origin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="blocking">
        /// Indicate if the write operation is blocking or nonblocking. If true, this method copies the source data and enqueues the write operation in the command-queue.
        /// The source array can be reused by the application after the method returns. If false, this method will use source data to perform a nonblocking write. As the write is non-blocking,
        /// the method return immediately. The source array cannot be reused by the application immediately after the call returns. The method returns an event object which can be used
        /// to query the execution status of the write command.
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <param name="rowPitch">
        /// Is the length of each row in bytes. This value must be greater than or equal to the element size in bytes * width. If row_pitch (or input_row_pitch) is set to 0,
        /// the appropriate row pitch is calculated based on the size of each element in bytes multiplied by width.
        /// </param>
        /// <param name="slicePitch">
        /// Is the size in bytes of the 2D slice of the 3D region of a 3D image or each image of a 1D or 2D image array being read or written respectively. This must be 0 if image is
        /// a 1D or 2D image.  This value must be greater than or equal to row_pitch * height.  If slice_pitch (or input_slice_pitch) is set to 0, the appropriate slice pitch is calculated
        /// based on the row_pitch * height.
        /// </param>
        /// <returns>An event object that identifies this particular write command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueWriteImage<DataType>(Image image, DataType[] source, Size region = default, Offset origin = default, bool blocking = false, List<Event> eventsWaitList = null, UInt64 rowPitch = 0, UInt64 slicePitch = 0)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
            {
                region = image.Size;
                if (image.MemoryObjectType == MemoryObjectType.Image1DArray)
                    region.Height = image.ArraySize;
                else if (image.MemoryObjectType == MemoryObjectType.Image2DArray)
                    region.Depth = image.ArraySize;
            }
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] originArray = new UIntPtr[3] { new UIntPtr(origin.X), new UIntPtr(origin.Y), new UIntPtr(origin.Z) };

            GCHandle gcHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            error = clEnqueueWriteImage(Handle, image.Handle, blocking, originArray, regionArray, new UIntPtr(rowPitch), new UIntPtr(slicePitch), gcHandle.AddrOfPinnedObject(), eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to copy a <see cref="Image"/> object to another one.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="destination">The destination image.</param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="sourceOrigin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D source image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="destinationOrigin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D destination image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular copy command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueCopyImage(Image source, Image destination, Size region = default, Offset sourceOrigin = default, Offset destinationOrigin = default, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
            {
                region = source.Size;
                if (source.MemoryObjectType == MemoryObjectType.Image1DArray)
                    region.Height = source.ArraySize;
                else if (source.MemoryObjectType == MemoryObjectType.Image2DArray)
                    region.Depth = source.ArraySize;
            }
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] sourceOriginArray = new UIntPtr[3] { new UIntPtr(sourceOrigin.X), new UIntPtr(sourceOrigin.Y), new UIntPtr(sourceOrigin.Z) };
            UIntPtr[] destinationOriginArray = new UIntPtr[3] { new UIntPtr(destinationOrigin.X), new UIntPtr(destinationOrigin.Y), new UIntPtr(destinationOrigin.Z) };

            error = clEnqueueCopyImage(Handle, source.Handle, destination.Handle, sourceOriginArray, destinationOriginArray, regionArray, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to copy an <see cref="Image"/> object to a <see cref="Buffer"/> object. 
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="sourceOrigin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D source image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="destinationOffset">The offset in the buffer object to write to.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular copy command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueCopyImageToBuffer(Image source, Buffer destination, Size region = default, Offset sourceOrigin = default, UInt64 destinationOffset = 0, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
                region = source.Size;
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] sourceOriginArray = new UIntPtr[3] { new UIntPtr(sourceOrigin.X), new UIntPtr(sourceOrigin.Y), new UIntPtr(sourceOrigin.Z) };

            error = clEnqueueCopyImageToBuffer(Handle, source.Handle, destination.Handle, sourceOriginArray, regionArray, new UIntPtr(destinationOffset), eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to copy a <see cref="Buffer"/> object to an <see cref="Image"/> object. 
        /// </summary>
        /// <param name="source">The source buffer.</param>
        /// <param name="destination">The destination image.</param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="sourceOffset">The offset in the buffer object to read from.</param>
        /// <param name="destinationOrigin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D destination image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular copy command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueCopyBufferToImage(Buffer source, Image destination, Size region = default, UInt64 sourceOffset = 0, Offset destinationOrigin = default, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
                region = destination.Size;
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] destinationOriginArray = new UIntPtr[3] { new UIntPtr(destinationOrigin.X), new UIntPtr(destinationOrigin.Y), new UIntPtr(destinationOrigin.Z) };

            error = clEnqueueCopyBufferToImage(Handle, source.Handle, destination.Handle, new UIntPtr(sourceOffset), destinationOriginArray, regionArray, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to fill a <see cref="Buffer"/> object with a pattern.
        /// </summary>
        /// <typeparam name="DataType">The type of buffer data.</typeparam>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="pattern">Pattern used to fill the buffer. Pattern size in bytes must be 1, 2, 4, 8, 16, 32, 64 or 128.</param>
        /// <param name="offset">Is the location of the region being filled in buffer and must be a multiple of pattern size.</param>
        /// <param name="size">Is the size of region being filled in buffer and must be a multiple of pattern size.</param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular fill command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueFillBuffer<DataType>(Buffer buffer, DataType[] pattern, UInt64 size = 0, UInt64 offset = 0, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);
            UInt64 sizeOfType = (UInt64)Marshal.SizeOf<DataType>();
            if (size == 0) size = buffer.Length - (buffer.Length % (UInt64)pattern.Length);
            GCHandle gcHandle = GCHandle.Alloc(pattern, GCHandleType.Pinned);
            error = clEnqueueFillBuffer(Handle, buffer.Handle, gcHandle.AddrOfPinnedObject(), new UIntPtr((UInt64)pattern.Length * sizeOfType), new UIntPtr(offset * sizeOfType), new UIntPtr(size * sizeOfType), eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a command to fill an image object with a specified color.
        /// </summary>
        /// <typeparam name="DataType">The type of image data.</typeparam>
        /// <param name="image">The image to fill.</param>
        /// <param name="color">
        /// The fill color must be a ColorFloat struct if the image channel data type is not an unnormalized signed and unsigned integer type, a ColorInt struct if the
        /// image channel data type is an unnormalized signed integer type and a ColorUInt struct if the image channel data type is an unnormalized unsigned integer type.
        /// </param>
        /// <param name="region">
        /// Structure that defines the (Width, Height, Depth) in pixels of the 1D, 2D or 3D rectangle, the (Width, Height) in pixels of the 2D rectangle and the number of images of a 2D image array
        /// or the (Width) in pixels of the 1D rectangle and the number of images of a 1D image array. If image is a 2D image object, Depth must be 1. If image is a 1D image or
        /// 1D image buffer object, Height and Depth must be 1. If image is a 1D image array object, Depth must be 1. The values in region cannot be 0. Default = all image.
        /// </param>
        /// <param name="origin">
        /// Defines the (X, Y, Z) offset in pixels in the 1D, 2D or 3D image, the (X, Y) offset and the image index in the 2D image array or the (X) offset and the image index in the 1D image array.
        /// If image is a 2D image object, Z must be 0. If image is a 1D image or 1D image buffer object, Y and Z must be 0.  If image is a 1D image array object, Z must be 0.
        /// If image is a 1D image array object, Y describes the image index in the 1D image array. If image is a 2D image array object, Z describes the image index in the 2D image array. Default = (0,0,0).
        /// </param>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>An event object that identifies this particular fill command and can be used to query or queue a wait for this particular command to complete.</returns>
        public Event EnqueueFillImage<ColorType>(Image image, ColorType color, Size region = default, Offset origin = default, List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            if (region.IsNull)
            {
                region = image.Size;
                if (image.MemoryObjectType == MemoryObjectType.Image1DArray)
                    region.Height = image.ArraySize;
                else if (image.MemoryObjectType == MemoryObjectType.Image2DArray)
                    region.Depth = image.ArraySize;
            }
            UIntPtr[] regionArray = new UIntPtr[3] { new UIntPtr(region.Width), new UIntPtr(region.Height), new UIntPtr(region.Depth) };
            UIntPtr[] originArray = new UIntPtr[3] { new UIntPtr(origin.X), new UIntPtr(origin.Y), new UIntPtr(origin.Z) };

            GCHandle gcHandle = GCHandle.Alloc(color, GCHandleType.Pinned);
            error = clEnqueueFillImage(Handle, image.Handle, gcHandle.AddrOfPinnedObject(), originArray, regionArray, eventsCount, eventHandles, out IntPtr newEvent);
            gcHandle.Free();
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a marker command which waits for either a list of events to complete, or if the list is empty it waits for all commands previously enqueued in command queue to complete before it completes.
        /// </summary>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>
        /// An event which can be waited on, i.e. this event can be waited on to insure that all events either in the eventsWaitList or all previously enqueued commands,
        /// queued before this command to command queue, have completed.
        /// </returns>
        public Event EnqueueMarker(List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            error = clEnqueueMarkerWithWaitList(Handle, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Enqueues a barrier command which waits for either a list of events to complete, or if the list is empty it waits for all commands previously enqueued in command_queue to complete before it completes.
        /// This command blocks command execution, that is, any following commands enqueued after it do not execute until it completes.
        /// </summary>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns>
        /// An event which can be waited on, i.e. this event can be waited on to insure that all events either in the eventsWaitList or all previously enqueued commands,
        /// queued before this command to command queue, have completed.
        /// </returns>
        public Event EnqueueBarrier(List<Event> eventsWaitList = null)
        {
            IntPtr[] eventHandles = EventListToHandles(eventsWaitList, out UInt32 eventsCount);

            error = clEnqueueBarrierWithWaitList(Handle, eventsCount, eventHandles, out IntPtr newEvent);
            if (error != ErrorCode.Success)
                return null;

            return new Event(newEvent, this);
        }

        /// <summary>
        /// Issues all previously queued OpenCL commands in this commandqueue to the device associated. Flush only guarantees that all queued commands will eventually be submitted
        /// to the appropriate device. There is no guarantee that they will be complete after Flush returns.
        /// </summary>
        /// <returns>True if the command is successful, false otherwise.</returns>
        public bool Flush()
        {
            error = clFlush(Handle);
            return error == ErrorCode.Success;
        }

        /// <summary> 
        /// This method blocks until all previously queued OpenCL commands in command_queue are issued to the associated device and have completed. Finish does not return until
        /// all previously queued commands in command_queue have been processed and completed. Finish is also a synchronization point.
        /// </summary>
        /// <returns>True if the command is successful, false otherwise.</returns>
        public bool Finish()
        {
            error = clFinish(Handle);
            return error == ErrorCode.Success;
        }
        #endregion

        #region Private Methods
        private IntPtr[] EventListToHandles(List<Event> events, out UInt32 eventsCount)
        {
            IntPtr[] eventHandles = null;
            eventsCount = 0;
            if (events?.Count > 0)
            {
                eventsCount = (UInt32)events.Count;
                eventHandles = new IntPtr[eventsCount];
                for (int i = 0; i < eventsCount; i++)
                    eventHandles[i] = events[i].Handle;
            }
            return eventHandles;
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
                clReleaseCommandQueue(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseCommandQueue(IntPtr command_queue);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueNDRangeKernel(
            IntPtr command_queue,
            IntPtr kernel,
            UInt32 work_dim,
            [MarshalAs(UnmanagedType.LPArray)] UIntPtr[] global_work_offset,
            [MarshalAs(UnmanagedType.LPArray)] UIntPtr[] global_work_size,
            [MarshalAs(UnmanagedType.LPArray)] UIntPtr[] local_work_size,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueTask(
            IntPtr command_queue,
            IntPtr kernel,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueReadBuffer(
            IntPtr command_queue,
            IntPtr buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_read,
            UIntPtr offset,
            UIntPtr size,
            IntPtr ptr,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueWriteBuffer(
            IntPtr command_queue,
            IntPtr buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
            UIntPtr offset,
            UIntPtr size,
            IntPtr ptr,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueCopyBuffer(
            IntPtr command_queue,
            IntPtr src_buffer,
            IntPtr dst_buffer,
            UIntPtr src_offset,
            UIntPtr dst_offset,
            UIntPtr size,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueReadImage(
            IntPtr command_queue,
            IntPtr image,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_read,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UIntPtr row_pitch,
            UIntPtr slice_pitch,
            IntPtr ptr,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueWriteImage(
            IntPtr command_queue,
            IntPtr image,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UIntPtr row_pitch,
            UIntPtr slice_pitch,
            IntPtr ptr,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueCopyImage(
            IntPtr command_queue,
            IntPtr src_image,
            IntPtr dst_image,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] src_origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] dst_origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueCopyImageToBuffer(
            IntPtr command_queue,
            IntPtr src_image,
            IntPtr dst_buffer,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] src_origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UIntPtr dst_offset,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueCopyBufferToImage(
            IntPtr command_queue,
            IntPtr src_buffer,
            IntPtr dst_image,
            UIntPtr src_offset,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] dst_origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueFillBuffer(
            IntPtr command_queue,
            IntPtr buffer,
            IntPtr pattern,
            UIntPtr pattern_size,
            UIntPtr offset,
            UIntPtr size,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueFillImage(
            IntPtr command_queue,
            IntPtr image,
            IntPtr fill_color,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] origin,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] UIntPtr[] region,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clFlush(IntPtr command_queue);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clFinish(IntPtr command_queue);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueMarkerWithWaitList(
            IntPtr command_queue,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clEnqueueBarrierWithWaitList(
            IntPtr command_queue,
            UInt32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_wait_list,
            out IntPtr out_event);
        #endregion
    }
}
