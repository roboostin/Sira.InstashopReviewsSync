using API.Helpers.Attributes;

namespace API.Shared.Models
{
    public class SelectEnumListDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}
