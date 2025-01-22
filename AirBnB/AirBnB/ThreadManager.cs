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
        private readonly ConcurrentQueue<WorkItem> workQueue;
        private readonly object threadLock = new object();
        private volatile bool isDisposed;
        private int currentThreadCount;
        private readonly AutoResetEvent workAvailable;

        private class WorkItem
        {
            public ThreadStart Work { get; set; }
            public ManualResetEventSlim CompletedEvent { get; set; }
        }

        public ThreadManager(int maxThreadCount)
        {
            maxThreads = maxThreadCount;
            activeThreads = new List<Thread>();
            workQueue = new ConcurrentQueue<WorkItem>();
            workAvailable = new AutoResetEvent(false);
            currentThreadCount = 0;

            // Create worker threads
            for (int i = 0; i < maxThreads; i++)
            {
                CreateWorkerThread();
            }
        }

        private void CreateWorkerThread()
        {
            var thread = 
                new Thread(() =>
                { 
                    while (!isDisposed)
                {
                        workAvailable.WaitOne();

                        if (isDisposed) break;

                        while (workQueue.TryDequeue(out WorkItem workItem))
                        {
                            if (isDisposed) break;

                            try
                            {
                                workItem.Work();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Thread error: {ex.Message}");
                            }
                            finally
                            {
                                workItem.CompletedEvent.Set();
                            }
                        }
                    }
                })
                {
                    IsBackground = true
                };

            lock (threadLock)
            {
                activeThreads.Add(thread);
                currentThreadCount++;
            }

            thread.Start();
        }

        public Thread GetThread(ThreadStart threadStart)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(ThreadManager));

            var completedEvent = new ManualResetEventSlim(false);
            var workItem = new WorkItem
            {
                Work = threadStart,
                CompletedEvent = completedEvent
            };

            workQueue.Enqueue(workItem);
            workAvailable.Set();

            // Return a dummy thread just to maintain compatibility
            // The actual work is handled by the worker threads
            return new Thread(() => { });
        }

        public void WaitForAllThreads()
        {
            while (workQueue.TryPeek(out _))
            {
                Thread.Sleep(100);
            }

            lock (threadLock)
            {
                foreach (var thread in activeThreads)
                {
                    if (thread.IsAlive)
                    {
                        thread.Join();
                    }
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            // Signal all waiting threads to wake up
            for (int i = 0; i < maxThreads; i++)
            {
                workAvailable.Set();
            }

            lock (threadLock)
            {
                foreach (var thread in activeThreads)
                {
                    try
                    {
                        if (thread.IsAlive)
                        {
                            thread.Join(100);
                        }
                    }
                    catch { }
                }

                activeThreads.Clear();
            }

            workAvailable.Dispose();

            // Clear any remaining work items
            while (workQueue.TryDequeue(out WorkItem workItem))
            {
                workItem.CompletedEvent.Dispose();
            }
        }
    }
}