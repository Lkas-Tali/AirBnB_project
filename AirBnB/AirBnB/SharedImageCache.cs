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
    public class SharedImageCache
    {
        private static readonly Lazy<SharedImageCache> instance =
            new Lazy<SharedImageCache>(() => new SharedImageCache());

        public static SharedImageCache Instance => instance.Value;

        private readonly ConcurrentDictionary<string, Image> imageCache;
        private readonly ConcurrentDictionary<string, Task<Image>> loadingTasks;
        private readonly SemaphoreSlim loadingSemaphore;
        private readonly HttpClient httpClient;

        private SharedImageCache()
        {
            imageCache = new ConcurrentDictionary<string, Image>();
            loadingTasks = new ConcurrentDictionary<string, Task<Image>>();
            loadingSemaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            httpClient = new HttpClient();
        }

        public async Task<Image> GetOrLoadImageAsync(string imageUrl, int width, int height)
        {
            // Try to get from cache first
            if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
            {
                return cachedImage;
            }

            // Get or create loading task
            // create an explicit Func<string, Task<Image>> to match the expected delegate type
            var loadingTask = loadingTasks.GetOrAdd(imageUrl, async (url) =>
            {
                await loadingSemaphore.WaitAsync();
                try
                {
                    // Double-check cache in case another task completed while waiting
                    if (imageCache.TryGetValue(url, out Image alreadyLoadedImage))
                    {
                        return alreadyLoadedImage;
                    }

                    var imageBytes = await httpClient.GetByteArrayAsync(url);
                    using (var ms = new MemoryStream(imageBytes))
                    using (var originalImage = Image.FromStream(ms))
                    {
                        var optimizedImage = new Bitmap(width, height);
                        using (var g = Graphics.FromImage(optimizedImage))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(originalImage, 0, 0, width, height);
                        }

                        imageCache.TryAdd(url, optimizedImage);
                        return optimizedImage;
                    }
                }
                finally
                {
                    loadingSemaphore.Release();
                    loadingTasks.TryRemove(url, out _);
                }
            });

            return await loadingTask;
        }

        public bool IsImageCached(string imageUrl)
        {
            return imageCache.ContainsKey(imageUrl);
        }

        public void Clear()
        {
            foreach (var image in imageCache.Values)
            {
                image.Dispose();
            }
            imageCache.Clear();
            loadingTasks.Clear();
        }
    }
}