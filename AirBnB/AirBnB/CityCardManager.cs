using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class CityCardManager : IDisposable
    {
        private readonly FirebaseClient firebaseClient;
        private const int CITY_CARD_WIDTH = 200;
        private const int CITY_CARD_HEIGHT = 150;
        private Dictionary<string, string> cityImages;
        private readonly ImageLoader imageLoader;
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);
        private bool isDisposed;

        public readonly List<string> AVAILABLE_CITIES = new List<string>
        {
            "London", "Manchester", "Birmingham", "Edinburgh", "Glasgow", "Liverpool",
            "Leeds", "Bristol", "Newcastle", "Cardiff", "Belfast", "Brighton",
            "Cambridge", "Oxford", "York", "Bath", "Nottingham", "Sheffield"
        };

        public event EventHandler<string> CitySelected;

        public CityCardManager(FirebaseClient client)
        {
            firebaseClient = client;
            int processorCount = Environment.ProcessorCount;
            imageLoader = new ImageLoader(processorCount * 2);
        }

        public async Task DisplayCityCards(FlowLayoutPanel flowPanel)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(CityCardManager));

            EnableDoubleBuffering(flowPanel);
            await LoadCityImages();

            flowPanel.SuspendLayout();
            flowPanel.Controls.Clear();
            flowPanel.AutoScroll = true;
            flowPanel.WrapContents = true;
            flowPanel.FlowDirection = FlowDirection.LeftToRight;
            flowPanel.Padding = new Padding(10);

            foreach (string city in AVAILABLE_CITIES)
            {
                var cityCard = CreateCityCard(city);
                flowPanel.Controls.Add(cityCard);

                if (cityImages != null && cityImages.TryGetValue(city, out string imageUrl))
                {
                    var pictureBox = cityCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        imageLoader.LoadImage(imageUrl, pictureBox, cityCard);
                    }
                }
            }

            flowPanel.ResumeLayout();
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

            // Add rounded corners
            int radius = 20;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            PictureBox cityImage = CreateCityImageBox();
            Label cityLabel = CreateCityLabel(city);

            card.Controls.Add(cityImage);
            card.Controls.Add(cityLabel);

            // Handle click events
            card.Click += (s, e) => CitySelected?.Invoke(this, city);
            cityLabel.Click += (s, e) => CitySelected?.Invoke(this, city);
            cityImage.Click += (s, e) => CitySelected?.Invoke(this, city);

            return card;
        }

        private PictureBox CreateCityImageBox()
        {
            PictureBox cityImage = new PictureBox
            {
                Width = CITY_CARD_WIDTH - 20,
                Height = CITY_CARD_HEIGHT - 40,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = LoadingColorLight,
                Cursor = Cursors.Hand
            };

            // Add rounded corners
            int picRadius = 10;
            GraphicsPath picPath = new GraphicsPath();
            picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
            picPath.AddArc(cityImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
            picPath.AddArc(cityImage.Width - picRadius, cityImage.Height - picRadius, picRadius, picRadius, 0, 90);
            picPath.AddArc(0, cityImage.Height - picRadius, picRadius, picRadius, 90, 90);
            cityImage.Region = new Region(picPath);

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
        }
    }
}