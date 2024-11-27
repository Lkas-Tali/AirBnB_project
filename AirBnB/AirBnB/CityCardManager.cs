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

        public static readonly List<string> AVAILABLE_CITIES = new List<string>
        {
            "Bath", "Belfast", "Birmingham", "Brighton", "Bristol", "Cambridge",
            "Cardiff", "Edinburgh", "Glasgow", "Leeds", "Liverpool", "London",
            "Manchester", "Newcastle", "Nottingham", "Oxford", "Sheffield", "York"
        };


        public event EventHandler<string> CitySelected;

        public CityCardManager(FirebaseClient client)
        {
            firebaseClient = client;

            int processorCount = Math.Min(Environment.ProcessorCount * 2, 32);
            imageLoader = new ImageLoader(processorCount);
            loadingSemaphore = new SemaphoreSlim(1, 1);
            threadManager = new ThreadManager(processorCount);
        }

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

        // New method to display the current page
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

        // New method to handle moving to the next page
        public void NextPage()
        {
            if (HasNextPage())
            {
                currentPage++;
                DisplayCurrentPage();
                UpdatePaginationButtons();
            }
        }

        // New method to handle moving to the previous page
        public void PreviousPage()
        {
            if (HasPreviousPage())
            {
                currentPage--;
                DisplayCurrentPage();
                UpdatePaginationButtons();
            }
        }

        // Helper method to check if there is a next page
        private bool HasNextPage()
        {
            int totalPages = (AVAILABLE_CITIES.Count + CITIES_PER_PAGE - 1) / CITIES_PER_PAGE;
            return currentPage < totalPages - 1;
        }

        // Helper method to check if there is a previous page
        private bool HasPreviousPage()
        {
            return currentPage > 0;
        }

        // Method to update button states
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

        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        public void WaitForAllImages()
        {
            if (!isDisposed)
            {
                imageLoader.WaitForAllImages();
            }
        }

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