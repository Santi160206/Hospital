using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FrontEndBlazor.Models
{
    public class Medicamentos
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre comercial es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        [JsonPropertyName("nombre_comercial")]
        public string? NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El principio activo es obligatorio")]
        [StringLength(250, ErrorMessage = "El principio activo no puede tener más de 250 caracteres")]
        [JsonPropertyName("principio_activo")]
        public string? Principio_Activo { get; set; } = string.Empty;

        [JsonPropertyName("presentacion")]
        public string? Presentacion { get; set; } = string.Empty;

        [JsonPropertyName("unidad_medida")]
        public string? Unidad_Medida { get; set; } = string.Empty;

        [JsonPropertyName("precio_unitario")]
        public decimal Precio_Unitario { get; set; }
    }
}
