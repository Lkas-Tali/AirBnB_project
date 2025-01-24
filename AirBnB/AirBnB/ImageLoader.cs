using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace AirBnB
{
    public class ImageLoader : IDisposable
    {
        private readonly ThreadManager threadManager;
        private readonly object imageLock = new object();
        private readonly ConcurrentDictionary<string, Image> imageCache;
        private volatile bool isDisposed;
        private readonly ConcurrentDictionary<string, ManualResetEventSlim> loadingEvents;
        private readonly SynchronizationContext synchronizationContext;

        // Constructor that initializes the ImageLoader with the specified maximum number of threads
        public ImageLoader(int maxThreads)
        {
            threadManager = new ThreadManager(maxThreads);
            imageCache = new ConcurrentDictionary<string, Image>();
            loadingEvents = new ConcurrentDictionary<string, ManualResetEventSlim>();
            synchronizationContext = SynchronizationContext.Current;
        }

        // Method to load an image asynchronously and display it in a PictureBox
        public void LoadImage(string imageUrl, PictureBox pictureBox, Panel card)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(ImageLoader));

            if (synchronizationContext == null)
                throw new InvalidOperationException("ImageLoader must be created on UI thread");

            // Try to get the image from cache first
            if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
            {
                SafeUpdateUI(pictureBox, cachedImage);
                return;
            }

            // If the image is already being loaded, wait for it
            var loadingEvent = loadingEvents.GetOrAdd(imageUrl, _ => new ManualResetEventSlim(false));
            if (loadingEvent.IsSet)
            {
                if (imageCache.TryGetValue(imageUrl, out cachedImage))
                {
                    SafeUpdateUI(pictureBox, cachedImage);
                }
                return;
            }

            // If the image is not in cache and not being loaded, start a new thread to load it
            threadManager.GetThread(() =>
            {
                try
                {
                    // Check if the card or PictureBox has been disposed
                    if (IsControlDisposed(card) || IsControlDisposed(pictureBox))
                        return;

                    using (var client = new HttpClient())
                    {
                        // Download the image data from the URL
                        var imageData = client.GetByteArrayAsync(imageUrl).Result;

                        lock (imageLock)
                        {
                            if (!imageCache.ContainsKey(imageUrl))
                            {
                                using (var ms = new MemoryStream(imageData))
                                {
                                    var originalImage = Image.FromStream(ms);
                                    Image optimizedImage = null;

                                    // Get the dimensions of the PictureBox on the UI thread
                                    int width = 0, height = 0;
                                    synchronizationContext.Send(_ =>
                                    {
                                        if (!IsControlDisposed(pictureBox))
                                        {
                                            width = pictureBox.Width;
                                            height = pictureBox.Height;
                                        }
                                    }, null);

                                    // Resize the image to fit the PictureBox dimensions
                                    if (width > 0 && height > 0)
                                    {
                                        optimizedImage = new Bitmap(width, height);
                                        using (var g = Graphics.FromImage(optimizedImage))
                                        {
                                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            g.DrawImage(originalImage, 0, 0, width, height);
                                        }
                                        imageCache.TryAdd(imageUrl, optimizedImage);
                                    }
                                }
                            }
                        }

                        // If the image is now in cache, update the PictureBox on the UI thread
                        if (imageCache.TryGetValue(imageUrl, out Image finalImage))
                        {
                            SafeUpdateUI(pictureBox, finalImage);
                        }
                    }
                }
                finally
                {
                    // Signal that the image loading is complete
                    loadingEvent.Set();
                }
            });
        }

        // Method to safely update the PictureBox on the UI thread
        private void SafeUpdateUI(PictureBox pictureBox, Image image)
        {
            if (synchronizationContext == null || IsControlDisposed(pictureBox))
                return;

            synchronizationContext.Post(_ =>
            {
                try
                {
                    if (!IsControlDisposed(pictureBox))
                    {
                        pictureBox.Image = image;
                        pictureBox.BackColor = Color.White;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Control might have been disposed between the check and the update
                }
            }, null);
        }

        // Method to check if a control has been disposed
        private bool IsControlDisposed(Control control)
        {
            if (control == null)
                return true;

            bool disposed = true;
            try
            {
                if (synchronizationContext != null)
                {
                    synchronizationContext.Send(_ =>
                    {
                        disposed = control.IsDisposed;
                    }, null);
                }
            }
            catch
            {
                // If we can't check, assume it's disposed
                disposed = true;
            }
            return disposed;
        }

        // Method to wait for all image loading threads to complete
        public void WaitForAllImages()
        {
            threadManager.WaitForAllThreads();
        }

        // Method to dispose the ImageLoader and clean up resources
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            threadManager.Dispose();

            foreach (var image in imageCache.Values)
            {
                image.Dispose();
            }
            imageCache.Clear();

            foreach (var evt in loadingEvents.Values)
            {
                evt.Dispose();
            }
            loadingEvents.Clear();
        }
    }
}