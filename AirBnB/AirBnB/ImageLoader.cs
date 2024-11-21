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

        public ImageLoader(int maxThreads)
        {
            threadManager = new ThreadManager(maxThreads);
            imageCache = new ConcurrentDictionary<string, Image>();
        }

        public void LoadImage(string imageUrl, PictureBox pictureBox, Panel card)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(ImageLoader));

            // Try to get from cache first
            if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
            {
                if (!pictureBox.IsDisposed)
                {
                    pictureBox.Invoke((MethodInvoker)delegate
                    {
                        pictureBox.Image = cachedImage;
                        pictureBox.BackColor = Color.White;
                    });
                }
                return;
            }

            // Create new thread for image loading
            threadManager.GetThread(() =>
            {
                try
                {
                    if (card.IsDisposed || pictureBox.IsDisposed)
                        return;

                    using (var client = new HttpClient())
                    {
                        var imageData = client.GetByteArrayAsync(imageUrl).Result;

                        lock (imageLock)
                        {
                            if (!imageCache.ContainsKey(imageUrl))
                            {
                                using (var ms = new MemoryStream(imageData))
                                {
                                    var originalImage = Image.FromStream(ms);
                                    var optimizedImage = new Bitmap(pictureBox.Width, pictureBox.Height);

                                    using (var g = Graphics.FromImage(optimizedImage))
                                    {
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        g.DrawImage(originalImage, 0, 0, optimizedImage.Width, optimizedImage.Height);
                                    }

                                    imageCache.TryAdd(imageUrl, optimizedImage);
                                }
                            }
                        }

                        if (!pictureBox.IsDisposed)
                        {
                            pictureBox.Invoke((MethodInvoker)delegate
                            {
                                if (!pictureBox.IsDisposed)
                                {
                                    pictureBox.Image = imageCache[imageUrl];
                                    pictureBox.BackColor = Color.White;
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            });
        }

        public void WaitForAllImages()
        {
            threadManager.WaitForAllThreads();
        }

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
        }
    }
}