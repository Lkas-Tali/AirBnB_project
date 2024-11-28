using NUnit.Framework;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.Linq;
using System.Threading;

namespace AirBnB.Tests
{
    public class TestReservationManager : PropertyReservationManager
    {
        private readonly List<Dictionary<string, object>> _mockReservations;
        private string _lastDeletedKey;

        public TestReservationManager() : base(null)
        {
            _mockReservations = new List<Dictionary<string, object>>();
        }

        public void SetupMockReservations(List<Dictionary<string, object>> reservations)
        {
            _mockReservations.Clear();
            _mockReservations.AddRange(reservations);
        }

        public string GetLastDeletedKey() => _lastDeletedKey;

        public override async Task<List<Dictionary<string, object>>> RetrieveUserReservationDetails(string username)
        {
            return _mockReservations
                .Where(r => r.ContainsKey("customerName") && r["customerName"].ToString() == username)
                .Select(r => new Dictionary<string, object>(r))
                .ToList();
        }

        public override async Task CancelReservation(Dictionary<string, object> reservation)
        {
            if (!reservation.ContainsKey("firebaseKey"))
            {
                throw new Exception("Could not find the reservation ID.");
            }

            _lastDeletedKey = reservation["firebaseKey"].ToString();
        }
    }

    [TestFixture]
    public class PropertyReservationManagerTests
    {
        private TestReservationManager _manager;
        private FlowLayoutPanel _flowPanel;

        [SetUp]
        public void Setup()
        {
            _manager = new TestReservationManager();
            _flowPanel = new FlowLayoutPanel();
        }

        [TearDown]
        public void Cleanup()
        {
            _flowPanel.Dispose();
        }

        [Test]
        public async Task RetrieveUserReservationDetails_ValidUsername_ReturnsCorrectReservations()
        {
            // Arrange
            var username = "testUser";
            var testData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "customerName", "testUser" },
                    { "city", "London" },
                    { "firebaseKey", "key1" }
                },
                new Dictionary<string, object> {
                    { "customerName", "otherUser" },
                    { "city", "Paris" },
                    { "firebaseKey", "key2" }
                }
            };

            _manager.SetupMockReservations(testData);

            // Act
            var result = await _manager.RetrieveUserReservationDetails(username);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0]["customerName"], Is.EqualTo("testUser"));
            Assert.That(result[0]["firebaseKey"], Is.EqualTo("key1"));
        }

        [Test]
        public async Task DisplayUserReservations_ValidReservations_CreatesCorrectNumberOfCards()
        {
            // Arrange
            var reservations = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "city", "London" },
                    { "pricePerNight", "100" },
                    { "nights", "2" },
                    { "owner", "TestOwner" },
                    { "email", "test@example.com" }
                },
                new Dictionary<string, object> {
                    { "city", "Paris" },
                    { "pricePerNight", "150" },
                    { "nights", "3" },
                    { "owner", "TestOwner2" },
                    { "email", "test2@example.com" }
                }
            };

            // Act
            await _manager.DisplayUserReservations(reservations, _flowPanel);

            // Assert
            Assert.That(_flowPanel.Controls.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DisplayUserReservations_ValidReservation_CreatesCardWithCorrectData()
        {
            // Arrange
            var reservations = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "city", "London" },
                    { "pricePerNight", "100" },
                    { "nights", "2" },
                    { "owner", "TestOwner" },
                    { "email", "test@example.com" }
                }
            };

            // Act
            await _manager.DisplayUserReservations(reservations, _flowPanel);

            // Assert
            var card = _flowPanel.Controls[0] as Panel;
            var labels = card.Controls.OfType<Label>().ToList();

            Assert.That(labels.Any(l => l.Text == "City: London"), Is.True);
            Assert.That(labels.Any(l => l.Text == "Total price: £200.00"), Is.True);
            Assert.That(labels.Any(l => l.Text == "Host: TestOwner"), Is.True);
            Assert.That(labels.Any(l => l.Text == "Contact: test@example.com"), Is.True);
        }

        [Test]
        public void ReservationSelected_CardClicked_EventRaised()
        {
            // Arrange
            var eventRaised = false;
            var reservationData = new Dictionary<string, object>
            {
                { "city", "London" },
                { "pricePerNight", "100" }
            };

            _manager.ReservationSelected += (sender, data) =>
            {
                eventRaised = true;
                Assert.That(data, Is.EqualTo(reservationData));
            };

            var card = new Panel { Tag = reservationData };

            // Act
            var method = typeof(PropertyReservationManager).GetMethod(
                "reservationCard_Click",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            method.Invoke(_manager, new object[] { card, EventArgs.Empty });

            // Assert
            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public async Task CancelReservation_ValidReservation_CallsFirebaseDelete()
        {
            // Arrange
            var reservation = new Dictionary<string, object>
            {
                { "firebaseKey", "test-key" }
            };

            // Act
            await _manager.CancelReservation(reservation);

            // Assert
            Assert.That(_manager.GetLastDeletedKey(), Is.EqualTo("test-key"));
        }

        [Test]
        public void CancelReservation_MissingFirebaseKey_ThrowsException()
        {
            // Arrange
            var reservation = new Dictionary<string, object>
            {
                { "city", "London" }
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _manager.CancelReservation(reservation));
            Assert.That(ex.Message, Is.EqualTo("Could not find the reservation ID."));
        }
    }

    public class TestPropertyBookingManager : PropertyBookingManager
    {
        private readonly List<Dictionary<string, object>> _mockProperties;
        private readonly TaskScheduler _testScheduler;

        public TestPropertyBookingManager() : base(null)
        {
            _mockProperties = new List<Dictionary<string, object>>();
            _testScheduler = TaskScheduler.Default;

            // Override the UI task factory with one that uses the test scheduler
            typeof(PropertyBookingManager)
                .GetField("uiTaskFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, new TaskFactory(_testScheduler));
        }

        public void SetupMockProperties(List<Dictionary<string, object>> properties)
        {
            _mockProperties.Clear();
            _mockProperties.AddRange(properties);
        }

        public override async Task<List<Dictionary<string, object>>> GetAvailablePropertiesFromFirebase()
        {
            return _mockProperties;
        }

        public override async Task<List<Dictionary<string, object>>> SearchPropertiesByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return _mockProperties;

            return _mockProperties.Where(p =>
                p.ContainsKey("Address") &&
                ((Dictionary<string, object>)p["Address"])["City"].ToString().Equals(
                    char.ToUpper(city[0]) + city.Substring(1).ToLower(),
                    StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Override methods that use UI synchronization
        public override async Task InitiateDataLoading(Panel card, Dictionary<string, object> property)
        {
            // Simplified version for testing that doesn't require UI synchronization
            if (property.ContainsKey("Front Image"))
            {
                var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pictureBox != null)
                {
                    pictureBox.BackColor = Color.White;
                }
            }
        }

        public override void UpdateLoadingEffect(Panel card, bool isLight)
        {
            // Simplified version for testing
            var pictureBox = card.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null)
            {
                pictureBox.BackColor = Color.White;
            }
        }
    }

    [TestFixture]
    public class PropertyBookingManagerTests
    {
        private TestPropertyBookingManager _manager;
        private FlowLayoutPanel _flowPanel;
        private Form _testForm;

        [SetUp]
        public void Setup()
        {
            // Create a test form to host the controls
            _testForm = new Form();
            _flowPanel = new FlowLayoutPanel();
            _testForm.Controls.Add(_flowPanel);
            _manager = new TestPropertyBookingManager();
        }

        [TearDown]
        public void Cleanup()
        {
            if (_flowPanel != null)
            {
                _flowPanel.Controls.Clear();
                _flowPanel.Dispose();
            }
            if (_testForm != null)
            {
                _testForm.Dispose();
            }
        }

        [Test]
        public async Task GetAvailablePropertiesFromFirebase_ReturnsCorrectProperties()
        {
            // Arrange
            var testProperties = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "Name", "Host1" },
                    { "Email", "host1@example.com" },
                    { "PricePerNight", "100" },
                    { "Address", new Dictionary<string, object> { { "City", "London" } } }
                },
                new Dictionary<string, object> {
                    { "Name", "Host2" },
                    { "Email", "host2@example.com" },
                    { "PricePerNight", "150" },
                    { "Address", new Dictionary<string, object> { { "City", "Paris" } } }
                }
            };

            _manager.SetupMockProperties(testProperties);

            // Act
            var result = await _manager.GetAvailablePropertiesFromFirebase();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0]["Name"], Is.EqualTo("Host1"));
            Assert.That(result[1]["Name"], Is.EqualTo("Host2"));
        }

        [Test]
        public async Task DisplayAvailableProperties_CreatesCorrectNumberOfCards()
        {
            // Arrange
            var testProperties = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "Name", "Host1" },
                    { "Email", "host1@example.com" },
                    { "PricePerNight", "100" },
                    { "Address", new Dictionary<string, object> { { "City", "London" } } }
                },
                new Dictionary<string, object> {
                    { "Name", "Host2" },
                    { "Email", "host2@example.com" },
                    { "PricePerNight", "150" },
                    { "Address", new Dictionary<string, object> { { "City", "Paris" } } }
                }
            };

            // Act
            await _manager.DisplayAvailableProperties(testProperties, _flowPanel);

            // Assert
            Assert.That(_flowPanel.Controls.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DisplayAvailableProperties_WithTitle_CreatesCorrectLabels()
        {
            // Arrange
            var testProperties = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "Name", "Host1" },
                    { "Email", "host1@example.com" },
                    { "PricePerNight", "100" },
                    { "Address", new Dictionary<string, object> { { "City", "London" } } }
                }
            };

            // Act
            await _manager.DisplayAvailableProperties(testProperties, _flowPanel, "Test Title");

            // Assert
            var titleLabel = _flowPanel.Controls[0] as Label;
            Assert.That(titleLabel, Is.Not.Null);
            Assert.That(titleLabel.Text, Is.EqualTo("Test Title"));
        }

        [Test]
        public async Task SearchPropertiesByCity_ReturnsCorrectProperties()
        {
            // Arrange
            var testProperties = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "Name", "Host1" },
                    { "Address", new Dictionary<string, object> { { "City", "London" } } }
                },
                new Dictionary<string, object> {
                    { "Name", "Host2" },
                    { "Address", new Dictionary<string, object> { { "City", "Paris" } } }
                }
            };

            _manager.SetupMockProperties(testProperties);

            // Act
            var result = await _manager.SearchPropertiesByCity("London");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(((Dictionary<string, object>)result[0]["Address"])["City"], Is.EqualTo("London"));
        }

        [Test]
        public async Task SearchPropertiesByCity_CaseInsensitive_ReturnsCorrectProperties()
        {
            // Arrange
            var testProperties = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> {
                    { "Name", "Host1" },
                    { "Address", new Dictionary<string, object> { { "City", "London" } } }
                }
            };

            _manager.SetupMockProperties(testProperties);

            // Act
            var result = await _manager.SearchPropertiesByCity("london");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(((Dictionary<string, object>)result[0]["Address"])["City"], Is.EqualTo("London"));
        }

        [Test]
        public void PropertySelected_EventRaised_WhenCardClicked()
        {
            // Arrange
            var eventRaised = false;
            var propertyData = new Dictionary<string, object>
            {
                { "Name", "Host1" },
                { "Email", "host1@example.com" }
            };

            _manager.PropertySelected += (sender, data) =>
            {
                eventRaised = true;
                Assert.That(data, Is.EqualTo(propertyData));
            };

            var card = new Panel { Tag = propertyData };

            // Act
            // Use reflection to invoke the private CardClick method
            var method = typeof(PropertyBookingManager).GetMethod(
                "CardClick",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            method.Invoke(_manager, new object[] { card });

            // Assert
            Assert.That(eventRaised, Is.True);
        }
    }

    public class TestCityCardManager : CityCardManager
    {
        private readonly Dictionary<string, string> _mockCityImages;
        private bool _isImageLoaded;

        public TestCityCardManager() : base(null)
        {
            _mockCityImages = new Dictionary<string, string>();
            _isImageLoaded = false;
        }

        public void SetupMockCityImages(Dictionary<string, string> cityImages)
        {
            _mockCityImages.Clear();
            foreach (var kvp in cityImages)
            {
                _mockCityImages.Add(kvp.Key, kvp.Value);
            }
        }

        public bool GetImageLoadedStatus() => _isImageLoaded;

        // Override the LoadCityImages method to use mock data
        private async Task LoadCityImages()
        {
            // Simulate async operation
            await Task.Delay(1);
            return;
        }
    }

    [TestFixture]
    public class CityCardManagerTests
    {
        private TestCityCardManager _manager;
        private FlowLayoutPanel _flowPanel;
        private Form _testForm;
        private Button _prevButton;
        private Button _nextButton;

        [SetUp]
        public void Setup()
        {
            _testForm = new Form();
            _flowPanel = new FlowLayoutPanel();
            _prevButton = new Button();
            _nextButton = new Button();
            _testForm.Controls.AddRange(new Control[] { _flowPanel, _prevButton, _nextButton });
            _manager = new TestCityCardManager();
        }

        [TearDown]
        public void Cleanup()
        {
            _manager.Dispose();
            _flowPanel.Controls.Clear();
            _flowPanel.Dispose();
            _prevButton.Dispose();
            _nextButton.Dispose();
            _testForm.Dispose();
        }

        [Test]
        public async Task DisplayCityCards_CreatesSixCardsPerPage()
        {
            // Arrange
            var mockCityImages = new Dictionary<string, string>
            {
                { "London", "london.jpg" },
                { "Manchester", "manchester.jpg" },
                { "Birmingham", "birmingham.jpg" }
            };
            _manager.SetupMockCityImages(mockCityImages);

            // Act
            await _manager.DisplayCityCards(_flowPanel, _prevButton, _nextButton);

            // Assert
            Assert.That(_flowPanel.Controls.Count, Is.EqualTo(6));
        }

        [Test]
        public async Task DisplayCityCards_CreatesCardsWithCorrectProperties()
        {
            // Act
            await _manager.DisplayCityCards(_flowPanel);

            // Assert
            var firstCard = _flowPanel.Controls[0] as Panel;
            Assert.That(firstCard, Is.Not.Null);
            Assert.That(firstCard.Width, Is.EqualTo(250));
            Assert.That(firstCard.Height, Is.EqualTo(300));
            Assert.That(firstCard.BackColor, Is.EqualTo(Color.White));
            Assert.That(firstCard.Cursor, Is.EqualTo(Cursors.Hand));
        }

        [Test]
        public async Task NextPage_UpdatesControlsAndButtons()
        {
            // Arrange
            await _manager.DisplayCityCards(_flowPanel, _prevButton, _nextButton);
            var initialControls = _flowPanel.Controls.Cast<Control>().ToList();

            // Act
            _manager.NextPage();
            var newControls = _flowPanel.Controls.Cast<Control>().ToList();

            // Assert
            Assert.That(newControls, Is.Not.EqualTo(initialControls));
            Assert.That(_prevButton.Enabled, Is.True);
        }

        [Test]
        public async Task PreviousPage_UpdatesControlsAndButtons()
        {
            // Arrange
            await _manager.DisplayCityCards(_flowPanel, _prevButton, _nextButton);
            _manager.NextPage();
            var initialControls = _flowPanel.Controls.Cast<Control>().ToList();

            // Act
            _manager.PreviousPage();
            var newControls = _flowPanel.Controls.Cast<Control>().ToList();

            // Assert
            Assert.That(newControls, Is.Not.EqualTo(initialControls));
            Assert.That(_nextButton.Enabled, Is.True);
        }

        [Test]
        public void CitySelected_EventRaised_WhenCardClicked()
        {
            var eventRaised = false;
            var expectedCity = "London";

            _manager.CitySelected += (sender, city) =>
            {
                eventRaised = true;
                Assert.That(city, Is.EqualTo(expectedCity));
            };

            var eventField = typeof(CityCardManager).GetField("CitySelected",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            var eventDelegate = (EventHandler<string>)eventField.GetValue(_manager);
            eventDelegate?.Invoke(_manager, expectedCity);

            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public async Task DisplayCityCards_InitialPageHasPreviousButtonDisabled()
        {
            // Arrange & Act
            await _manager.DisplayCityCards(_flowPanel, _prevButton, _nextButton);

            // Assert
            Assert.That(_prevButton.Enabled, Is.False);
        }

        [Test]
        public async Task DisplayCityCards_LastPageHasNextButtonDisabled()
        {
            // Arrange
            await _manager.DisplayCityCards(_flowPanel, _prevButton, _nextButton);

            // Act - Navigate to last page
            while (_nextButton.Enabled)
            {
                _manager.NextPage();
            }

            // Assert
            Assert.That(_nextButton.Enabled, Is.False);
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _manager.Dispose();
                _manager.Dispose(); // Should not throw when called multiple times
            });
        }
    }

    [TestFixture]
    public class PaymentManagerTests
    {
        private PaymentManager _manager;
        private PaymentDetails _validDetails;

        [SetUp]
        public void Setup()
        {
            _manager = new PaymentManager();
            _validDetails = new PaymentDetails
            {
                FullName = "John Smith",
                CardNumber = "4111111111111111",
                ExpiryDate = "12/25",
                CVV = "123",
                AddressLine1 = "123 Test Street",
                City = "London",
                PostCode = "SW1A 1AA"
            };
        }

        [Test]
        public void ValidatePaymentDetails_ValidData_ReturnsTrue()
        {
            Assert.That(() => _manager.ValidatePaymentDetails(_validDetails), Is.True);
        }

        [Test]
        public void ValidatePaymentDetails_EmptyFields_ThrowsArgumentException()
        {
            var details = new PaymentDetails();
            var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(details));
            Assert.That(ex.Message, Is.EqualTo("All required fields must be filled out."));
        }

        [Test]
        public void ValidatePaymentDetails_InvalidFullName_ThrowsArgumentException()
        {
            _validDetails.FullName = "A";
            var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
            Assert.That(ex.Message, Is.EqualTo("Please enter a valid full name."));
        }

        [Test]
        public void ValidatePaymentDetails_InvalidCardNumber_ThrowsArgumentException()
        {
            string[] invalidCards = { "411111111111111", "41111111111111111", "abcd111111111111" };
            foreach (var card in invalidCards)
            {
                _validDetails.CardNumber = card;
                var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
                Assert.That(ex.Message, Is.EqualTo("Please enter a valid 16-digit card number."));
            }
        }

        [Test]
        public void ValidatePaymentDetails_InvalidExpiryDate_ThrowsArgumentException()
        {
            string[] invalidDates = { "13/25", "00/25", "12/19", "12-25" };
            foreach (var date in invalidDates)
            {
                _validDetails.ExpiryDate = date;
                var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
                Assert.That(ex.Message, Is.EqualTo("Please enter a valid expiry date in MM/YY format."));
            }
        }

        [Test]
        public void ValidatePaymentDetails_InvalidCVV_ThrowsArgumentException()
        {
            string[] invalidCVVs = { "12", "12345", "abc" };
            foreach (var cvv in invalidCVVs)
            {
                _validDetails.CVV = cvv;
                var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
                Assert.That(ex.Message, Is.EqualTo("Please enter a valid CVV number (3 or 4 digits)."));
            }
        }

        [Test]
        public void ValidatePaymentDetails_InvalidAddress_ThrowsArgumentException()
        {
            _validDetails.AddressLine1 = "1234";
            var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
            Assert.That(ex.Message, Is.EqualTo("Please enter a valid address."));
        }

        [Test]
        public void ValidatePaymentDetails_InvalidCity_ThrowsArgumentException()
        {
            _validDetails.City = "A";
            var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
            Assert.That(ex.Message, Is.EqualTo("Please enter a valid city name."));
        }

        [Test]
        public void ValidatePaymentDetails_InvalidPostCode_ThrowsArgumentException()
        {
            string[] invalidPostCodes = { "12345", "ABC 123", "123 ABC" };
            foreach (var postCode in invalidPostCodes)
            {
                _validDetails.PostCode = postCode;
                var ex = Assert.Throws<ArgumentException>(() => _manager.ValidatePaymentDetails(_validDetails));
                Assert.That(ex.Message, Is.EqualTo("Please enter a valid UK post code."));
            }
        }

        [Test]
        public void ProcessPayment_ValidDetails_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => _manager.ProcessPayment(_validDetails, 100.00m));
        }

        [Test]
        public void ProcessPayment_InvalidDetails_ThrowsArgumentException()
        {
            _validDetails.CardNumber = "invalid";
            Assert.Throws<ArgumentException>(() => _manager.ProcessPayment(_validDetails, 100.00m));
        }
    }

    [TestFixture]
    public class ThreadManagerTests
    {
        private ThreadManager _manager;
        private const int MAX_THREADS = 4;

        [SetUp]
        public void Setup()
        {
            _manager = new ThreadManager(MAX_THREADS);
        }

        [TearDown]
        public void Cleanup()
        {
            _manager.Dispose();
        }

        [Test]
        public void GetThread_ExecutesWork()
        {
            var manualResetEvent = new ManualResetEventSlim(false);
            bool workExecuted = false;

            _manager.GetThread(() => {
                workExecuted = true;
                manualResetEvent.Set();
            });

            Assert.That(manualResetEvent.Wait(1000), Is.True, "Work did not complete in time");
            Assert.That(workExecuted, Is.True);
        }

        [Test]
        public void GetThread_MultipleThreads_ExecutesAllWork()
        {
            var manualResetEvent = new ManualResetEventSlim(false);
            int completedCount = 0;
            int totalTasks = 10;

            for (int i = 0; i < totalTasks; i++)
            {
                _manager.GetThread(() => {
                    Interlocked.Increment(ref completedCount);
                    if (completedCount == totalTasks)
                        manualResetEvent.Set();
                });
            }

            Assert.That(manualResetEvent.Wait(1000), Is.True, "Work did not complete in time");
            Assert.That(completedCount, Is.EqualTo(totalTasks));
        }

        [Test]
        public void GetThread_AfterDispose_ThrowsObjectDisposedException()
        {
            _manager.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _manager.GetThread(() => { }));
        }

        [Test]
        public void WaitForAllThreads_WaitsForCompletion()
        {
            var manualResetEvent = new ManualResetEventSlim(false);
            int completedCount = 0;
            int totalTasks = 5;

            for (int i = 0; i < totalTasks; i++)
            {
                _manager.GetThread(() => {
                    Interlocked.Increment(ref completedCount);
                    if (completedCount == totalTasks)
                        manualResetEvent.Set();
                });
            }

            Assert.That(manualResetEvent.Wait(1000), Is.True, "Work did not complete in time");
            Assert.That(completedCount, Is.EqualTo(totalTasks));
        }

        [Test]
        public void GetThread_HandlesExceptions()
        {
            var manualResetEvent = new ManualResetEventSlim(false);
            bool exceptionThrown = false;

            _manager.GetThread(() => {
                exceptionThrown = true;
                manualResetEvent.Set();
                throw new Exception("Test exception");
            });

            Assert.That(manualResetEvent.Wait(1000), Is.True, "Work did not complete in time");
            Assert.That(exceptionThrown, Is.True);
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            Assert.DoesNotThrow(() => {
                _manager.Dispose();
                _manager.Dispose();
            });
        }

        [Test]
        public void Dispose_CancelsQueuedWork()
        {
            int completedCount = 0;
            for (int i = 0; i < 100; i++)
            {
                _manager.GetThread(() => {
                    Thread.Sleep(50);
                    Interlocked.Increment(ref completedCount);
                });
            }

            _manager.Dispose();
            Thread.Sleep(200);

            Assert.That(completedCount, Is.LessThan(100));
        }
    }

    [TestFixture]
    public class SelectedPropertyTests
    {
        private TestSelectedProperty _manager;
        private FlowLayoutPanel _flowPanel;
        private Form _testForm;

        public class TestSelectedProperty : SelectedProperty
        {
            private readonly List<string> _mockImages;
            private readonly Dictionary<string, object> _mockAddress;
            private Dictionary<string, object> _lastReservation;

            public TestSelectedProperty() : base(null)
            {
                _mockImages = new List<string>();
                _mockAddress = new Dictionary<string, object>();
            }

            public void SetupMockImages(List<string> images)
            {
                _mockImages.Clear();
                _mockImages.AddRange(images);
            }

            public void SetupMockAddress(Dictionary<string, object> address)
            {
                _mockAddress.Clear();
                foreach (var kvp in address)
                {
                    _mockAddress.Add(kvp.Key, kvp.Value);
                }
            }

            public Dictionary<string, object> GetLastReservation() => _lastReservation;

            public new async Task<Dictionary<string, object>> GetPropertyAddress(string username)
            {
                await Task.Delay(1);
                return _mockAddress;
            }

            public new async void AddReservationToDatabase(string customerName, string endDate, int nights,
            string startDate, Dictionary<string, object> propertyData, Dictionary<string, object> propertyAddress)
            {
                _lastReservation = new Dictionary<string, object>
                {
                    { "customerName", customerName },
                    { "endDate", endDate },
                    { "nights", nights },
                    { "startDate", startDate }
                };
                await Task.Delay(1);
            }
        }

        [SetUp]
        public void Setup()
        {
            _testForm = new Form();
            _flowPanel = new FlowLayoutPanel();
            _testForm.Controls.Add(_flowPanel);
            _manager = new TestSelectedProperty();
        }

        [TearDown]
        public void Cleanup()
        {
            _flowPanel.Controls.Clear();
            _flowPanel.Dispose();
            _testForm.Dispose();
        }

        [Test]
        public async Task GetPropertyAddress_ReturnsCorrectAddress()
        {
            // Arrange
            var mockAddress = new Dictionary<string, object>
        {
            { "Address", "123 Test St" },
            { "City", "London" },
            { "Description", "Test property" }
        };
            _manager.SetupMockAddress(mockAddress);

            // Act
            var result = await _manager.GetPropertyAddress("testUser");

            // Assert
            Assert.That(result, Is.EqualTo(mockAddress));
        }

        [Test]
        public void AddReservationToDatabase_StoresCorrectData()
        {
            // Arrange
            var customerName = "John Doe";
            var startDate = "2024-01-01";
            var endDate = "2024-01-05";
            var nights = 4;
            var propertyData = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "Name", "TestHost" },
            { "Front Image", "image.jpg" },
            { "PricePerNight", "100" }
        };
            var propertyAddress = new Dictionary<string, object>
        {
            { "Address", "123 Test St" },
            { "City", "London" },
            { "Description", "Test property" },
            { "Title", "Test Property" }
        };

            // Act
            _manager.AddReservationToDatabase(customerName, endDate, nights, startDate, propertyData, propertyAddress);

            // Assert
            var reservation = _manager.GetLastReservation();
            Assert.That(reservation["customerName"], Is.EqualTo(customerName));
            Assert.That(reservation["endDate"], Is.EqualTo(endDate));
            Assert.That(reservation["nights"], Is.EqualTo(nights));
            Assert.That(reservation["startDate"], Is.EqualTo(startDate));
        }

        [Test]
        public async Task DisplayPropertyDetails_HandlesEmptyImageList()
        {
            // Arrange
            _manager.SetupMockImages(new List<string>());
            var propertyData = new Dictionary<string, object> { { "Username", "testUser" } };

            // Act
            await _manager.DisplayPropertyDetails(propertyData, _flowPanel);

            // Assert
            Assert.That(_flowPanel.Controls.Count, Is.Zero);
        }
    }
}