using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL Event.
    /// </summary>
    public class Event : OpenCLObject
    {
        #region Properties
        /// <summary>
        /// The <see cref="CommandQueue"/> associated with this Event.
        /// </summary>
        public CommandQueue CommandQueue { get; private set; }

        /// <summary>
        /// The <see cref="Context"/> associated with this <see cref="Event"/>.
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// The <see cref="SharpCL.ExecutionStatus"/> of the command associated with this Event.
        /// </summary>
        public Int32 ExecutionStatus
        {
            get
            {
                Int32 executionStatus = 0;
                GCHandle gcHandle = GCHandle.Alloc(executionStatus, GCHandleType.Pinned);
                error = clGetEventInfo(Handle, EventInfo.ExecutionStatus, new UIntPtr((UInt64)Marshal.SizeOf<Int32>()), gcHandle.AddrOfPinnedObject(), out _);
                gcHandle.Free();
                if (error != ErrorCode.Success)
                    return (Int32)error;
                return executionStatus;
            }
        }
        #endregion

        #region Private Enums
        private enum EventInfo : UInt32
        {
            CommandQueue    = 0x11D0,
            CommandType     = 0x11D1,
            ReferenceCount  = 0x11D2,
            ExecutionStatus = 0x11D3,
            Context         = 0x11D4
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Waits on the host thread for commands identified by <see cref="Event"/> objects in event list to complete. A command is considered complete if its execution status
        /// is complete or a negative value (error). The events specified in eventsWaitList act as synchronization points.
        /// </summary>
        /// <param name="eventsWaitList">A list of events that need to complete before this particular command can be executed.</param>
        /// <returns></returns>
        public static bool WaitForEvents(List<Event> eventsWaitList)
        {
            if (eventsWaitList == null || eventsWaitList.Count == 0)
                return true;

            IntPtr[] eventHandles = new IntPtr[eventsWaitList.Count];
            for (int i = 0; i < eventsWaitList.Count; i++)
                eventHandles[i] = eventsWaitList[i].Handle;

            ErrorCode error = clWaitForEvents((UInt32)eventsWaitList.Count, eventHandles);
            return error == ErrorCode.Success;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="Event"/> object. Don't use this constructor, events will be created when adding a command to a <see cref="CommandQueue"/>.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="commandQueue"></param>
        public Event(IntPtr handle, CommandQueue commandQueue)
        {
            Handle = handle;
            CommandQueue = commandQueue;
            Context = commandQueue?.Context;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Registers a user callback function for a specific command execution status. The registered callback function will be called when the execution status of command
        /// associated with this <see cref="Event"/> changes to an execution status equal to or past the specified status.
        /// </summary>
        /// <param name="status">The status for which to call the callback function.</param>
        /// <param name="notifyFunction">The callback function.</param>
        /// <param name="userData">User data to pass to the callback.</param>
        /// <returns></returns>
        public bool SetEventCallback(ExecutionStatus status, EventNotifyFunction notifyFunction, IntPtr userData = default)
        {
            error = clSetEventCallback(Handle, status, notifyFunction, userData);
            return error == ErrorCode.Success;
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
                clReleaseEvent(Handle);
                Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clGetEventInfo(
            IntPtr _event,
            EventInfo param_name,
            UIntPtr param_value_size,
            IntPtr param_value,
            out UIntPtr param_value_size_ret);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clReleaseEvent(IntPtr _event);

        [DllImport("OpenCL.dll", ExactSpelling = true, EntryPoint = "clWaitForEvents")]
        private extern static ErrorCode clWaitForEvents(UInt32 num_events, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] event_list);

        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clSetEventCallback(
            IntPtr eventHandle,
            ExecutionStatus command_exec_callback_type,
            EventNotifyFunction pfn_notify,
            IntPtr user_data);
        #endregion
    }

    /// <summary>
    /// Callback function for <see cref="Event"/> execution status changes.
    /// </summary>
    /// <param name="eventHandle">Handle of the event.</param>
    /// <param name="eventCommandExecStatus">Status of the event.</param>
    /// <param name="userData">User data passed to the callback.</param>
    public delegate void EventNotifyFunction(IntPtr eventHandle, int eventCommandExecStatus, IntPtr userData);
}
