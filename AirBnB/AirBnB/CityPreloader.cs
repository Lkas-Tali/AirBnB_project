using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;

namespace AirBnB
{
    public class CityPreloader
    {
        private readonly FirebaseClient firebaseClient;
        private readonly int citiesPerPage;
        private Dictionary<string, string> cityImages;
        private bool isInitialized;
        private CancellationTokenSource cancellationTokenSource;

        // Constants for city image dimensions
        public const int CITY_IMAGE_WIDTH = 230;
        public const int CITY_IMAGE_HEIGHT = 250;

        // Event raised when a page load is complete
        public event EventHandler<int> PageLoadComplete;

        // Property to check if the preloader is initialized
        public bool IsInitialized => isInitialized;

        public CityPreloader(FirebaseClient client, int citiesPerPage = 6)
        {
            firebaseClient = client;
            this.citiesPerPage = citiesPerPage;
            cancellationTokenSource = new CancellationTokenSource();
        }

        // Initialize the preloader by loading city images from Firebase
        public async Task Initialize()
        {
            if (isInitialized) return;

            cityImages = await firebaseClient
                .Child("cities images")
                .OnceSingleAsync<Dictionary<string, string>>();

            isInitialized = true;
        }

        // Start preloading city images for the specified cities
        public async Task StartPreloading(List<string> cities)
        {
            if (!isInitialized) await Initialize();

            int totalPages = (cities.Count + citiesPerPage - 1) / citiesPerPage;

            for (int page = 0; page < totalPages; page++)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                await PreloadPage(cities, page);
            }
        }

        // Preload a specific page of city images
        private async Task PreloadPage(List<string> cities, int pageIndex)
        {
            int startIndex = pageIndex * citiesPerPage;
            var citiesToLoad = cities
                .Skip(startIndex)
                .Take(citiesPerPage)
                .ToList();

            var loadingTasks = new List<Task>();

            foreach (string city in citiesToLoad)
            {
                if (cityImages.TryGetValue(city, out string imageUrl))
                {
                    loadingTasks.Add(SharedImageCache.Instance.GetOrLoadImageAsync(
                        imageUrl,
                        CITY_IMAGE_WIDTH,
                        CITY_IMAGE_HEIGHT
                    ));
                }
            }

            await Task.WhenAll(loadingTasks);

            // Raise the PageLoadComplete event with the loaded page index
            PageLoadComplete?.Invoke(this, pageIndex);
        }

        // Stop the preloading process
        public void Stop()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }
    }
}