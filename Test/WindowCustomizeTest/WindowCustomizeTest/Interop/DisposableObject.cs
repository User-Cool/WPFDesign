using System;

namespace WindowCustomizeTest.Interop
{
    public class DisposableObject : IDisposable
    {
        private EventHandler _disposing;


        public DisposableObject() { }

        /// <summary>
        /// 析构函数由运行时自动调用，用来实现自动的内存释放。
        /// 此时内存管理将由运行时完全负责，这种情况下，托管内存将由运行时自动释放，但是非托管内存依旧无法释放，
        /// 因此需要我们“告诉”运行时如何释放非托管资源。
        /// </summary>
        ~DisposableObject()
        {
            // 此时由运行时进行内存管理，我们只需要通知运行时如何释放非托管资源就好。
            Dispose(false); 
        }


        /// <summary>
        /// 返回一个值，这个值指示是否已经释放完成。
        /// </summary>
        public bool IsDisposed { get; private set; }


        public event EventHandler Disposing
        {
            add { ThrowIfDisposed(); _disposing += value; }
            remove { _disposing -= value; }
        }

        /// <summary>
        /// 来自 IDisposable。实现手动释放。
        /// 
        /// 调用这个函数就是手动释放而不是让运行时负责释放了，因此我们需要释放托管和非托管的所有内存占用。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        protected void Dispose(bool disposingManaged)
        {
            if (IsDisposed) return;

            try
            {
                if (disposingManaged)
                {
                    _disposing?.Invoke(this, EventArgs.Empty);
                    _disposing = null;
                    DisposeManagedResources();
                }

                DisposeNativeResources();

            }
            finally
            {
                IsDisposed = true;
            }
        }

        #region 用来实现内存释放的 可重写 方法
        /// <summary>
        /// 子类实现该方法实现托管内存的释放。
        /// </summary>
        protected virtual void DisposeManagedResources() { }

        /// <summary>
        /// 子类实现该方法实现非托管内存的释放。
        /// </summary>
        /// <remarks>
        /// 如果存在非托管内存，必须要实现该方法。
        /// </remarks>
        protected virtual void DisposeNativeResources() { }
        #endregion
    }
}
