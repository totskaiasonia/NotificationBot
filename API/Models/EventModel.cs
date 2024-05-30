
using System.Text.Json;

namespace API.Models
{
    public class EventModel
    {
        public string? Title { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string UsersIds { get; set; } = JsonSerializer.Serialize(new List<string>());
        public string MaterialsUrls { get; set; } = JsonSerializer.Serialize(new List<string>());
    }
}
