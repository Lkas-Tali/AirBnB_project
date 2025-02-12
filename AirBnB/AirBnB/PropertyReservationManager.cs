﻿using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class PropertyReservationManager
    {
        private FirebaseClient firebaseClient;
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;
        private const int FLICKER_INTERVAL = 200; // milliseconds
        private readonly Color LoadingColorDark = Color.FromArgb(230, 230, 230);
        private readonly Color LoadingColorLight = Color.FromArgb(245, 245, 245);

        // Field to track loading timers for each property card
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers =
            new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>();

        // Cache to store downloaded images to avoid redundant downloads
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();

        // Configure HttpClient settings for optimal performance
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(0.1),
            DefaultRequestHeaders = { ConnectionClose = false }
        };

        // Event to notify when a reservation is selected
        public event EventHandler<Dictionary<string, object>> ReservationSelected;

        public PropertyReservationManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        // Retrieves reservation details for a specific user from Firebase
        public virtual async Task<List<Dictionary<string, object>>> RetrieveUserReservationDetails(string username)
        {
            var reservationData = await firebaseClient
                .Child("Reservations")
                .OnceAsync<Dictionary<string, object>>();

            var reservations = new List<Dictionary<string, object>>();

            foreach (var reservation in reservationData)
            {
                var reservationDetails = reservation.Object;

                if (reservationDetails != null &&
                    reservationDetails["customerName"].ToString() == username)
                {
                    reservationDetails["firebaseKey"] = reservation.Key;
                    reservations.Add(reservationDetails);
                }
            }

            return reservations;
        }

        // Displays user reservations in a FlowLayoutPanel
        public async Task DisplayUserReservations(List<Dictionary<string, object>> reservations, FlowLayoutPanel flowPanel)
        {
            try
            {
                // Enable double buffering for smoother rendering
                EnableDoubleBuffering(flowPanel);

                // Create all property cards first before modifying the panel
                var propertyCards = reservations.Select(property => CreateReservationCard(property)).ToList();

                // Suspend layout once before making changes
                flowPanel.SuspendLayout();

                // Configure panel properties
                flowPanel.Controls.Clear();
                flowPanel.AutoScroll = true;
                flowPanel.WrapContents = true;
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.Padding = new Padding(10);

                // Add all cards at once for better performance
                flowPanel.Controls.AddRange(propertyCards.ToArray());

                // Resume layout once after all changes
                flowPanel.ResumeLayout();

                // Load data for all cards asynchronously to improve responsiveness
                var loadingTasks = propertyCards.Select((card, index) =>
                    LoadReservationData(card, reservations[index]));

                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to enable double buffering for a FlowLayoutPanel
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        // Helper method for resizing images while preserving aspect ratio
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

        // Loads reservation data asynchronously for a property card
        private async Task LoadReservationData(Panel card, Dictionary<string, object> reservation)
        {
            try
            {
                if (reservation.ContainsKey("mainImage"))
                {
                    var imageUrl = reservation["mainImage"].ToString();
                    await LoadPropertyImageAsync(imageUrl, card);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reservation data: {ex.Message}");
            }
        }

        // Creates a reservation card panel for a reservation
        private Panel CreateReservationCard(Dictionary<string, object> reservation)
        {
            var card = new Panel
            {
                Width = PROPERTY_CARD_WIDTH,
                Height = PROPERTY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = reservation
            };

            // Add rounded corners to the card
            int radius = 20;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            // Add the regular controls to the card
            AddReservationControls(card, reservation);

            // Start loading effect for the card
            StartLoadingEffect(card);

            // Add click handlers for the card and its controls
            card.Click += (s, e) => reservationCard_Click(card, e);
            foreach (Control control in card.Controls)
            {
                control.Click += (s, e) => reservationCard_Click(card, e);
                if (control is PictureBox pictureBox)
                {
                    pictureBox.Cursor = Cursors.Hand;
                    pictureBox.Click += (s, e) => reservationCard_Click(card, e);
                }
            }

            return card;
        }

        // Adds controls to a reservation card panel
        private void AddReservationControls(Panel card, Dictionary<string, object> reservation)
        {
            // Add property image
            PictureBox propertyImage = CreatePropertyImageBox();
            card.Controls.Add(propertyImage);

            // Add labels for reservation details
            int yPosition = 170;
            int spacing = 25;

            // City Label
            Label cityLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"City: {reservation["city"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            card.Controls.Add(cityLabel);
            yPosition += spacing;

            // Total Price Label
            decimal pricePerNight = decimal.Parse(reservation["pricePerNight"].ToString());
            int nights = int.Parse(reservation["nights"].ToString());
            decimal totalPrice = pricePerNight * nights;

            Label totalPriceLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Total price: £{totalPrice:F2}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            card.Controls.Add(totalPriceLabel);
            yPosition += spacing;

            // Host Label
            Label hostLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Host: {reservation["owner"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            card.Controls.Add(hostLabel);
            yPosition += spacing;

            // Contact Label
            Label contactLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Contact: {reservation["email"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            card.Controls.Add(contactLabel);
        }

        // Creates a PictureBox for the property image with rounded corners
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

            // Add rounded corners to the PictureBox
            int picRadius = 10;
            System.Drawing.Drawing2D.GraphicsPath picPath = new System.Drawing.Drawing2D.GraphicsPath();
            picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
            picPath.AddArc(propertyImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
            picPath.AddArc(propertyImage.Width - picRadius, propertyImage.Height - picRadius, picRadius, picRadius, 0, 90);
            picPath.AddArc(0, propertyImage.Height - picRadius, picRadius, picRadius, 90, 90);
            propertyImage.Region = new Region(picPath);

            return propertyImage;
        }

        // Starts the loading effect for a property card
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

        // Updates the loading effect for a property card
        private void UpdateLoadingEffect(Panel card, bool isLight)
        {
            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null && pictureBox.Image == null)
            {
                pictureBox.BackColor = isLight ? LoadingColorLight : LoadingColorDark;
            }
        }

        // Stops the loading effect for a property card
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

        // Loads the property image asynchronously and updates the card
        private async Task LoadPropertyImageAsync(string imageUrl, Panel propertyCard)
        {
            try
            {
                // Check if the image is already cached
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
                                        // Store original size image in cache
                                        imageCache.TryAdd(imageUrl, new Bitmap(image));
                                    }

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

        // Updates the property card image with the resized image
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

        // Event handler for when a reservation card is clicked
        private void reservationCard_Click(object sender, EventArgs e)
        {
            if (sender is Panel propertyCard && propertyCard.Tag is Dictionary<string, object> propertyData)
            {
                ReservationSelected?.Invoke(this, propertyData);
            }
        }

        // Cancels a reservation by deleting it from Firebase
        public virtual async Task CancelReservation(Dictionary<string, object> reservation)
        {
            try
            {
                if (reservation.ContainsKey("firebaseKey"))
                {
                    string reservationId = reservation["firebaseKey"].ToString();
                    await firebaseClient
                    .Child("Reservations")
                    .Child(reservationId)
                    .DeleteAsync(); MessageBox.Show("Reservation cancelled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not find the reservation ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling reservation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}