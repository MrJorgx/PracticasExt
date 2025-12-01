using System.Globalization;

namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// Clase para procesar y separar elementos con formato especificado ItemName$$##Price$$##Quantity
    /// </summary>
    public class ItemSeparator
    {
        /// <summary>
        /// Nombre del item extraido
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Precio del item extraido
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Cantidad del item extraido
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Constructor que procesa la cadena entrada y extraesus componentes
        /// </summary>
        /// <param name="rawInput"></param>
        /// <exception cref="ArgumentException"></exception>
        public ItemSeparator(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                throw new ArgumentException("La entrada no puede ser vacía", nameof(rawInput));
            }

            // Separar la cadena en las tres partes usando el delimitador $$##
            string[] parts = rawInput.Split(new string[] { "$$##" }, StringSplitOptions.None);
        
            if (parts.Length != 3)
            {
                throw new ArgumentException("El formato no es el indicado, debe ser: ItemName$$##ItermPrice$$##ItemQuantity", nameof(rawInput));
            }

            // Asignar y validar cada parte
            Name = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("El nombre no puede estar vacío", nameof(rawInput));
            }

            string priceText = parts[1].Trim();
            if (priceText.Contains('.'))
            {
                throw new ArgumentException("El precio debe usar coma como separador decimal", nameof(rawInput));
            }
            
            string priceForParsing = priceText.Replace(',', '.');
            if (!double.TryParse(priceForParsing, NumberStyles.Float, CultureInfo.InvariantCulture, out double price))
            {
                throw new ArgumentException("El precio debe ser un número válido con coma decimal", nameof(rawInput));
            }
            Price = price;

            if (!int.TryParse(parts[2].Trim(), out int quantity))
            {
                throw new ArgumentException("La cantidad debe ser un número válido", nameof(rawInput));
            }
            Quantity = quantity;
        }

        /// <summary>
        /// Obtiene el nombre del item
        /// </summary>
        /// <returns>Nombre del item</returns>
        public String GetName() => Name;

        /// <summary>
        /// Obtiene el precio del item
        /// </summary>
        /// <returns>Precio del item</returns>
        public double GetPrice() => Price;

        /// <summary>
        /// Obtiene la cantidad del item
        /// </summary>
        /// <returns>Cantidad del item</returns>
        public int GetQuantity() => Quantity;

        /// <summary>
        /// Convierte la información del item a una representación en cadena formateada
        /// </summary>
        /// <returns>Cadena con la información completa del item</returns>
        public override string ToString()
        {
            return $"Item Name: {Name}\nItem Price: {Price}\nItem Quantity: {Quantity}";
        }
    }

    /// <summary>
    /// DTO para las solicitudes de procesamiento de un solo item
    /// </summary>
    public class ItemRequest
    {
        /// <summary>
        /// Cadena de entrada sin procesar con formato ItemName$$##Price$$##Quantity
        /// </summary>
        public string RawInput { get; set; } = "";
    }

    /// <summary>
    /// DTO para las respuestas de procesamiento de item
    /// </summary>
    public class ItemResponse
    {
        /// <summary>
        /// Nombre del item extraido
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Precio del item extraido
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Cantidad del item extraido
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Salida formateada del item procesado
        /// </summary>
        public string FormattedOutput { get; set; } = "";

        /// <summary>
        /// Indica si el procesamiento fue exitoso
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje de error en caso de fallo
        /// </summary>
        public string ErrorMessage { get; set; } = "";
    }

    /// <summary>
    /// DTO para las solicitudes de procesamiento de múltiples items
    /// </summary>
    public class MultipleItemsRequest
    {
        /// <summary>
        /// Lista de items procesados (exitosos y fallidos)
        /// </summary>
        public List<string> RawInputs { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para las respuestas de procesamiento múltiple con resumen
    /// </summary>
    public class MultipleItemsResponse
    {
        /// <summary>
        /// Lista de items procesados
        /// </summary>
        public List<ItemResponse> Items { get; set; } = new List<ItemResponse>();

        /// <summary>
        /// Número total de items procesados
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Valor total de todos los items válidos (precio * cantidad)
        /// </summary>
        public double TotalValue { get; set; }

        /// <summary>
        /// Resumen textual del procesamiento
        /// </summary>
        public string Summary { get; set; } = "";
    }
}