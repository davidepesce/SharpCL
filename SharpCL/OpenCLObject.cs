using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCL
{
    /// <summary>
    /// The base object for every OpenCL objects that has an handle.
    /// </summary>
    public class OpenCLObject : IDisposable
    {
        #region Protected Fields
        /// <summary>
        /// Store the error code of the last operation.
        /// </summary>
        protected ErrorCode error;
        #endregion

        #region Properties
        /// <summary>
        /// The handle of this object. Used internally by SharpCL.
        /// </summary>
        public IntPtr Handle { get; protected set; }

        /// <summary>
        /// The error code of the last operation.
        /// </summary>
        public ErrorCode LastError
        {
            get
            {
                return error;
            }
        }

        /// <summary>
        /// This property indicates whether an error occurred in the last operation performed.
        /// </summary>
        public bool Error
        {
            get
            {
                return error != ErrorCode.Success;
            }
        }
        #endregion

        #region IDisposable Support
        /// <summary>
        /// Release the memory allocated by the OpenCL library. Overriden in each class that must release OpenCL resources.
        /// </summary>
        protected virtual void Release()
        {
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Finalizer of the class. Call <see cref="Release"/> to release the memory allocated by the OpenCL library. 
        /// </summary>
        ~OpenCLObject()
        {
            Release();
        }

        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> interface. Call <see cref="Release"/> to release the memory allocated by the OpenCL library.
        /// </summary>
        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
