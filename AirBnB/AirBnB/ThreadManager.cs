using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AirBnB
{
    // ThreadManager class manages the creation, execution, and synchronization of worker threads
    public class ThreadManager : IDisposable
    {
        private readonly int maxThreads; // Maximum number of threads the manager can handle
        private readonly List<Thread> activeThreads; // List of active worker threads
        private readonly ConcurrentQueue<WorkItem> workQueue; // Queue to hold work items
        private readonly object threadLock = new object(); // Lock to ensure thread-safe access to activeThreads
        private volatile bool isDisposed; // Flag to indicate if the manager has been disposed
        private int currentThreadCount; // The current number of active threads
        private readonly AutoResetEvent workAvailable; // Event to signal worker threads when work is available

        // Represents a unit of work to be processed by a worker thread
        private class WorkItem
        {
            public ThreadStart Work { get; set; } // Delegate to the work to be executed
            public ManualResetEventSlim CompletedEvent { get; set; } // Event to signal when work is completed
        }

        // Constructor initializes the thread manager with a specified max number of threads
        public ThreadManager(int maxThreadCount)
        {
            maxThreads = maxThreadCount;
            activeThreads = new List<Thread>();
            workQueue = new ConcurrentQueue<WorkItem>();
            workAvailable = new AutoResetEvent(false);
            currentThreadCount = 0;

            // Create the worker threads
            for (int i = 0; i < maxThreads; i++)
            {
                CreateWorkerThread();
            }
        }

        // Creates a new worker thread that will continuously process work items from the queue
        private void CreateWorkerThread()
        {
            var thread =
                new Thread(() =>
                {
                    // Continuously check for work until disposed
                    while (!isDisposed)
                    {
                        workAvailable.WaitOne(); // Wait for a signal that work is available

                        if (isDisposed) break; // Check if the manager has been disposed

                        // Process any work items in the queue
                        while (workQueue.TryDequeue(out WorkItem workItem))
                        {
                            if (isDisposed) break; // Ensure the manager hasn't been disposed during work execution

                            try
                            {
                                workItem.Work(); // Execute the work
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Thread error: {ex.Message}"); // Handle exceptions
                            }
                            finally
                            {
                                workItem.CompletedEvent.Set(); // Signal that the work is completed
                            }
                        }
                    }
                })
                {
                    IsBackground = true // Mark the thread as a background thread to allow it to terminate when the app ends
                };

            // Add the created thread to the list of active threads
            lock (threadLock)
            {
                activeThreads.Add(thread);
                currentThreadCount++;
            }

            thread.Start(); // Start the thread
        }

        // Enqueues a work item to be processed and returns a dummy thread object
        public Thread GetThread(ThreadStart threadStart)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(ThreadManager)); // Ensure that the manager is not disposed

            var completedEvent = new ManualResetEventSlim(false); // Event to signal completion of work
            var workItem = new WorkItem
            {
                Work = threadStart, // Set the work to be executed
                CompletedEvent = completedEvent // Associate the completion event
            };

            workQueue.Enqueue(workItem); // Enqueue the work item
            workAvailable.Set(); // Signal a worker thread to start processing

            // Return a dummy thread just to maintain compatibility; actual work is handled by the worker threads
            return new Thread(() => { });
        }

        // Waits for all work items to be processed and for all worker threads to complete
        public void WaitForAllThreads()
        {
            // Keep checking the work queue until it is empty
            while (workQueue.TryPeek(out _))
            {
                Thread.Sleep(100); // Sleep for 100ms to avoid busy-waiting
            }

            // Wait for all active threads to finish
            lock (threadLock)
            {
                foreach (var thread in activeThreads)
                {
                    if (thread.IsAlive)
                    {
                        thread.Join(); // Wait for the thread to complete
                    }
                }
            }
        }

        // Disposes of the thread manager and cleans up resources
        public void Dispose()
        {
            if (isDisposed)
                return; // If already disposed, do nothing

            isDisposed = true; // Mark the manager as disposed

            // Signal all worker threads to wake up and check if they should terminate
            for (int i = 0; i < maxThreads; i++)
            {
                workAvailable.Set();
            }

            lock (threadLock)
            {
                // Wait for all active threads to finish
                foreach (var thread in activeThreads)
                {
                    try
                    {
                        if (thread.IsAlive)
                        {
                            thread.Join(100); // Wait for up to 100ms for each thread to finish
                        }
                    }
                    catch { }
                }

                activeThreads.Clear(); // Clear the list of active threads
            }

            workAvailable.Dispose(); // Dispose the event used to signal work availability

            // Dispose of any remaining work items
            while (workQueue.TryDequeue(out WorkItem workItem))
            {
                workItem.CompletedEvent.Dispose(); // Dispose of the completion event for each work item
            }
        }
    }
}
