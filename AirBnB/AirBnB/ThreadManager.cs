using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AirBnB
{
    public class ThreadManager : IDisposable
    {
        private readonly int maxThreads;
        private readonly List<Thread> activeThreads;
        private readonly ConcurrentQueue<Thread> availableThreads;
        private readonly object threadLock = new object();
        private readonly ManualResetEventSlim threadAvailableEvent;
        private volatile bool isDisposed;

        public ThreadManager(int maxThreadCount)
        {
            maxThreads = maxThreadCount;
            activeThreads = new List<Thread>();
            availableThreads = new ConcurrentQueue<Thread>();
            threadAvailableEvent = new ManualResetEventSlim(true);
        }

        public Thread GetThread(ThreadStart threadStart)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(ThreadManager));

            Thread thread = null;
            bool threadCreated = false;

            if (threadCreated)
            {
                thread.Start();
            }
            else
            {
                // Reuse existing thread
                ThreadPool.QueueUserWorkItem(_ => threadStart());
            }

            return thread;
        }

        private void ReleaseThread(Thread thread)
        {
            if (isDisposed)
                return;

            lock (threadLock)
            {
                activeThreads.Remove(thread);
                if (!isDisposed)
                {
                    availableThreads.Enqueue(thread);
                    threadAvailableEvent.Set();
                }
            }
        }

        public void WaitForAllThreads()
        {
            List<Thread> threadsToWaitFor;
            lock (threadLock)
            {
                threadsToWaitFor = new List<Thread>(activeThreads);
            }

            foreach (var thread in threadsToWaitFor)
            {
                thread.Join();
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            lock (threadLock)
            {
                foreach (var thread in activeThreads)
                {
                    try
                    {
                        thread.Join(10); // Give threads a chance to complete
                    }
                    catch { }
                }

                activeThreads.Clear();
                while (availableThreads.TryDequeue(out _)) { }
            }

            threadAvailableEvent.Dispose();
        }
    }
}