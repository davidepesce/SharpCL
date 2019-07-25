using System;
using System.Runtime.InteropServices;

namespace SharpCL
{
    /// <summary>
    /// An OpenCL user event object.
    /// </summary>
    public class UserEvent : Event
    {
        #region Properties
        /// <summary>
        /// The current execution status of the user event.
        /// </summary>
        public new Int32 ExecutionStatus { get; private set; }

        /// <summary>
        /// Set this property to true to mark the user event as completed. Once the event is set as complete, it can't be reverted to uncomplete and an error code can't be set.
        /// </summary>
        public bool Completed
        {
            get
            {
                return ExecutionStatus == (Int32)SharpCL.ExecutionStatus.Complete;
            }

            set
            {
                if(!Completed && value && (error < 0))
                {
                    throw new Exception("OpenCL - UserEvent: Can't mark as completed, there's an error value already set.");
                }

                if(Completed && !value)
                {
                    throw new Exception("OpenCL - UserEvent: event already set as completed.");
                }
                
                if(!Completed && value && (error == 0))
                {
                    ErrorCode clerror = clSetUserEventStatus(Handle, (Int32)SharpCL.ExecutionStatus.Complete);
                    if (clerror != SharpCL.ErrorCode.Success)
                        throw new Exception("OpenCL - UserEvent Set Complete: " + clerror);
                    ExecutionStatus = (Int32)SharpCL.ExecutionStatus.Complete;
                }
            }
        }

        /// <summary>
        ///  Set the error code for this user event. The event must not be set as completed.
        /// </summary>
        public ErrorCode ErrorCode
        {
            get
            {
                return error;
            }

            set
            {
                if(Completed && (error < 0))
                {
                    throw new Exception("OpenCL - UserEvent: can't set error value for completed UserEvent.");
                }

                if((error < 0) && (error != value))
                {
                    throw new Exception("OpenCL - UserEvent: error value already set.");
                }

                if(!Completed && (error == 0) && (value < 0))
                {
                    ErrorCode clerror = clSetUserEventStatus(Handle, (Int32)value);
                    if (clerror != SharpCL.ErrorCode.Success)
                        throw new Exception("OpenCL - UserEvent Set Error: " + clerror);
                    error = (ErrorCode)value;
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the <see cref="UserEvent"/> object. Don't use this constructor, use <see cref="Context.CreateUserEvent"/> method instead.
        /// </summary>
        public UserEvent(IntPtr handle) : base(handle, null) {
            ExecutionStatus = (Int32)SharpCL.ExecutionStatus.Submitted;
        }
        #endregion

        #region Dll Imports
        [DllImport("OpenCL.dll", ExactSpelling = true)]
        private extern static ErrorCode clSetUserEventStatus(
            IntPtr user_event,
            Int32 execution_status);
        #endregion
    }
}
