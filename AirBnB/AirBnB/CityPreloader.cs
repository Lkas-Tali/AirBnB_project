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

        public const int CITY_IMAGE_WIDTH = 230;
        public const int CITY_IMAGE_HEIGHT = 250;

        public event EventHandler<int> PageLoadComplete;
        public bool IsInitialized => isInitialized;

        public CityPreloader(FirebaseClient client, int citiesPerPage = 6)
        {
            firebaseClient = client;
            this.citiesPerPage = citiesPerPage;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Initialize()
        {
            if (isInitialized) return;

            cityImages = await firebaseClient
                .Child("cities images")
                .OnceSingleAsync<Dictionary<string, string>>();

            isInitialized = true;
        }

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
            PageLoadComplete?.Invoke(this, pageIndex);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }
    }
}