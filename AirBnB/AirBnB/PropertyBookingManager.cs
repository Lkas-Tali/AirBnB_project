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

namespace AirBnB
{
    public class PropertyBookingManager
    {
        private FirebaseClient firebaseClient;
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();
        private const int FLICKER_INTERVAL = 200; // milliseconds
        private readonly Color LoadingColorDark = Color.FromArgb(230, 230, 230);
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);

        private const int DETAIL_CARD_WIDTH = 350;
        private const int DETAIL_CARD_HEIGHT = 300;

        //  Field to track loading timers
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers =
            new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>();

        // Configure all HttpClient settings in a single initialization
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(0.1),
            DefaultRequestHeaders = { ConnectionClose = false }
        };

        public event EventHandler<Dictionary<string, object>> PropertySelected;

        public PropertyBookingManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<Dictionary<string, object>>> GetAvailablePropertiesFromFirebase()
        {
            // Get properties and their addresses in a single query
            var propertiesTask = firebaseClient
                .Child("Available Properties")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            var properties = await propertiesTask;

            return properties?.Select(property =>
            {
                var propertyData = new Dictionary<string, object>(property.Value);
                propertyData["Username"] = property.Key;
                return propertyData;
            }).ToList() ?? new List<Dictionary<string, object>>();
        }

        public async Task DisplayAvailableProperties(List<Dictionary<string, object>> properties, FlowLayoutPanel flowPanel)
        {
            try
            {
                EnableDoubleBuffering(flowPanel);

                // Create all property cards first before modifying the panel
                var propertyCards = properties.Select(property => CreatePropertyCard(property)).ToList();

                // Suspend layout once before making changes
                flowPanel.SuspendLayout();

                // Configure panel
                flowPanel.Controls.Clear();
                flowPanel.AutoScroll = true;
                flowPanel.WrapContents = true;
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.Padding = new Padding(10);

                // Add all cards at once
                flowPanel.Controls.AddRange(propertyCards.ToArray());

                // Resume layout once after all changes
                flowPanel.ResumeLayout();

                // Load data for all cards asynchronously
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

        // Image resizing
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

        private async Task InitiateDataLoading(Panel card, Dictionary<string, object> property)
        {
            try
            {
                var loadingTasks = new List<Task>();

                // Load image if URL exists
                if (property.ContainsKey("Front Image"))
                {
                    var imageUrl = property["Front Image"].ToString();
                    loadingTasks.Add(LoadPropertyImageAsync(imageUrl, card));
                }

                // Load address data
                loadingTasks.Add(LoadPropertyAddressAsync(property, card));

                // Wait for all loading tasks to complete
                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating data loading: {ex.Message}");
            }
        }

        private Panel CreatePropertyCard(Dictionary<string, object> property, bool isLoading = false)
        {
            var card = new Panel
            {
                Width = PROPERTY_CARD_WIDTH,
                Height = PROPERTY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = property
            };

            // Add rounded corners
            int radius = 20;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            if (isLoading)
            {
                // Add a loading indicator instead of placeholder text
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
                // Add the regular controls
                AddPropertyControls(card, property);

                // Start loading effect
                StartLoadingEffect(card);

                // Add click handler to the card
                card.Click += (s, e) => CardClick(card);

                // Add click handlers to interactive elements
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

        // Separate method to handle card clicks to ensure consistent behavior
        private void CardClick(Panel card)
        {
            if (card.Tag is Dictionary<string, object> propertyData)
            {
                PropertySelected?.Invoke(this, propertyData);
            }
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

                foreach (Control control in card.Controls)
                {
                    if (control is Label)
                    {
                        control.BackColor = Color.White;
                    }
                }
            }
        }

        private void AddPropertyControls(Panel card, Dictionary<string, object> property)
        {
            PictureBox propertyImage = CreatePropertyImageBox();
            card.Controls.Add(propertyImage);
            AddPropertyLabels(card, property);
        }

        private async Task LoadPropertyImageAsync(string imageUrl, Panel propertyCard)
        {
            try
            {
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
                                using (var ms = new System.IO.MemoryStream(imageBytes))
                                {
                                    var image = Image.FromStream(ms);
                                    if (!imageCache.ContainsKey(imageUrl))
                                    {
                                        // Store original size in cache
                                        imageCache.TryAdd(imageUrl, new Bitmap(image));
                                    }

                                    var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox != null)
                                    {
                                        // Resize for display
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
                            await Task.Delay(1000 * currentRetry); // Exponential backoff
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading address: {ex.Message}");
            }
        }

        private void UpdatePropertyCardAddress(Panel propertyCard, string city)
        {
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

        private PictureBox CreatePropertyImageBox()
        {
            PictureBox propertyImage = new PictureBox
            {
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 150,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand  // Add cursor to indicate clickable
            };

            // Add rounded corners
            int picRadius = 10;
            System.Drawing.Drawing2D.GraphicsPath picPath = new System.Drawing.Drawing2D.GraphicsPath();
            picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
            picPath.AddArc(propertyImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
            picPath.AddArc(propertyImage.Width - picRadius, propertyImage.Height - picRadius, picRadius, picRadius, 0, 90);
            picPath.AddArc(0, propertyImage.Height - picRadius, picRadius, picRadius, 90, 90);
            propertyImage.Region = new Region(picPath);

            return propertyImage;
        }

        private void AddPropertyLabels(Panel propertyCard, Dictionary<string, object> property)
        {
            int yPosition = 170;
            int spacing = 25;

            // City Label (placeholder)
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

        public async Task<List<Dictionary<string, object>>> SearchPropertiesByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return await GetAvailablePropertiesFromFirebase();

            var allProperties = await GetAvailablePropertiesFromFirebase();
            var matchingProperties = new List<Dictionary<string, object>>();

            //Capitalise city
            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

            foreach (var property in allProperties)
            {
                var addressData = await firebaseClient
                        .Child("Available Properties")
                        .Child(property["Username"].ToString())
                        .Child("Address")
                        .OnceSingleAsync<Dictionary<string, object>>();

                if (addressData != null && addressData["City"].ToString() == city)
                {
                    matchingProperties.Add(property);
                }
            }

            return matchingProperties;
        }

        // Enable double buffering on the panel
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        public async void AddReservationToDatabase(string customerName, string endDate, int nights, string startDate, Dictionary<string, object> propertyData, Dictionary<string, object> propertyAddress)
        {

            //debug:
            // Debug: Print all keys in the dictionaries
            Console.WriteLine("Available propertyAddress:");
            foreach (var key in propertyAddress.Keys)
            {
                Console.WriteLine($"Key: '{key}'");
            }

            foreach (var key in propertyData.Keys)
            {
                Console.WriteLine($"Key: '{key}'");
            }

            // Initialize reservation data dictionary with booking details
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
    }
}