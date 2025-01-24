using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class CityCardManager : IDisposable
    {
        private readonly FirebaseClient firebaseClient;
        private const int CITY_CARD_WIDTH = 250;
        private const int CITY_CARD_HEIGHT = 300;
        private Dictionary<string, string> cityImages;
        private readonly ImageLoader imageLoader;
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);
        private bool isDisposed;
        private readonly SemaphoreSlim loadingSemaphore;
        private readonly ThreadManager threadManager;

        // Pagination-related fields
        private int currentPage = 0;
        private const int CITIES_PER_PAGE = 6;
        private FlowLayoutPanel activeFlowPanel;
        private Button previousButton;
        private Button nextButton;

        // List of available cities
        public static readonly List<string> AVAILABLE_CITIES = new List<string>
        {
            "Bath", "Belfast", "Birmingham", "Brighton", "Bristol", "Cambridge",
            "Cardiff", "Edinburgh", "Glasgow", "Leeds", "Liverpool", "London",
            "Manchester", "Newcastle", "Nottingham", "Oxford", "Sheffield", "York"
        };

        // Event raised when a city is selected
        public event EventHandler<string> CitySelected;

        public CityCardManager(FirebaseClient client)
        {
            firebaseClient = client;

            int processorCount = Math.Min(Environment.ProcessorCount * 2, 32);
            imageLoader = new ImageLoader(processorCount);
            loadingSemaphore = new SemaphoreSlim(1, 1);
            threadManager = new ThreadManager(processorCount);
        }

        // Display city cards in the provided FlowLayoutPanel
        public async Task DisplayCityCards(FlowLayoutPanel flowPanel, Button prevButton = null, Button nextButton = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(CityCardManager));

            try
            {
                await loadingSemaphore.WaitAsync();

                if (flowPanel.InvokeRequired)
                {
                    await (Task)flowPanel.Invoke(new Func<Task>(() => DisplayCityCards(flowPanel, prevButton, nextButton)));
                    return;
                }

                // Store references for pagination
                activeFlowPanel = flowPanel;
                previousButton = prevButton;
                this.nextButton = nextButton;

                EnableDoubleBuffering(flowPanel);
                await LoadCityImages();

                DisplayCurrentPage();

                // Update button states
                UpdatePaginationButtons();
            }
            finally
            {
                loadingSemaphore.Release();
            }
        }

        // Display the current page of city cards
        public async Task DisplayCurrentPage()
        {
            if (activeFlowPanel == null) return;

            activeFlowPanel.SuspendLayout();
            activeFlowPanel.Controls.Clear();

            // Calculate start and end indices for current page
            int startIndex = currentPage * CITIES_PER_PAGE;
            var citiesToDisplay = AVAILABLE_CITIES
                .Skip(startIndex)
                .Take(CITIES_PER_PAGE)
                .ToList();

            foreach (string city in citiesToDisplay)
            {
                var cityCard = CreateCityCard(city);
                activeFlowPanel.Controls.Add(cityCard);

                if (cityImages?.TryGetValue(city, out string imageUrl) == true)
                {
                    var pictureBox = cityCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        // Try to get the image from shared cache
                        if (SharedImageCache.Instance.IsImageCached(imageUrl))
                        {
                            var image = await SharedImageCache.Instance.GetOrLoadImageAsync(
                                imageUrl,
                                CityPreloader.CITY_IMAGE_WIDTH,
                                CityPreloader.CITY_IMAGE_HEIGHT
                            );
                            pictureBox.Image = image;
                            pictureBox.BackColor = Color.White;
                        }
                        else
                        {
                            // Start loading if not cached
                            pictureBox.BackColor = LoadingColorLight;
                            LoadImageForCard(imageUrl, pictureBox, cityCard);
                        }
                    }
                }
            }

            activeFlowPanel.ResumeLayout();
            UpdatePaginationButtons();
        }

        // Load the image for a city card asynchronously
        private async void LoadImageForCard(string imageUrl, PictureBox pictureBox, Panel card)
        {
            try
            {
                var image = await SharedImageCache.Instance.GetOrLoadImageAsync(
                    imageUrl,
                    CityPreloader.CITY_IMAGE_WIDTH,
                    CityPreloader.CITY_IMAGE_HEIGHT
                );

                if (!pictureBox.IsDisposed && !card.IsDisposed)
                {
                    pictureBox.Image = image;
                    pictureBox.BackColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        // Move to the next page of city cards
        public void NextPage()
        {
            if (HasNextPage())
            {
                currentPage++;
                DisplayCurrentPage();
                UpdatePaginationButtons();
            }
        }

        // Move to the previous page of city cards
        public void PreviousPage()
        {
            if (HasPreviousPage())
            {
                currentPage--;
                DisplayCurrentPage();
                UpdatePaginationButtons();
            }
        }

        // Check if there is a next page of city cards
        private bool HasNextPage()
        {
            int totalPages = (AVAILABLE_CITIES.Count + CITIES_PER_PAGE - 1) / CITIES_PER_PAGE;
            return currentPage < totalPages - 1;
        }

        // Check if there is a previous page of city cards
        private bool HasPreviousPage()
        {
            return currentPage > 0;
        }

        // Update the states of the pagination buttons
        private void UpdatePaginationButtons()
        {
            if (previousButton != null)
            {
                previousButton.Enabled = HasPreviousPage();
            }

            if (nextButton != null)
            {
                nextButton.Enabled = HasNextPage();
            }
        }

        // Check if a city card is visible within the FlowLayoutPanel
        private bool IsCardVisible(Panel card, FlowLayoutPanel flowPanel)
        {
            try
            {
                if (flowPanel.InvokeRequired)
                {
                    return (bool)flowPanel.Invoke(new Func<bool>(() => IsCardVisible(card, flowPanel)));
                }

                Rectangle visibleBounds = flowPanel.ClientRectangle;
                Point cardLocation = flowPanel.PointToClient(card.PointToScreen(Point.Empty));
                return visibleBounds.IntersectsWith(new Rectangle(cardLocation, card.Size));
            }
            catch (InvalidOperationException)
            {
                // Control might have been disposed
                return false;
            }
        }

        // Handle the scroll event of the FlowLayoutPanel
        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            try
            {
                var flowPanel = sender as FlowLayoutPanel;
                if (flowPanel == null) return;

                if (flowPanel.InvokeRequired)
                {
                    flowPanel.Invoke(new ScrollEventHandler(HandleScroll), sender, e);
                    return;
                }

                var controls = flowPanel.Controls.Cast<Control>().ToList();
                foreach (Panel card in controls.OfType<Panel>().Where(c => IsCardVisible(c, flowPanel)))
                {
                    string city = card.Tag as string;
                    if (cityImages?.TryGetValue(city, out string imageUrl) == true)
                    {
                        var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
                        if (pictureBox != null && pictureBox.Image == null)
                        {
                            imageLoader.LoadImage(imageUrl, pictureBox, card);
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Control might have been disposed
            }
        }

        // Create a city card with rounded corners and city image and label
        private Panel CreateCityCard(string city)
        {
            var card = new Panel
            {
                Width = CITY_CARD_WIDTH,
                Height = CITY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = city
            };

            using (var path = new GraphicsPath())
            {
                int radius = 20;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
            }

            PictureBox cityImage = CreateCityImageBox();
            Label cityLabel = CreateCityLabel(city);

            card.Controls.Add(cityImage);
            card.Controls.Add(cityLabel);

            // Handle click events
            EventHandler clickHandler = (s, e) => CitySelected?.Invoke(this, city);
            card.Click += clickHandler;
            cityLabel.Click += clickHandler;
            cityImage.Click += clickHandler;

            return card;
        }

        // Create the city image picture box with rounded corners
        private PictureBox CreateCityImageBox()
        {
            var cityImage = new PictureBox
            {
                Width = CITY_CARD_WIDTH - 20,
                Height = CITY_CARD_HEIGHT - 50,
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

        // Create the city label with the specified city name
        private Label CreateCityLabel(string city)
        {
            return new Label
            {
                Text = city,
                Font = new Font("Nirmala UI", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = CITY_CARD_WIDTH - 20,
                Height = 30,
                Location = new Point(10, CITY_CARD_HEIGHT - 35)
            };
        }

        // Load city images from Firebase asynchronously
        private async Task LoadCityImages()
        {
            if (cityImages == null)
            {
                try
                {
                    cityImages = await firebaseClient
                        .Child("cities images")
                        .OnceSingleAsync<Dictionary<string, string>>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading city images: {ex.Message}");
                    cityImages = new Dictionary<string, string>();
                }
            }
        }

        // Enable double buffering for the FlowLayoutPanel to reduce flickering
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        // Wait for all images to finish loading
        public void WaitForAllImages()
        {
            if (!isDisposed)
            {
                imageLoader.WaitForAllImages();
            }
        }

        // Dispose the CityCardManager and its resources
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            imageLoader.Dispose();
            loadingSemaphore.Dispose();
            threadManager.Dispose();
        }
    }
}