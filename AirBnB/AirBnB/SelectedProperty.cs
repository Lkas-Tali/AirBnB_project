using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Linq;
using Firebase.Database.Query;

namespace AirBnB
{
    public class SelectedProperty
    {
        private FirebaseClient firebaseClient;
        private const int DETAIL_CARD_WIDTH = 350;
        private const int DETAIL_CARD_HEIGHT = 300;
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers =
            new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>();
        private const int FLICKER_INTERVAL = 200;
        private readonly Color LoadingColorDark = Color.FromArgb(230, 230, 230);
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);

        public SelectedProperty(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task DisplayPropertyDetails(Dictionary<string, object> propertyData, FlowLayoutPanel flowPanel)
        {
            try
            {
                EnableDoubleBuffering(flowPanel);

                flowPanel.SuspendLayout();
                flowPanel.Controls.Clear();
                flowPanel.AutoScroll = true;
                flowPanel.WrapContents = true;
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.Padding = new Padding(10);

                var images = await GetPropertyImages(propertyData);
                var imageCards = images.Select(imageUrl => CreateDetailImageCard(imageUrl)).ToList();

                flowPanel.Controls.AddRange(imageCards.ToArray());
                flowPanel.ResumeLayout();

                var loadingTasks = imageCards.Select((card, index) =>
                    LoadDetailImageAsync(images[index], card));

                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetPropertyImages(Dictionary<string, object> propertyData)
        {
            if (propertyData.ContainsKey("Username"))
            {
                var images = await firebaseClient
                    .Child("Available Properties")
                    .Child(propertyData["Username"].ToString())
                    .Child("ImageUrls")
                    .OnceSingleAsync<List<string>>();
                return images ?? new List<string>();
            }
            else if (propertyData.ContainsKey("mainImage"))
            {
                return new List<string> { propertyData["mainImage"].ToString() };
            }
            return new List<string>();
        }

        public async Task<Dictionary<string, object>> GetPropertyAddress(string username)
        {
            return await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("Address")
                .OnceSingleAsync<Dictionary<string, object>>();
        }

        public async void AddReservationToDatabase(string customerName, string endDate, int nights, string startDate,
            Dictionary<string, object> propertyData, Dictionary<string, object> propertyAddress)
        {
            var reservationData = new Dictionary<string, object>
            {
                { "address", propertyAddress["Address"]},
                { "city", propertyAddress["City"] },
                { "customerName", customerName },
                { "description", propertyAddress["Description"] },
                { "email", propertyData["Email"] },
                { "endDate", endDate },
                { "mainImage", propertyData["Front Image"] },
                { "nights", nights },
                { "owner", propertyData["Name"] },
                { "pricePerNight", propertyData["PricePerNight"]},
                { "startDate", startDate },
                { "title", propertyAddress["Title"] }
            };

            await firebaseClient
                .Child("Reservations")
                .PostAsync(reservationData);
        }

        private Panel CreateDetailImageCard(string imageUrl)
        {
            var card = new Panel
            {
                Width = DETAIL_CARD_WIDTH,
                Height = DETAIL_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Tag = imageUrl
            };

            int cardRadius = 20;
            System.Drawing.Drawing2D.GraphicsPath cardPath = new System.Drawing.Drawing2D.GraphicsPath();
            cardPath.AddArc(0, 0, cardRadius, cardRadius, 180, 90);
            cardPath.AddArc(card.Width - cardRadius, 0, cardRadius, cardRadius, 270, 90);
            cardPath.AddArc(card.Width - cardRadius, card.Height - cardRadius, cardRadius, cardRadius, 0, 90);
            cardPath.AddArc(0, card.Height - cardRadius, cardRadius, cardRadius, 90, 90);
            card.Region = new Region(cardPath);

            PictureBox propertyImage = new PictureBox
            {
                Width = DETAIL_CARD_WIDTH - 20,
                Height = DETAIL_CARD_HEIGHT - 20,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            int imageRadius = 10;
            System.Drawing.Drawing2D.GraphicsPath imagePath = new System.Drawing.Drawing2D.GraphicsPath();
            imagePath.AddArc(0, 0, imageRadius, imageRadius, 180, 90);
            imagePath.AddArc(propertyImage.Width - imageRadius, 0, imageRadius, imageRadius, 270, 90);
            imagePath.AddArc(propertyImage.Width - imageRadius, propertyImage.Height - imageRadius, imageRadius, imageRadius, 0, 90);
            imagePath.AddArc(0, propertyImage.Height - imageRadius, imageRadius, imageRadius, 90, 90);
            propertyImage.Region = new Region(imagePath);

            card.Controls.Add(propertyImage);
            StartLoadingEffect(card);

            return card;
        }

        private void StartLoadingEffect(Panel card)
        {
            var timer = new System.Windows.Forms.Timer
            {
                Interval = FLICKER_INTERVAL
            };

            bool isLight = true;
            timer.Tick += (s, e) =>
            {
                if (card.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

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

            loadingTimers.TryAdd(card, timer);
            timer.Start();
        }

        private void UpdateLoadingEffect(Panel card, bool isLight)
        {
            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && pictureBox.Image == null)
            {
                pictureBox.BackColor = isLight ? LoadingColorLight : LoadingColorDark;
            }
        }

        private void StopLoadingEffect(Panel card)
        {
            if (loadingTimers.TryRemove(card, out var timer))
            {
                timer.Stop();
                timer.Dispose();
            }

            if (!card.IsDisposed)
            {
                var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pictureBox != null)
                {
                    pictureBox.BackColor = Color.White;
                }
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private async Task LoadDetailImageAsync(string imageUrl, Panel imageCard)
        {
            try
            {
                if (imageCache.TryGetValue(imageUrl, out Image cachedImage))
                {
                    var pictureBox = imageCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        UpdatePropertyCardImage(imageCard, cachedImage);
                        StopLoadingEffect(imageCard);
                    }
                    return;
                }

                int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries && !imageCard.IsDisposed)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            var imageBytes = await client.GetByteArrayAsync(imageUrl);

                            if (!imageCard.IsDisposed)
                            {
                                using (var ms = new System.IO.MemoryStream(imageBytes))
                                {
                                    var image = Image.FromStream(ms);
                                    if (!imageCache.ContainsKey(imageUrl))
                                    {
                                        imageCache.TryAdd(imageUrl, new Bitmap(image));
                                    }

                                    var pictureBox = imageCard.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox != null)
                                    {
                                        UpdatePropertyCardImage(imageCard, imageCache[imageUrl]);
                                        StopLoadingEffect(imageCard);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    catch
                    {
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
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

        private void UpdatePropertyCardImage(Panel propertyCard, Image originalImage)
        {
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

        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }
    }
}