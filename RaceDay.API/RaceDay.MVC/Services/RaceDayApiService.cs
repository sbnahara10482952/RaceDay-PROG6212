using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace RaceDay.MVC.Services
{
    public class RaceDayApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public RaceDayApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Message)> LoginAsync(
            string email,
            string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                var response = await client.PostAsJsonAsync(
                    "api/auth/login",
                    new
                    {
                        Email = email,
                        Password = password
                    });

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? "Login failed. Please check your email and password."
                            : content);
                }

                using var document = JsonDocument.Parse(content);

                var data = document.RootElement;

                int userId = data.GetProperty("userId").GetInt32();
                string role = data.GetProperty("role").GetString() ?? "";

                // Capture the API session cookie from the login response.
                string? sessionCookie = null;

                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        if (cookie.StartsWith(
                            ".AspNetCore.Session=",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            sessionCookie = cookie.Split(';')[0];
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(sessionCookie))
                {
                    return (
                        false,
                        "Login succeeded, but the API session cookie was not received. Check the API session configuration.");
                }

                var context = _httpContextAccessor.HttpContext;

                if (context == null)
                {
                    return (false, "The current login session could not be accessed.");
                }

                // Save the API cookie and user information in the MVC session.
                context.Session.SetString("ApiSessionCookie", sessionCookie);
                context.Session.SetInt32("UserID", userId);
                context.Session.SetString("Role", role);

                return (true, "Login successful.");
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.");
            }
            catch (JsonException)
            {
                return (
                    false,
                    "The API returned an unexpected response during login.");
            }
        }
        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var context = _httpContextAccessor.HttpContext;

            if (context != null &&
                context.Session.TryGetValue("ApiSessionCookie", out var cookieBytes))
            {
                var cookieValue = System.Text.Encoding.UTF8.GetString(cookieBytes);

                if (!string.IsNullOrWhiteSpace(cookieValue))
                {
                    client.DefaultRequestHeaders.Remove("Cookie");
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Cookie",
                        cookieValue);
                }
            }

            return client;
        }

        public async Task<(bool Success, string Message, JsonElement? Data)>
            PostAsync<T>(string endpoint, T request)
        {
            try
            {
                var client = CreateClient();

                var response = await client.PostAsJsonAsync(endpoint, request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"API request failed: {(int)response.StatusCode}."
                            : content,
                        null);
                }

                JsonElement? data = null;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    data = JsonDocument.Parse(content).RootElement.Clone();
                }

                return (true, string.Empty, data);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.",
                    null);
            }
        }

        public async Task<(bool Success, string Message, JsonElement? Data)>
            GetAsync(string endpoint)
        {
            try
            {
                var client = CreateClient();

                var response = await client.GetAsync(endpoint);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"API request failed: {(int)response.StatusCode}."
                            : content,
                        null);
                }

                JsonElement? data = null;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    data = JsonDocument.Parse(content).RootElement.Clone();
                }

                return (true, string.Empty, data);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.",
                    null);
            }
        }

        public async Task<(bool Success, string Message, JsonElement? Data)>
        UploadFileAsync(
            string endpoint,
            Stream fileStream,
            string fileName,
            string contentType)
        {
            try
            {
                var client = CreateClient();

                using var form = new MultipartFormDataContent();

                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(contentType);

                form.Add(fileContent, "file", fileName);

                var response = await client.PostAsync(endpoint, form);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"Image upload failed: {(int)response.StatusCode}."
                            : content,
                        null);
                }

                JsonElement? data = null;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    data = JsonDocument.Parse(content).RootElement.Clone();
                }

                return (true, string.Empty, data);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.",
                    null);
            }
        }

        public async Task<(bool Success, string Message, byte[]? ImageData, string? ContentType)>
            GetImageAsync(string endpoint)
        {
            try
            {
                var client = CreateClient();

                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"Unable to retrieve image: {(int)response.StatusCode}."
                            : content,
                        null,
                        null);
                }

                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                string? contentType = response.Content.Headers.ContentType?.MediaType;

                return (
                    true,
                    string.Empty,
                    imageData,
                    contentType);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.",
                    null,
                    null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phoneNumber)
        {
            var result = await PostAsync(
                "api/auth/register",
                new
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password,
                    PhoneNumber = phoneNumber
                });

            return (result.Success, result.Message);
        }

        public async Task<(bool Success, string Message, JsonElement? Data)>
            PutAsync<T>(string endpoint, T request)
        {
            try
            {
                var client = CreateClient();

                var response = await client.PutAsJsonAsync(endpoint, request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"API request failed: {(int)response.StatusCode}."
                            : content,
                        null);
                }

                JsonElement? data = null;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    data = JsonDocument.Parse(content).RootElement.Clone();
                }

                return (true, string.Empty, data);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.",
                    null);
            }
        }

        public async Task<(bool Success, string Message)>
            DeleteAsync(string endpoint)
        {
            try
            {
                var client = CreateClient();

                var response = await client.DeleteAsync(endpoint);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        false,
                        string.IsNullOrWhiteSpace(content)
                            ? $"API request failed: {(int)response.StatusCode}."
                            : content);
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException)
            {
                return (
                    false,
                    "Could not connect to the RaceDay API. Make sure the API is running.");
            }
        }



        public async Task LogoutAsync()
        {
            try
            {
                var client = CreateClient();

                await client.PostAsync("api/auth/logout", null);
            }
            catch (HttpRequestException)
            {
                // Clear the MVC session even if the API is unavailable.
            }
        }
    }
}
