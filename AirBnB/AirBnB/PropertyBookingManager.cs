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
    public partial class PropertyBookingManager
    {
        // Core dependencies and database connection
        private FirebaseClient firebaseClient;
        private readonly CityCardManager cityCardManager;
        private readonly Saka saka;



        // Constants for UI layout and styling
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;
        private const int DETAIL_CARD_WIDTH = 350;
        private const int DETAIL_CARD_HEIGHT = 300;
        private const int FLICKER_INTERVAL = 200;

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
            MaxResponseContentBufferSize = 1024 * 1024 * 10, // 10MB max to prevent memory issues
            DefaultRequestHeaders = { ConnectionClose = false } // Keep-alive for connection reuse
        };

        // Events for handling user interactions
        public event EventHandler<string> CitySelected;
        public event EventHandler<Dictionary<string, object>> PropertySelected;
        public CityCardManager CityCardManager => cityCardManager;

        /// <summary>
        /// Initializes a new instance of the PropertyBookingManager with optimized threading and resource management.
        /// </summary>
        /// <param name="client">Firebase client for database operations</param>
        public PropertyBookingManager(FirebaseClient client)
        {
            firebaseClient = client;
            cityCardManager = new CityCardManager(client);
            saka = new Saka(client);

            // Ensure UI updates happen on the correct thread
            uiTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());

            // Configure concurrent operations based on system capabilities
            maxConcurrentDownloads = Math.Min(Environment.ProcessorCount * 2, 32);
            imageSemaphore = new SemaphoreSlim(maxConcurrentDownloads);

            // Forward city selection events
            cityCardManager.CitySelected += (sender, city) => {
                CitySelected?.Invoke(this, city);
            };

            saka.CitySelected += (sender, city) => {
                CitySelected?.Invoke(this, city);
            };

            // Optimize thread pool settings for concurrent operations
            ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(
                Math.Max(workerThreads, maxConcurrentDownloads * 2),
                Math.Max(completionPortThreads, maxConcurrentDownloads * 2)
            );
        }

        /// <summary>
        /// Retrieves available properties from Firebase and transforms them into a structured format.
        /// </summary>
        /// <returns>List of property information dictionaries</returns>
        public async Task<List<Dictionary<string, object>>> GetAvailablePropertiesFromFirebase()
        {
            var propertiesTask = firebaseClient
                .Child("Available Properties")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            var properties = await propertiesTask;

            // Transform the data to include the username as part of each property's data
            return properties?.Select(property =>
            {
                var propertyData = new Dictionary<string, object>(property.Value);
                propertyData["Username"] = property.Key;
                return propertyData;
            }).ToList() ?? new List<Dictionary<string, object>>();
        }

        /// <summary>
        /// Creates and displays property cards in a flow layout panel with optional title.
        /// Handles layout, styling, and asynchronous data loading.
        /// </summary>
        public async Task DisplayAvailableProperties(List<Dictionary<string, object>> properties, FlowLayoutPanel flowPanel, string title = null)
        {
            try
            {
                // Enable double buffering to prevent flickering during updates
                EnableDoubleBuffering(flowPanel);

                // Create all property cards first
                var propertyCards = properties.Select(property => CreatePropertyCard(property)).ToList();

                // Configure the flow panel layout
                flowPanel.SuspendLayout();
                flowPanel.Controls.Clear();
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

                // Add all cards to the panel
                flowPanel.Controls.AddRange(propertyCards.ToArray());
                flowPanel.ResumeLayout();

                // Start loading data for all cards concurrently
                var loadingTasks = propertyCards.Select((card, index) =>
                    InitiateDataLoading(card, properties[index]));

                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resizes an image while maintaining quality using high-quality bicubic interpolation.
        /// </summary>
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

        /// <summary>
        /// Initiates asynchronous loading of property data including images and address information.
        /// Uses concurrent operations for better performance.
        /// </summary>
        private async Task InitiateDataLoading(Panel card, Dictionary<string, object> property)
        {
            try
            {
                var loadingTasks = new List<Task>();

                // Load property image if available
                if (property.ContainsKey("Front Image"))
                {
                    var imageUrl = property["Front Image"].ToString();
                    loadingTasks.Add(LoadPropertyImageAsync(imageUrl, card));
                }

                // Load address information
                loadingTasks.Add(LoadPropertyAddressAsync(property, card));

                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating data loading: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a property card with loading animation and rounded corners.
        /// Sets up event handlers for user interaction.
        /// </summary>
        private Panel CreatePropertyCard(Dictionary<string, object> property, bool isLoading = false)
        {
            // Create main card panel with rounded corners
            var card = new Panel
            {
                Width = PROPERTY_CARD_WIDTH,
                Height = PROPERTY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = property
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
                AddPropertyControls(card, property);
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

        /// <summary>
        /// Handles the click event for a property card, triggering the PropertySelected event.
        /// This allows parent components to respond to user selection of properties.
        /// </summary>
        private void CardClick(Panel card)
        {
            if (card.Tag is Dictionary<string, object> propertyData)
            {
                PropertySelected?.Invoke(this, propertyData);
            }
        }

        /// <summary>
        /// Initiates a loading animation effect for property cards while content is being loaded.
        /// Uses a timer to create a subtle flickering effect that indicates loading state.
        /// </summary>
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

        /// <summary>
        /// Updates the visual state of loading elements within a property card.
        /// Applies alternating colors to create a subtle loading animation effect.
        /// </summary>
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

        /// <summary>
        /// Stops the loading animation and cleans up resources.
        /// Called when content has finished loading or when the card is being disposed.
        /// </summary>
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

        /// <summary>
        /// Adds the basic control structure to a property card including image box and labels.
        /// This creates the initial layout before data is loaded.
        /// </summary>
        private void AddPropertyControls(Panel card, Dictionary<string, object> property)
        {
            PictureBox propertyImage = CreatePropertyImageBox();
            card.Controls.Add(propertyImage);
            AddPropertyLabels(card, property);
        }

        /// <summary>
        /// Asynchronously loads and caches property images with retry logic and error handling.
        /// Uses a concurrent dictionary to prevent duplicate downloads of the same image.
        /// </summary>
        private async Task LoadPropertyImageAsync(string imageUrl, Panel propertyCard)
        {
            try
            {
                // Check if image is already cached
                if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
                {
                    var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        UpdatePropertyCardImage(propertyCard, cachedImage);
                        StopLoadingEffect(propertyCard);
                    }
                    return;
                }

                // Implement retry logic for resilient image loading
                int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries && !propertyCard.IsDisposed)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            var imageBytes = await client.GetByteArrayAsync(imageUrl);

                            if (!propertyCard.IsDisposed)
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
                                    var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox != null)
                                    {
                                        UpdatePropertyCardImage(propertyCard, imageCache[imageUrl]);
                                        StopLoadingEffect(propertyCard);
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

        /// <summary>
        /// Asynchronously loads property address data from Firebase with retry logic.
        /// Updates the property card with city information when available.
        /// </summary>
        private async Task LoadPropertyAddressAsync(Dictionary<string, object> property, Panel propertyCard)
        {
            try
            {
                int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries && !propertyCard.IsDisposed)
                {
                    try
                    {
                        // Query Firebase for address information
                        var addressData = await firebaseClient
                            .Child("Available Properties")
                            .Child(property["Username"].ToString())
                            .Child("Address")
                            .OnceSingleAsync<Dictionary<string, object>>();

                        if (!propertyCard.IsDisposed && addressData != null && addressData.ContainsKey("City"))
                        {
                            UpdatePropertyCardAddress(propertyCard, addressData["City"].ToString());
                            StopLoadingEffect(propertyCard);
                            return;
                        }
                    }
                    catch
                    {
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
                Console.WriteLine($"Error loading address: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the address display on a property card, ensuring the update occurs on the UI thread.
        /// </summary>
        private void UpdatePropertyCardAddress(Panel propertyCard, string city)
        {
            // Ensure UI updates happen on the correct thread
            if (propertyCard.InvokeRequired)
            {
                propertyCard.Invoke(new Action(() => UpdatePropertyCardAddress(propertyCard, city)));
                return;
            }

            var cityLabel = propertyCard.Controls.OfType<Label>().FirstOrDefault();
            if (cityLabel != null && !cityLabel.IsDisposed)
            {
                cityLabel.Text = $"City: {city}";
                cityLabel.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Updates the image displayed on a property card, ensuring the update occurs on the UI thread.
        /// Resizes the image to fit the card while maintaining aspect ratio.
        /// </summary>
        private void UpdatePropertyCardImage(Panel propertyCard, Image originalImage)
        {
            // Ensure UI updates happen on the correct thread
            if (propertyCard.InvokeRequired)
            {
                propertyCard.Invoke(new Action(() => UpdatePropertyCardImage(propertyCard, originalImage)));
                return;
            }

            var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && !pictureBox.IsDisposed)
            {
                pictureBox.Image = ResizeImage(originalImage, pictureBox.Width, pictureBox.Height);
                pictureBox.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Creates a PictureBox control with rounded corners for displaying property images.
        /// Configures the control with appropriate sizing and styling.
        /// </summary>
        private PictureBox CreatePropertyImageBox()
        {
            PictureBox propertyImage = new PictureBox
            {
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 150,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Add rounded corners using GraphicsPath
            int picRadius = 10;
            GraphicsPath picPath = new GraphicsPath();
            picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
            picPath.AddArc(propertyImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
            picPath.AddArc(propertyImage.Width - picRadius, propertyImage.Height - picRadius, picRadius, picRadius, 0, 90);
            picPath.AddArc(0, propertyImage.Height - picRadius, picRadius, picRadius, 90, 90);
            propertyImage.Region = new Region(picPath);

            return propertyImage;
        }

        /// <summary>
        /// Adds information labels to a property card with consistent styling and layout.
        /// Displays city, price, host information, and contact details.
        /// </summary>
        private void AddPropertyLabels(Panel propertyCard, Dictionary<string, object> property)
        {
            int yPosition = 170;
            int spacing = 25;

            // City Label (placeholder until data is loaded)
            Label cityLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = "Loading city...",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(cityLabel);
            yPosition += spacing;

            // Price Label
            Label priceLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Price per night: £{property["PricePerNight"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(priceLabel);
            yPosition += spacing;

            // Host Label
            Label hostLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Host: {property["Name"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(hostLabel);
            yPosition += spacing;

            // Contact Label
            Label contactLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Contact: {property["Email"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(contactLabel);
        }

        /// <summary>
        /// Searches for properties by city name using Firebase's query capabilities.
        /// Implements case-insensitive search with proper capitalization.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> SearchPropertiesByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return await GetAvailablePropertiesFromFirebase();

            try
            {
                // Normalize city name for consistent searching
                city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

                // Query Firebase using indexing for efficient filtering
                var query = await firebaseClient
                    .Child("Available Properties")
                    .OrderBy("Address/City")
                    .EqualTo(city)
                    .OnceAsync<Dictionary<string, object>>();

                // Transform query results into consistent format
                return query.Select(item =>
                {
                    var propertyData = new Dictionary<string, object>(item.Object);
                    propertyData["Username"] = item.Key;
                    return propertyData;
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }

        /// <summary>
        /// Enables double buffering on a FlowLayoutPanel to prevent flickering during updates.
        /// Uses reflection to access the protected double buffering property.
        /// </summary>
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        /// <summary>
        /// Displays city cards using the CityCardManager component.
        /// Delegates city card creation and management to specialized class.
        /// </summary>
        public async Task DisplayCitiesPanel(FlowLayoutPanel flowPanel, Button prevButton = null, Button nextButton = null)
        {
            //var cities = await saka.FetchCityImages();

            //await saka.DisplayAvailableCities(cities, flowPanel);

            await cityCardManager.DisplayCityCards(flowPanel, prevButton, nextButton);
        }
    }
}