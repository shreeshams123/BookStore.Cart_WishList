using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using System.Text.Json;

namespace BookStore.Cart_WishList.Services
{
    public class ExternalService:IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bookServiceUrl = "https://localhost:7183/api/book/";
        public ExternalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<BookDataDto> BookExists(int bookId)
        {
            Console.WriteLine($"Checking if book with ID {bookId} exists...");

            var response = await _httpClient.GetAsync($"{_bookServiceUrl}{bookId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received response: {content}");

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var bookResponse = JsonSerializer.Deserialize<BookResponseDto>(content, options);
                    Console.WriteLine($"Deserialized bookResponse: {JsonSerializer.Serialize(bookResponse)}");

                    if (bookResponse != null && bookResponse.Success)
                    {
                        Console.WriteLine($"Book found: {bookResponse.Data.Title}");
                        return bookResponse.Data;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to find book with ID {bookId}. Response was: {content}");
                        return null;
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing response: {jsonEx.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch book data. Status Code: {response.StatusCode}");
                return null;
            }
        }

    }
}
