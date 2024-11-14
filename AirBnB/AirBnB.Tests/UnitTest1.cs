using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using System.Windows.Forms;
using System.IO;

namespace AirBnB.Tests
{
    [TestClass]
    public class PaymentManagerTests
    {
        private PaymentManager _paymentManager;
        private PaymentDetails _validPaymentDetails;

        [TestInitialize]
        public void Setup()
        {
            _paymentManager = new PaymentManager();
            _validPaymentDetails = new PaymentDetails
            {
                FullName = "John Smith",
                CardNumber = "1234567890123456",
                ExpiryDate = "12/25",
                CVV = "123",
                AddressLine1 = "123 Test Street",
                AddressLine2 = "Apt 4B",
                City = "London",
                PostCode = "SW1A 1AA"
            };
        }

        [TestMethod]
        public void ValidatePaymentDetails_WithValidDetails_ReturnsTrue()
        {
            bool result = _paymentManager.ValidatePaymentDetails(_validPaymentDetails);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidatePaymentDetails_WithInvalidCardNumber_ThrowsException()
        {
            _validPaymentDetails.CardNumber = "123";
            _paymentManager.ValidatePaymentDetails(_validPaymentDetails);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidatePaymentDetails_WithExpiredCard_ThrowsException()
        {
            _validPaymentDetails.ExpiryDate = "01/20";
            _paymentManager.ValidatePaymentDetails(_validPaymentDetails);
        }
    }

    [TestClass]
    public class ListPropertyManagerTests
    {
        private ListPropertyManager _listPropertyManager;
        private FirebaseClient _firebaseClient;

        [TestInitialize]
        public void Setup()
        {
            _firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
            _listPropertyManager = new ListPropertyManager(_firebaseClient);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UploadProperty_WithEmptyAddress_ThrowsException()
        {
            await _listPropertyManager.UploadProperty(
                "testUser",
                "", // Empty address
                "London",
                "Nice House",
                "100",
                "Description"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UploadProperty_WithEmptyCity_ThrowsException()
        {
            await _listPropertyManager.UploadProperty(
                "testUser",
                "123 Street",
                "", // Empty city
                "Nice House",
                "100",
                "Description"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UploadProperty_WithEmptyTitle_ThrowsException()
        {
            await _listPropertyManager.UploadProperty(
                "testUser",
                "123 Street",
                "London",
                "", // Empty title
                "100",
                "Description"
            );
        }
    }

    [TestClass]
    public class RegistrationValidationTests
    {
        private frmRegister _register;

        [TestInitialize]
        public void Setup()
        {
            _register = new frmRegister();
        }

        [TestMethod]
        public void IsValidEmail_WithValidEmail_ReturnsTrue()
        {
            bool result = frmRegister.IsValidEmail("test@example.com");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidEmail_WithInvalidEmail_ReturnsFalse()
        {
            bool result = frmRegister.IsValidEmail("invalid-email");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_WithValidPassword_ReturnsTrue()
        {
            bool result = frmRegister.IsValidPassword("Password123");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidPassword_WithNoUppercase_ReturnsFalse()
        {
            bool result = frmRegister.IsValidPassword("password123");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_WithNoNumber_ReturnsFalse()
        {
            bool result = frmRegister.IsValidPassword("Password");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPassword_TooShort_ReturnsFalse()
        {
            bool result = frmRegister.IsValidPassword("Pass1");
            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class SelectedPropertyTests
    {
        private SelectedProperty _selectedProperty;
        private FirebaseClient _firebaseClient;

        [TestInitialize]
        public void Setup()
        {
            _firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
            _selectedProperty = new SelectedProperty(_firebaseClient);
        }

        [TestMethod]
        public void AddReservationToDatabase_ValidData_NoExceptionThrown()
        {
            // Arrange
            string customerName = "Test User";
            string endDate = "01/01/2025";
            int nights = 3;
            string startDate = "29/12/2024";

            var propertyData = new Dictionary<string, object>
            {
                { "Email", "host@example.com" },
                { "Front Image", "http://example.com/image.jpg" },
                { "Name", "Host Name" },
                { "PricePerNight", "100" }
            };

            var propertyAddress = new Dictionary<string, object>
            {
                { "Address", "123 Test St" },
                { "City", "London" },
                { "Description", "Nice place" },
                { "Title", "Cozy Home" }
            };

            // Act & Assert
            try
            {
                _selectedProperty.AddReservationToDatabase(
                    customerName, endDate, nights, startDate,
                    propertyData, propertyAddress);
                Assert.IsTrue(true); // If we get here, no exception was thrown
            }
            catch
            {
                Assert.Fail("Method threw an exception unexpectedly");
            }
        }
    }

    [TestClass]
    public class PropertyBookingManagerTests
    {
        private PropertyBookingManager _bookingManager;
        private FirebaseClient _firebaseClient;

        [TestInitialize]
        public void Setup()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            _firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
            _bookingManager = new PropertyBookingManager(_firebaseClient);
        }

        [TestMethod]
        public async Task SearchPropertiesByCity_WithEmptyCity_ReturnsAllProperties()
        {
            // Act
            var results = await _bookingManager.SearchPropertiesByCity("");

            // Assert
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task SearchPropertiesByCity_WithValidCity_ReturnsFilteredResults()
        {
            // Act
            var results = await _bookingManager.SearchPropertiesByCity("London");

            // Assert
            Assert.IsNotNull(results);
        }
    }

    [TestClass]
    public class ListedPropertyViewerTests
    {
        private ListedPropertyViewer _propertyViewer;
        private FirebaseClient _firebaseClient;

        [TestInitialize]
        public void Setup()
        {
            _firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
            _propertyViewer = new ListedPropertyViewer(_firebaseClient);
        }

        [TestMethod]
        public async Task GetImageUrlsFromFirebase_WithValidUsername_ReturnsUrls()
        {
            var results = await _propertyViewer.GetImageUrlsFromFirebase("zach");
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public void DisplayImages_WithEmptyImageList_ClearsFlowPanel()
        {
            // Arrange
            var flowPanel = new FlowLayoutPanel();
            flowPanel.Controls.Add(new PictureBox());
            var imageUrls = new List<string>();

            // Act
            _propertyViewer.DisplayImages(imageUrls, flowPanel);

            // Assert
            Assert.AreEqual(0, flowPanel.Controls.Count, "FlowPanel should be cleared when empty image list is provided");
        }

        [TestMethod]
        public void DisplayImages_WithValidImageUrls_AddsPictureBoxesToPanel()
        {
            // Arrange
            var flowPanel = new FlowLayoutPanel();
            var imageUrls = new List<string> { "https://firebasestorage.googleapis.com/v0/b/airbnb-d4964.appspot.com/o/Jake%2Ffront%20image%2FMP_oct22_0617.jpg?alt=media&token=49d4f2a3-7d10-420d-9a89-04574770d48e", "https://images.unsplash.com/photo-1625882979709-34f79ca4bb96?q=80&w=3087&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" };

            // Act
            _propertyViewer.DisplayImages(imageUrls, flowPanel);

            // Assert
            Assert.AreEqual(imageUrls.Count, flowPanel.Controls.Count, "Should add one PictureBox per image URL");
            Assert.IsInstanceOfType(flowPanel.Controls[0], typeof(PictureBox), "Controls should be PictureBoxes");
        }
    }

    [TestClass]
    public class PropertyReservationManagerTests
    {
        private PropertyReservationManager _reservationManager;
        private FirebaseClient _firebaseClient;

        [TestInitialize]
        public void Setup()
        {
            _firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
            _reservationManager = new PropertyReservationManager(_firebaseClient);
        }

        [TestMethod]
        public async Task RetrieveUserReservationDetails_WithValidUsername_ReturnsReservations()
        {
            var results = await _reservationManager.RetrieveUserReservationDetails("testUser");
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CancelReservation_WithValidReservation_NoExceptionThrown()
        {
            var reservation = new Dictionary<string, object>
            {
                { "firebaseKey", "test-reservation-id" }
            };

            try
            {
                await _reservationManager.CancelReservation(reservation);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Method threw an exception unexpectedly");
            }
        }
    }
}