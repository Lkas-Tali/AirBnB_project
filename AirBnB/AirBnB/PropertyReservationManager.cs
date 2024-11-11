using Firebase.Database;
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

        // Field to track loading timers
        private readonly ConcurrentDictionary<Panel, System.Windows.Forms.Timer> loadingTimers =
            new ConcurrentDictionary<Panel, System.Windows.Forms.Timer>();

        // Image cache
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();

        // Configure HttpClient settings
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(0.1),
            DefaultRequestHeaders = { ConnectionClose = false }
        };

        public event EventHandler<Dictionary<string, object>> ReservationSelected;

        public PropertyReservationManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<Dictionary<string, object>>> RetrieveUserReservationDetails(string username)
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

        public async Task DisplayUserReservations(List<Dictionary<string, object>> reservations, FlowLayoutPanel flowPanel)
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

                var loadingLabel = new Label
                {
                    Text = "Loading reservations...",
                    Font = new Font("Nirmala UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 56, 92),
                    AutoSize = true,
                    Location = new Point(flowPanel.Width / 2 - 100, flowPanel.Height / 2 - 15),
                };
                flowPanel.Controls.Add(loadingLabel);
                flowPanel.ResumeLayout();

                // Create cards and immediately set cached images if available
                var cards = reservations.Select(reservation =>
                {
                    var card = CreateReservationCard(reservation);

                    if (reservation.ContainsKey("mainImage"))
                    {
                        var imageUrl = reservation["mainImage"].ToString();
                        if (imageCache.TryGetValue(imageUrl, out var cachedImage))
                        {
                            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
                            if (pictureBox != null)
                            {
                                pictureBox.Image = ResizeImage(cachedImage, pictureBox.Width, pictureBox.Height);
                                if (loadingTimers.TryGetValue(card, out var timer))
                                {
                                    timer.Stop();
                                    timer.Dispose();
                                    loadingTimers.TryRemove(card, out _);
                                }
                            }
                        }
                    }

                    return (card, reservation);
                }).ToList();

                flowPanel.SuspendLayout();
                flowPanel.Controls.Remove(loadingLabel);
                flowPanel.Controls.AddRange(cards.Select(c => c.card).ToArray());
                flowPanel.ResumeLayout();

                // Start loading data for all cards after they're added to the panel
                var loadingTasks = cards.Select(c => LoadReservationData(c.card, c.reservation));
                await Task.WhenAll(loadingTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method at the start of the class
        private void EnableDoubleBuffering(FlowLayoutPanel panel)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(panel, true, null);
        }

        // Helper method for image resizing
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

            // Add rounded corners
            int radius = 20;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            // Add the regular controls
            AddReservationControls(card, reservation);

            // Start loading effect
            StartLoadingEffect(card);

            // Add click handlers
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

        private void AddReservationControls(Panel card, Dictionary<string, object> reservation)
        {
            // Add property image
            PictureBox propertyImage = CreatePropertyImageBox();
            card.Controls.Add(propertyImage);

            // Add labels
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

        private void reservationCard_Click(object sender, EventArgs e)
        {
            if (sender is Panel propertyCard && propertyCard.Tag is Dictionary<string, object> propertyData)
            {
                ReservationSelected?.Invoke(this, propertyData);
            }
        }

        public async Task CancelReservation(Dictionary<string, object> reservation)
        {
            try
            {
                if (reservation.ContainsKey("firebaseKey"))
                {
                    string reservationId = reservation["firebaseKey"].ToString();
                    await firebaseClient
                        .Child("Reservations")
                        .Child(reservationId)
                        .DeleteAsync();

                    MessageBox.Show("Reservation cancelled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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