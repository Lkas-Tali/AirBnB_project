using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace AirBnB
{
    /// <summary>
    /// Manages the display and interaction of property listings in an AirBnB-style application.
    /// Handles property card creation, image loading, and Firebase data synchronization.
    /// </summary>
    public partial class Saka
    {
        // Core dependencies and database connection
        private FirebaseClient firebaseClient;

        // Constants for UI layout and styling
        private const int CITY_CARD_WIDTH = 200;
        private const int CITY_CARD_HEIGHT = 200;
        private const int DETAIL_CARD_WIDTH = 350;
        private const int DETAIL_CARD_HEIGHT = 300;
        private const int FLICKER_INTERVAL = 200;

        // Fields for pagination
        private int currentPage = 0;
        private int itemsPerPage = 3;
        private Dictionary<string, string> allCities;
        private FlowLayoutPanel currentFlowPanel;

        // Colors used for the loading animation effect
        private readonly Color LoadingColorDark = Color.FromArgb(230, 230, 230);
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);

        // Thread-safe collections for managing concurrent operations
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();
        private readonly ConcurrentDictionary<string, Task<Image>> imageLoadingTasks = new ConcurrentDictionary<string, Task<Image>>();
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers =
            new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>();

        // Concurrency control mechanisms
        private readonly SemaphoreSlim imageSemaphore;
        private readonly int maxConcurrentDownloads;
        private readonly TaskFactory uiTaskFactory;
        private static readonly int MaxConcurrentTasks = Math.Max(2, Environment.ProcessorCount);

        // HTTP client configured for optimal performance and resource usage
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10),
            MaxResponseContentBufferSize = 1024 * 1024 * 50, // 50MB max to prevent memory issues
            DefaultRequestHeaders = { ConnectionClose = false } // Keep-alive for connection reuse
        };

        // Events for handling user interactions
        public event EventHandler<string> CitySelected;
        //public event EventHandler<Dictionary<string, object>> CitySelected;

        /// <summary>
        /// Initializes a new instance of the PropertyBookingManager with optimized threading and resource management.
        /// </summary>
        /// <param name="client">Firebase client for database operations</param>
        public Saka(FirebaseClient client)
        {
            firebaseClient = client;

            // Ensure UI updates happen on the correct thread
            uiTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());

            // Configure concurrent operations based on system capabilities
            maxConcurrentDownloads = Math.Min(Environment.ProcessorCount * 2, 32);
            imageSemaphore = new SemaphoreSlim(maxConcurrentDownloads);

            // Optimize thread pool settings for concurrent operations
            ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(
                Math.Max(workerThreads, maxConcurrentDownloads * 2),
                Math.Max(completionPortThreads, maxConcurrentDownloads * 2)
            );
        }

        public async Task<Dictionary<string, string>> FetchCityImages()
        {
            var citiesTask = firebaseClient
                .Child("cities images")
                .OnceSingleAsync<Dictionary<string, string>>();

            var citiesData = await citiesTask;

            var cityImageDictionary = new Dictionary<string, string>();

            // Only process the data if we received a valid response
            if (citiesData != null)
            {
                foreach (var cityData in citiesData)
                {
                    cityImageDictionary[cityData.Key] = cityData.Value;
                }
            }

            return cityImageDictionary;
        }

        public async Task DisplayAvailableCities(Dictionary<string, string> cities, FlowLayoutPanel flowPanel, string title = null)
        {
            try
            {
                // Store the cities and panel reference for pagination
                allCities = cities;
                currentFlowPanel = flowPanel;
                currentPage = 0;

                // Enable double buffering to prevent flickering during updates
                EnableDoubleBuffering(flowPanel);

                // Display the first page
                await DisplayCurrentPage();

                // Configure the flow panel layout
                flowPanel.AutoScroll = true;
                flowPanel.WrapContents = true;
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.Padding = new Padding(10);

                // Add title if provided
                if (!string.IsNullOrEmpty(title))
                {
                    Label titleLabel = new Label
                    {
                        Text = title,
                        Font = new Font("Nirmala UI", 16, FontStyle.Bold),
                        ForeColor = Color.FromArgb(255, 56, 92),
                        AutoSize = true,
                        Margin = new Padding(10),
                        Dock = DockStyle.Top
                    };
                    flowPanel.Controls.Add(titleLabel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DisplayCurrentPage()
        {
            if (currentFlowPanel == null || allCities == null) return;

            // Calculate the start and end indices for the current page
            int startIndex = currentPage * itemsPerPage;
            var citiesToDisplay = allCities
                .Skip(startIndex)
                .Take(itemsPerPage)
                .ToDictionary(x => x.Key, x => x.Value);

            // Clear existing controls
            currentFlowPanel.SuspendLayout();
            currentFlowPanel.Controls.Clear();

            // Create cards for the current page
            var cityCards = citiesToDisplay.Select(cityPair => CreatePropertyCard(cityPair)).ToList();
            currentFlowPanel.Controls.AddRange(cityCards.ToArray());

            // Start loading data for all cards concurrently
            var loadingTasks = citiesToDisplay.Zip(cityCards, (cityPair, card) =>
                InitiateDataLoading(card, cityPair));

            await Task.WhenAll(loadingTasks);
            currentFlowPanel.ResumeLayout();
        }

        public bool CanGoNext()
        {
            if (allCities == null) return false;
            int totalPages = (allCities.Count + itemsPerPage - 1) / itemsPerPage;
            return currentPage < totalPages - 1;
        }

        public bool CanGoPrevious()
        {
            return currentPage > 0;
        }

        public async Task NextPage()
        {
            if (CanGoNext())
            {
                currentPage++;
                await DisplayCurrentPage();
            }
        }

        public async Task PreviousPage()
        {
            if (CanGoPrevious())
            {
                currentPage--;
                await DisplayCurrentPage();
            }
        }

        private Panel CreatePropertyCard(KeyValuePair<string, string> city, bool isLoading = false)
        {
            // Create main card panel with rounded corners
            var card = new Panel
            {
                Width = CITY_CARD_WIDTH,
                Height = CITY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = city
            };

            // Apply rounded corners using GraphicsPath
            int radius = 20;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            if (isLoading)
            {
                // Show loading indicator
                var loadingLabel = new Label
                {
                    Text = "Loading...",
                    Font = new Font("Nirmala UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 56, 92),
                    AutoSize = true,
                    Location = new Point(card.Width / 2 - 30, card.Height / 2 - 10)
                };
                card.Controls.Add(loadingLabel);
            }
            else
            {
                // Add property information and controls
                AddPropertyControls(card, city);
                StartLoadingEffect(card);

                // Set up click handlers for the card and all its controls
                card.Click += (s, e) => CardClick(card);
                foreach (Control control in card.Controls)
                {
                    if (control is PictureBox || control is Label)
                    {
                        control.Click += (s, e) => CardClick(card);
                        control.Cursor = Cursors.Hand;
                    }
                }
            }

            return card;
        }

        private void CardClick(Panel card)
        {
            if (card.Tag is KeyValuePair<string, string> city)
            {
                // Pass only the city name (Key)
                CitySelected?.Invoke(this, city.Key);
            }
        }

        private void AddPropertyControls(Panel card, KeyValuePair<string, string> city)
        {
            PictureBox cityImage = CreateCityImageBox();
            card.Controls.Add(cityImage);
            AddCityLabel(card, city);
        }

        private PictureBox CreateCityImageBox()
        {
            var cityImage = new PictureBox
            {
                Width = CITY_CARD_WIDTH - 20,
                Height = 150,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = LoadingColorLight,
                Cursor = Cursors.Hand
            };

            using (var picPath = new GraphicsPath())
            {
                int picRadius = 10;
                picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
                picPath.AddArc(cityImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
                picPath.AddArc(cityImage.Width - picRadius, cityImage.Height - picRadius, picRadius, picRadius, 0, 90);
                picPath.AddArc(0, cityImage.Height - picRadius, picRadius, picRadius, 90, 90);
                picPath.CloseFigure();
                cityImage.Region = new Region(picPath);
            }

            return cityImage;
        }

        private void AddCityLabel(Panel cityCard, KeyValuePair<string, string> city)
        {
            Label cityLabel = new Label
            {
                Text = city.Key,
                Font = new Font("Nirmala UI", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = CITY_CARD_WIDTH - 20,
                Height = 30,
                Location = new Point(10, CITY_CARD_HEIGHT - 35)
            };

            cityCard.Controls.Add(cityLabel);
        }

        private void StartLoadingEffect(Panel card)
        {
            var timer = new System.Windows.Forms.Timer
            {
                Interval = FLICKER_INTERVAL
            };

            // Alternate between light and dark colors to create loading animation
            bool isLight = true;
            timer.Tick += (s, e) =>
            {
                // Check if card has been disposed to prevent invalid operations
                if (card.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                // Ensure UI updates happen on the correct thread
                if (card.InvokeRequired)
                {
                    card.Invoke(new Action(() => UpdateLoadingEffect(card, isLight)));
                }
                else
                {
                    UpdateLoadingEffect(card, isLight);
                }
                isLight = !isLight;
            };

            // Store timer in concurrent dictionary for proper cleanup
            loadingTimers.TryAdd(card, timer);
            timer.Start();
        }

        private void UpdateLoadingEffect(Panel card, bool isLight)
        {
            // Update PictureBox background if image hasn't loaded yet
            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && pictureBox.Image == null)
            {
                pictureBox.BackColor = isLight ? LoadingColorLight : LoadingColorDark;
            }

            // Update loading label backgrounds
            foreach (Control control in card.Controls)
            {
                if (control is Label label && label.Text.StartsWith("Loading"))
                {
                    label.BackColor = isLight ? LoadingColorLight : LoadingColorDark;
                }
            }
        }

        private void StopLoadingEffect(Panel card)
        {
            // Remove and dispose of the loading timer
            if (loadingTimers.TryRemove(card, out var timer))
            {
                timer.Stop();
                timer.Dispose();
            }

            // Reset control backgrounds to normal state if card still exists
            if (!card.IsDisposed)
            {
                var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pictureBox != null)
                {
                    pictureBox.BackColor = Color.White;
                }

                foreach (Control control in card.Controls)
                {
                    if (control is Label)
                    {
                        control.BackColor = Color.White;
                    }
                }
            }
        }

        private async Task InitiateDataLoading(Panel card, KeyValuePair<string, string> city)
        {
            try
            {
                var loadingTasks = new List<Task>();

                var imageUrl = city.Value.ToString();
                loadingTasks.Add(LoadCityImageAsync(imageUrl, card));

                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating data loading: {ex.Message}");
            }
        }

        private async Task LoadCityImageAsync(string imageUrl, Panel cityCard)
        {
            try
            {
                // Check if image is already cached
                if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
                {
                    var pictureBox = cityCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        UpdateCityCardImage(cityCard, cachedImage);
                        StopLoadingEffect(cityCard);
                    }
                    return;
                }

                // Implement retry logic for resilient image loading
                int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries && !cityCard.IsDisposed)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            var imageBytes = await client.GetByteArrayAsync(imageUrl);

                            if (!cityCard.IsDisposed)
                            {
                                using (var ms = new MemoryStream(imageBytes))
                                {
                                    var image = Image.FromStream(ms);

                                    // Add to cache if not already present
                                    if (!imageCache.ContainsKey(imageUrl))
                                    {
                                        imageCache.TryAdd(imageUrl, new Bitmap(image));
                                    }

                                    // Update UI with loaded image
                                    var pictureBox = cityCard.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox != null)
                                    {
                                        UpdateCityCardImage(cityCard, imageCache[imageUrl]);
                                        StopLoadingEffect(cityCard);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {currentRetry + 1} failed: {ex.Message}");
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            // Implement exponential backoff
                            await Task.Delay(1000 * currentRetry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        private void UpdateCityCardImage(Panel cityCard, Image originalImage)
        {
            // Ensure UI updates happen on the correct thread
            if (cityCard.InvokeRequired)
            {
                cityCard.Invoke(new Action(() => UpdateCityCardImage(cityCard, originalImage)));
                return;
            }

            var pictureBox = cityCard.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && !pictureBox.IsDisposed)
            {
                pictureBox.Image = ResizeImage(originalImage, pictureBox.Width, pictureBox.Height);
                pictureBox.BackColor = Color.White;
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }
    }
}