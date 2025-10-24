namespace Ejercicios.Backend.Models
{
    public class ItemSeparator
    {
        public String Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

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

            if (!double.TryParse(parts[1].Trim(), out double price))
            {
                throw new ArgumentException("El precio debe ser un número válido", nameof(rawInput));
            }
            Price = price;

            if (!int.TryParse(parts[2].Trim(), out int quantity))
            {
                throw new ArgumentException("La cantidad debe ser un número válido", nameof(rawInput));
            }
            Quantity = quantity;
        }

        // Getters adicionales
        public String GetName() => Name;
        public double GetPrice() => Price;
        public int GetQuantity() => Quantity;

        // ToString para mostrar la info del item
        public override string ToString()
        {
            return $"Item Name: {Name}\nItem Price: {Price}\nItem Quantity: {Quantity}";
        }
    }

    // DTOs para API
    public class ItemRequest
    {
        public string RawInput { get; set; } = "";
    }

    public class ItemResponse
    {
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string FormattedOutput { get; set; } = "";
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    public class MultipleItemsRequest
    {
        public List<string> RawInputs { get; set; } = new List<string>();
    }

    public class MultipleItemsResponse
    {
        public List<ItemResponse> Items { get; set; } = new List<ItemResponse>();
        public int TotalItems { get; set; }
        public double TotalValue { get; set; }
        public string Summary { get; set; } = "";
    }
}