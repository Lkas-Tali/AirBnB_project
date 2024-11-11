using System;
using System.Text.RegularExpressions;

namespace AirBnB
{
    public class PaymentManager
    {
        // Regular expressions for validation
        private static readonly Regex CardNumberRegex = new Regex(@"^[0-9]{16}$");
        private static readonly Regex ExpiryDateRegex = new Regex(@"^(0[1-9]|1[0-2])\/([0-9]{2})$");
        private static readonly Regex CVVRegex = new Regex(@"^[0-9]{3,4}$");
        private static readonly Regex PostCodeRegex = new Regex(@"^[A-Z]{1,2}[0-9][A-Z0-9]? ?[0-9][A-Z]{2}$", RegexOptions.IgnoreCase);

        public bool ValidatePaymentDetails(PaymentDetails details)
        {
            try
            {
                // Check if any field is null or empty
                if (string.IsNullOrWhiteSpace(details.FullName) ||
                    string.IsNullOrWhiteSpace(details.CardNumber) ||
                    string.IsNullOrWhiteSpace(details.ExpiryDate) ||
                    string.IsNullOrWhiteSpace(details.CVV) ||
                    string.IsNullOrWhiteSpace(details.AddressLine1) ||
                    string.IsNullOrWhiteSpace(details.City) ||
                    string.IsNullOrWhiteSpace(details.PostCode))
                {
                    throw new ArgumentException("All required fields must be filled out.");
                }

                // Validate full name
                if (details.FullName.Length < 2 || details.FullName.Length > 100)
                {
                    throw new ArgumentException("Please enter a valid full name.");
                }

                // Validate card number format
                if (!CardNumberRegex.IsMatch(details.CardNumber))
                {
                    throw new ArgumentException("Please enter a valid 16-digit card number.");
                }

                // Validate expiry date format and check if not expired
                if (!ValidateExpiryDate(details.ExpiryDate))
                {
                    throw new ArgumentException("Please enter a valid expiry date in MM/YY format.");
                }

                // Validate CVV
                if (!CVVRegex.IsMatch(details.CVV))
                {
                    throw new ArgumentException("Please enter a valid CVV number (3 or 4 digits).");
                }

                // Validate address
                if (details.AddressLine1.Length < 5)
                {
                    throw new ArgumentException("Please enter a valid address.");
                }

                // Validate city
                if (details.City.Length < 2)
                {
                    throw new ArgumentException("Please enter a valid city name.");
                }

                // Validate post code
                if (!PostCodeRegex.IsMatch(details.PostCode))
                {
                    throw new ArgumentException("Please enter a valid UK post code.");
                }

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while validating payment details.", ex);
            }
        }

        private bool ValidateExpiryDate(string expiryDate)
        {
            if (!ExpiryDateRegex.IsMatch(expiryDate))
                return false;

            var parts = expiryDate.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]) + 2000; // Convert YY to YYYY

            var cardExpiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return cardExpiry > DateTime.Now;
        }

        public void ProcessPayment(PaymentDetails details, decimal amount)
        {
            // This is where you would integrate with a real payment gateway
            // For now, we'll just simulate a successful payment

            // In a real implementation, you would:
            // 1. Connect to payment gateway
            // 2. Encrypt sensitive data
            // 3. Send payment request
            // 4. Handle response
            // 5. Log transaction

            // For demonstration, we'll just validate the details
            ValidatePaymentDetails(details);
        }
    }

    public class PaymentDetails
    {
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
    }
}