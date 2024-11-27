using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace AirBnB
{
    internal static class Program
    {
        private static FirebaseClient firebaseClient; // Declare FirebaseClient to interact with the database
        private static CityPreloader cityPreloader;
        private static ImageLoader imageLoader;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeFirebase(); // Call to initialize Firebase on program start
            InitializePreloader();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new frmRegister();
            mainForm.FormClosed += (s, args) => {
                cityPreloader?.Stop();
                imageLoader?.Dispose();
                Application.Exit();
            };

            // Start preloading in the background
            Task.Run(async () =>
            {
                try
                {
                    await cityPreloader.Initialize();
                    await cityPreloader.StartPreloading(CityCardManager.AVAILABLE_CITIES);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during preloading: {ex.Message}");
                }
            });

            Application.Run(mainForm);
        }

        public static void InitializeFirebase()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize the Firebase app using the JSON key file
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(pathToJson)
            });

            // Initialize Firebase Realtime Database client
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private static void InitializePreloader()
        {
            // Note that we now only pass the citiesPerPage parameter
            cityPreloader = new CityPreloader(firebaseClient, 6);  // 6 cities per page

            cityPreloader.PageLoadComplete += (sender, pageIndex) =>
            {
                Console.WriteLine($"Finished preloading page {pageIndex + 1}");
            };
        }

        public static CityPreloader GetCityPreloader() => cityPreloader;
    }
}

