using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace WebApi.Services
{
    public class HubSpotAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _accessToken;

        public HubSpotAuthService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        public string GetAuthorizationUrl()
        {
            var clientId = _configuration["HubSpot:ClientId"];
            var redirectUri = _configuration["HubSpot:RedirectUri"];
            var scope = "contacts";

            var authorizationUrl = $"https://app.hubspot.com/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}";
            return authorizationUrl;
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            var clientId = _configuration["HubSpot:ClientId"];
            var clientSecret = _configuration["HubSpot:ClientSecret"];
            var redirectUri = _configuration["HubSpot:RedirectUri"];

            var url = "https://api.hubapi.com/oauth/v1/token";
            var content = new StringContent(
                $"grant_type=authorization_code&client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUri}&code={code}",
                Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error getting access token: {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<HubSpotTokenResponse>(json);
            _accessToken = tokenResponse.AccessToken;
            return tokenResponse.AccessToken;
        }

        public async Task<List<Contact>> GetContactsAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new Exception("Access token is not set");
            }

            var url = "https://api.hubapi.com/contacts/v1/lists/all/contacts/all";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error getting contacts: {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var contactsResponse = JsonSerializer.Deserialize<ContactsResponse>(json);

            return contactsResponse.Contacts;
        }

        public class HubSpotTokenResponse
        {
            public required string AccessToken { get; set; }
            public required string RefreshToken { get; set; }
        }

        public class ContactsResponse
        {
            public List<Contact> Contacts { get; set; }
        }

        public class Contact
        {
            public string Vid { get; set; }
            public Dictionary<string, PropertyValue> Properties { get; set; }
        }

        public class PropertyValue
        {
            public string Value { get; set; }
        }
    }
}
