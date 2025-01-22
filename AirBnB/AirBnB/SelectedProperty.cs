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
        private FirebaseClient firebaseClient; // Firebase client to interact with the Firebase database
        private const int DETAIL_CARD_WIDTH = 350; // Width of the property detail card
        private const int DETAIL_CARD_HEIGHT = 300; // Height of the property detail card
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>(); // Cache for images
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers = new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>(); // Timers for loading effect
        private const int FLICKER_INTERVAL = 200; // Interval for flickering effect
        private readonly Color LoadingColorDark = Color.FromArgb(230, 230, 230); // Dark loading color
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245); // Light loading color

        // Constructor accepting a FirebaseClient instance to interact with Firebase
        public SelectedProperty(FirebaseClient client)
        {
            firebaseClient = client;
        }

        // Method to display property details in the FlowLayoutPanel
        public async Task DisplayPropertyDetails(Dictionary<string, object> propertyData, FlowLayoutPanel flowPanel)
        {
            try
            {
                EnableDoubleBuffering(flowPanel); // Enable double buffering for smoother UI rendering

                flowPanel.SuspendLayout(); // Suspend layout while adding controls
                flowPanel.Controls.Clear(); // Clear previous content
                flowPanel.AutoScroll = true; // Enable auto-scrolling
                flowPanel.WrapContents = true; // Enable wrapping of controls
                flowPanel.FlowDirection = FlowDirection.LeftToRight; // Set flow direction for layout
                flowPanel.Padding = new Padding(10); // Set padding for the FlowLayoutPanel

                var images = await GetPropertyImages(propertyData); // Fetch property images
                var imageCards = images.Select(imageUrl => CreateDetailImageCard(imageUrl)).ToList(); // Create image cards for each image

                flowPanel.Controls.AddRange(imageCards.ToArray()); // Add image cards to FlowLayoutPanel
                flowPanel.ResumeLayout(); // Resume layout after adding controls

                var loadingTasks = imageCards.Select((card, index) =>
                    LoadDetailImageAsync(images[index], card)); // Load images asynchronously

                await Task.WhenAll(loadingTasks); // Wait for all image loading tasks to complete
            }
            catch (Exception ex)
            {
                // Show error message if there is any exception
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to fetch property images from Firebase or propertyData
        private async Task<List<string>> GetPropertyImages(Dictionary<string, object> propertyData)
        {
            if (propertyData.ContainsKey("Username"))
            {
                // Fetch images from Firebase database based on the username
                var images = await firebaseClient
                    .Child("Available Properties")
                    .Child(propertyData["Username"].ToString())
                    .Child("ImageUrls")
                    .OnceSingleAsync<List<string>>();
                return images ?? new List<string>();
            }
            else if (propertyData.ContainsKey("mainImage"))
            {
                // If a main image exists, return it as a list with a single entry
                return new List<string> { propertyData["mainImage"].ToString() };
            }
            return new List<string>();
        }

        // Method to fetch property address from Firebase
        public async Task<Dictionary<string, object>> GetPropertyAddress(string username)
        {
            return await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("Address")
                .OnceSingleAsync<Dictionary<string, object>>();
        }

        // Method to add reservation data to the Firebase database
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

            // Post the reservation data to the Firebase database under the "Reservations" node
            await firebaseClient
                .Child("Reservations")
                .PostAsync(reservationData);
        }

        // Method to create a card panel for displaying a property image
        private Panel CreateDetailImageCard(string imageUrl)
        {
            var card = new Panel
            {
                Width = DETAIL_CARD_WIDTH,
                Height = DETAIL_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Tag = imageUrl // Store the image URL in the Tag property
            };

            int cardRadius = 20;
            System.Drawing.Drawing2D.GraphicsPath cardPath = new System.Drawing.Drawing2D.GraphicsPath();
            cardPath.AddArc(0, 0, cardRadius, cardRadius, 180, 90);
            cardPath.AddArc(card.Width - cardRadius, 0, cardRadius, cardRadius, 270, 90);
            cardPath.AddArc(card.Width - cardRadius, card.Height - cardRadius, cardRadius, cardRadius, 0, 90);
            cardPath.AddArc(0, card.Height - cardRadius, cardRadius, cardRadius, 90, 90);
            card.Region = new Region(cardPath); // Apply rounded corners to the card

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
            propertyImage.Region = new Region(imagePath); // Apply rounded corners to the image

            card.Controls.Add(propertyImage); // Add the image to the card
            StartLoadingEffect(card); // Start the loading effect for the card

            return card;
        }

        // Method to start the loading effect (flickering) on a card
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

            loadingTimers.TryAdd(card, timer); // Add the timer to the loading timers dictionary
            timer.Start(); // Start the timer
        }

        // Method to update the loading effect (change background color) on the card
        private void UpdateLoadingEffect(Panel card, bool isLight)
        {
            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && pictureBox.Image == null)
            {
                pictureBox.BackColor = isLight ? LoadingColorLight : LoadingColorDark;
            }
        }

        // Method to stop the loading effect (flickering) on a card
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
                    pictureBox.BackColor = Color.White; // Set the background color to white once loading stops
                }
            }
        }

        // Method to resize an image to fit the specified width and height
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

        // Method to load an image asynchronously from a URL
        private async Task LoadDetailImageAsync(string imageUrl, Panel imageCard)
        {
            try
            {
                if (imageCache.TryGetValue(imageUrl, out Image cachedImage)) // Check if the image is already cached
                {
                    var pictureBox = imageCard.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox != null)
                    {
                        UpdatePropertyCardImage(imageCard, cachedImage); // Update the image with cached version
                        StopLoadingEffect(imageCard); // Stop the loading effect
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
                            var imageBytes = await client.GetByteArrayAsync(imageUrl); // Download image as byte array

                            if (!imageCard.IsDisposed)
                            {
                                using (var ms = new System.IO.MemoryStream(imageBytes))
                                {
                                    var image = Image.FromStream(ms); // Convert byte array to image
                                    if (!imageCache.ContainsKey(imageUrl))
                                    {
                                        imageCache.TryAdd(imageUrl, new Bitmap(image)); // Cache the image
                                    }

                                    var pictureBox = imageCard.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox != null)
                                    {
                                        UpdatePropertyCardImage(imageCard, imageCache[imageUrl]); // Update the image on the card
                                        StopLoadingEffect(imageCard); // Stop the loading effect
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
                            await Task.Delay(1000 * currentRetry); // Retry if there is a failure
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}"); // Log any exception encountered while loading image
            }
        }

        // Method to update the property card image with a new resized image
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
                pictureBox.Image = ResizeImage(originalImage, pictureBox.Width, pictureBox.Height); // Resize and set the image
                pictureBox.BackColor = Color.White; // Set background color to white after image loading
            }
        }

        // Method to enable double buffering for smoother rendering
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null); // Set DoubleBuffered property to true
        }
    }
}
