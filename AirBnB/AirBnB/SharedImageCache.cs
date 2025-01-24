using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

namespace AirBnB
{
    // Singleton class to manage a shared image cache across the application
    public class SharedImageCache
    {
        // Lazy initialization of the shared image cache instance
        private static readonly Lazy<SharedImageCache> instance =
            new Lazy<SharedImageCache>(() => new SharedImageCache());

        // Public access to the singleton instance
        public static SharedImageCache Instance => instance.Value;

        // Thread-safe cache for storing images by their URL
        private readonly ConcurrentDictionary<string, Image> imageCache;
        // Thread-safe cache for tracking ongoing image loading tasks
        private readonly ConcurrentDictionary<string, Task<Image>> loadingTasks;
        // Semaphore to limit concurrent image downloads
        private readonly SemaphoreSlim loadingSemaphore;
        // HTTP client to download images
        private readonly HttpClient httpClient;

        // Private constructor to prevent direct instantiation
        private SharedImageCache()
        {
            imageCache = new ConcurrentDictionary<string, Image>();
            loadingTasks = new ConcurrentDictionary<string, Task<Image>>();
            loadingSemaphore = new SemaphoreSlim(Environment.ProcessorCount * 2); // Limits concurrent downloads based on system's CPU cores
            httpClient = new HttpClient();
        }

        // Method to get an image from the cache or download and cache it
        public async Task<Image> GetOrLoadImageAsync(string imageUrl, int width, int height)
        {
            // Attempt to retrieve the image from cache
            if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
            {
                return cachedImage;
            }

            // If the image is not in cache, initiate a loading task
            var loadingTask = loadingTasks.GetOrAdd(imageUrl, async (url) =>
            {
                await loadingSemaphore.WaitAsync();  // Limit concurrent downloads
                try
                {
                    // Double-check the cache in case another task finished downloading while waiting
                    if (imageCache.TryGetValue(url, out Image alreadyLoadedImage))
                    {
                        return alreadyLoadedImage;
                    }

                    // Download image data
                    var imageBytes = await httpClient.GetByteArrayAsync(url);
                    using (var ms = new MemoryStream(imageBytes))
                    using (var originalImage = Image.FromStream(ms))
                    {
                        // Resize the image to the specified width and height
                        var optimizedImage = new Bitmap(width, height);
                        using (var g = Graphics.FromImage(optimizedImage))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(originalImage, 0, 0, width, height);  // Draw the image with high quality scaling
                        }

                        // Add the resized image to cache and return it
                        imageCache.TryAdd(url, optimizedImage);
                        return optimizedImage;
                    }
                }
                finally
                {
                    loadingSemaphore.Release(); // Release the semaphore
                    loadingTasks.TryRemove(url, out _); // Clean up the loading task from the dictionary
                }
            });

            // Return the result of the loading task
            return await loadingTask;
        }

        // Method to check if an image is already cached
        public bool IsImageCached(string imageUrl)
        {
            return imageCache.ContainsKey(imageUrl);
        }

        // Method to clear the image cache and dispose of the images
        public void Clear()
        {
            // Dispose of all images in the cache
            foreach (var image in imageCache.Values)
            {
                image.Dispose();
            }
            // Clear the image cache and the loading task cache
            imageCache.Clear();
            loadingTasks.Clear();
        }
    }
}
