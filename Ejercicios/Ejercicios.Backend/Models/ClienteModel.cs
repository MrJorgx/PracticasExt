using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// 
    /// </summary>
    public enum TipoCliente
    {
        REGISTRADO,
        SOCIO
    }

    // Validar DNI
    public class ValidarDniAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            string dni = value.ToString()!.Trim().ToUpper();

            var patron = @"^[0-9]{8}[A-Z]$";
            return Regex.IsMatch(dni, patron);
        }

        public override string FormatErrorMessage(string name)
        {
            return "El DNI debe tener 8 dígitos y una letra.";
        }
    }

    // Validar NIE
    public class ValidarNieAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            string nie = value.ToString()!.Trim().ToUpper();

            var patron = @"^[XYZ][0-9]{7}[A-Z]$";
            return Regex.IsMatch(nie, patron);
        }

        public override string FormatErrorMessage(string name)
        {
            return "El NIE debe tener 1 letra (X, Y o Z), 7 dígitos y una letra.";
        }
    }

    public class Cliente
    {
        public string Dni { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public TipoCliente TipoCliente { get; set; }
        public decimal? CuotaMaxima { get; set; }
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual ICollection<Recibo> Recibos { get; set; } = new List<Recibo>();
    }

    public class Recibo
    {
        public string NumeroRecibo { get; set; } = "";
        public string DniCliente { get; set; } = "";
        public decimal Importe { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual Cliente Cliente { get; set; } = null!;
    }

    // DTOs para las requests/responses
    public class ClienteRequest
    {
        [Required(ErrorMessage = "El DNI es requerido")]
        [ValidarDni]
        public string Dni { get; set; } = "";

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos deben tener máximo 100 caracteres")]
        public string Apellidos { get; set; } = "";

        [Required(ErrorMessage = "El tipo de cliente es requerido")]
        public string TipoCliente { get; set; } = "";

        public decimal? CuotaMaxima { get; set; }
    }

    public class ClienteUpdateRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos deben tener máximo 100 caracteres")]
        public string Apellidos { get; set; } = "";

        [Required(ErrorMessage = "El tipo de cliente es requerido")]
        public string TipoCliente { get; set; } = "";

        public decimal? CuotaMaxima { get; set; }
    }

    public class ClienteResponse
    {
        public string Dni { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string TipoCliente { get; set; } = "";
        public decimal? CuotaMaxima { get; set; }
        public DateTime FechaAlta { get; set; }
        public int TotalRecibos { get; set; }
    }

    public class ReciboRequest
    {
        [Required(ErrorMessage = "El número de recibo es requerido")]
        [StringLength(50, ErrorMessage = "El número de recibo debe tener máximo 50 caracteres")]
        public string NumeroRecibo { get; set; } = "";

        [Required(ErrorMessage = "El DNI del cliente es requerido")]
        [ValidarDni]
        public string DniCliente { get; set; } = "";

        [Required(ErrorMessage = "El importe es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor que 0")]
        public decimal Importe { get; set; }

        public DateTime? FechaEmision { get; set; }
    }

    public class ReciboUpdateRequest
    {
        [Required(ErrorMessage = "El importe es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor que 0")]
        public decimal Importe { get; set; }

        public DateTime? FechaEmision { get; set; }
    }

    public class ReciboResponse
    {
        public string NumeroRecibo { get; set; } = "";
        public string DniCliente { get; set; } = "";
        public string NombreCliente { get; set; } = "";
        public decimal Importe { get; set; }
        public DateTime FechaEmision { get; set; }
    }
}