using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

public class InstagramUploader
{
    private readonly string _username;
    private readonly string _password;

    public InstagramUploader(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public void UploadVideo(string filePath, string caption)
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");

        using (var driver = new ChromeDriver(options))
        {
            try
            {
                driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");

                Thread.Sleep(2000); // Wait for the page to load

                // Login
                var usernameField = driver.FindElement(By.Name("username"));
                var passwordField = driver.FindElement(By.Name("password"));
                var loginButton = driver.FindElement(By.XPath("//button[@type='submit']"));

                usernameField.SendKeys(_username);
                passwordField.SendKeys(_password);
                loginButton.Click();

                Thread.Sleep(5000); // Wait for login to complete

                // Handle "Save Your Login Info?" popup if it appears
                try
                {
                    var saveInfoNotNowButton = driver.FindElement(By.XPath("//button[contains(text(), 'Not Now')]"));
                    saveInfoNotNowButton.Click();
                }
                catch (NoSuchElementException) { }

                Thread.Sleep(2000); // Wait for any popups to close

                // Handle "Turn on Notifications" popup if it appears
                try
                {
                    var notificationsNotNowButton = driver.FindElement(By.XPath("//button[contains(text(), 'Not Now')]"));
                    notificationsNotNowButton.Click();
                }
                catch (NoSuchElementException) { }

                Thread.Sleep(2000); // Wait for any popups to close

                // Go to upload page
                driver.Navigate().GoToUrl("https://www.instagram.com/create/style/");

                Thread.Sleep(2000); // Wait for the page to load

                // Upload video
                var fileInput = driver.FindElement(By.XPath("//input[@type='file']"));
                fileInput.SendKeys(filePath);

                Thread.Sleep(5000); // Wait for the video to upload

                // Add caption
                var captionField = driver.FindElement(By.XPath("//textarea[@aria-label='Write a caption…']"));
                captionField.SendKeys(caption);

                // Share post
                var shareButton = driver.FindElement(By.XPath("//button[contains(text(), 'Share')]"));
                shareButton.Click();

                Thread.Sleep(5000); // Wait for the video to be shared
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}