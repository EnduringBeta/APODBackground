// This program grabs the current Astronomy Picture of the Day and makes it the wallpaper.
// Currently it only tries to download the first displayed image in the source code, which should be the APOD in most cases.
// Though when the feature is a YouTube video, it will not change the wallpaper.
// This program will place the background image in "C:\TEMP\" as "Background.XXX".
// The code checks to see if "src" appears before the image URL to see if it is actually displayed.

// TODO: Write code to scan through many days for potential issues
// TODO: Test other OSes

// By Ross Llewallyn

using System;
using System.Net;
using System.Runtime.InteropServices;

namespace APODBackground
{
    class APODBackground
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static UInt32 SPI_SETDESKWALLPAPER = 20;
        private static UInt32 SPIF_UPDATEINIFILE = 0x1;

        static void Main(string[] args)
        {
            // Initialize Background image directory and filename
            string backgroundDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\";
            string backgroundFilename  = "Background";

            // Create local image directory/filename
            string localFilename = string.Concat(backgroundDirectory, backgroundFilename);

            // Keep track of how many images found
            int numImages = 0;

            // Initialize raw data variable
            string rawData = "";

            string protocol = "https://";

            // URL for Astronomy Picture of the Day (APOD)
            string URL = protocol + "apod.nasa.gov/";
            
            // Get current APOD HTML file
            Console.WriteLine("Accessing current Astronomy Picture of the Day webpage...");

            // Set up web client
            // Thank you, nqynik! (http://stackoverflow.com/questions/34945002/the-request-was-aborted-could-not-create-ssl-tls-secure-channel-system-net-webe)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var myClient = new System.Net.WebClient();
            try
            {
                rawData = myClient.DownloadString(URL);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Website could not be accessed: " + e.Message);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }

            // Debug - Display downloaded HTML file
            //Console.Write(rawData);
            //Console.ReadKey(true);

            string[] searchExts = { ".jpg", ".jpeg", ".JPG", ".JPEG", ".png", ".PNG" };
            string imageExt;
            int i;

            // Begin image search
            for (i = 0; i < searchExts.Length; i++)
            {
                // Apply new extension to search for
                imageExt = searchExts[i];

                // Find location of end of first image name
                int imageEnd = rawData.IndexOf(imageExt);

                // While we keep finding new images
                while (imageEnd != -1)
                {
                    Console.WriteLine();
                    Console.WriteLine("Image URL found...");

                    // Find location of beginning of image
                    int imageStart = rawData.Substring(0, imageEnd).LastIndexOf("\"");

                    // Get image name
                    string imageName = rawData.Substring((imageStart + 1), (imageEnd - imageStart + imageExt.Length - 1));

                    // If the image URL found is the one displayed
                    if (rawData.Substring(imageStart - 10, 10).ToLower().Contains("src"))
                    {
                        string imageURL;

                        // This section may not be full-proof. But I haven't seen this situation in an APOD yet.
                        // If local reference, make full URL
                        if (!imageName.Contains(protocol))
                            imageURL = string.Concat(URL, imageName);
                        // If already direct reference, leave alone
                        else
                            imageURL = imageName;

                        Console.WriteLine();
                        Console.WriteLine("Downloading image...");

                        // Download raw image
                        try
                        {
                            myClient.DownloadFile(imageURL, localFilename + imageExt);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Download failed: " + e.Message);

                            Console.WriteLine();
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey(true);
                            return;
                        }

                        // Download successful!

                        // Set as background
                        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, localFilename + imageExt, SPIF_UPDATEINIFILE);

                        Console.WriteLine();
                        Console.WriteLine("Done! Set as wallpaper.");

                        Console.WriteLine();
                        Console.WriteLine("Filename: {0}", imageURL);

                        Console.WriteLine();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                        return;
                    }
                    // Image URL not the one displayed
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Image not the APOD...");

                        numImages++;

                        // Find location of end of next image name, starting search at previous image
                        int newImageEnd = imageEnd + rawData.Substring(imageEnd+1).IndexOf(imageExt) + 1;

                        // If no more images, must break out. Otherwise, loop again and check new potential.
                        if (newImageEnd == imageEnd)
                            break;
                        else
                            imageEnd = newImageEnd;
                    }
                }
                // Tried all images of this extension, none worked.
            }
            // Tried all images and extensions, none worked.
            Console.WriteLine();
            Console.WriteLine("No displayed image found as the APOD. (Is today's a YouTube video?)");
            if (numImages == 1)
                Console.WriteLine("1 image link checked");
            else
                Console.WriteLine("{0} image links checked", numImages);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
    }
}
