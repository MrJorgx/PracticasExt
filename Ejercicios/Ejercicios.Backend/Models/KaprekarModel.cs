namespace Ejercicios.Backend.Models
{
    public class KaprekarRequest
    {
        public int Numero { get; set; }
        public string MetodoCalculo { get; set; } = "OPTIMIZADO";   // OPTIMIZADO, FUERZA_BRUTA, MATEMATICO
    }

    public class KaprekarResponse
    {
        public int Numero { get; set; }
        public bool EsKaprekar { get; set; }
        public string MetodoUtilizacion { get; set; } = "";
        public int NumeroOperaciones { get; set; }
        public long Cuadrado { get; set; }
        public string Descomposicion { get; set; } = "";
        public string Explicacion { get; set; } = "";
        public List<string> PasosCalculo { get; set; } = new List<string>();
    }

    // Clase auxiliar para cálculos
    public static class KaprekarCalculator
    {
        public static KaprekarResponse CalcularKaprekar(int numero, string metodo)
        {
            var response = new KaprekarResponse
            {
                Numero = numero,
                MetodoUtilizacion = metodo,
                PasosCalculo = new List<string>()
            };

            try
            {
                response.PasosCalculo.Add($"Verificando si {numero} es un número Kaprekar");

                switch (metodo.ToUpper())
                {
                    case "OPTIMIZADO":
                        CalcularOptimizado(response);
                        break;
                    case "FUERZA_BRUTA":
                        CalcularFuerzaBruta(response);
                        break;
                    case "MATEMATICO":
                        CalcularMatematico(response);
                        break;
                    default:
                        CalcularOptimizado(response);
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Explicacion = $"Error en el cáculo: {ex.Message}";
                response.EsKaprekar = false;
            }

            return response;
        }

        private static void CalcularOptimizado(KaprekarResponse response)
        {
            int operaciones = 0;
            int numero = response.Numero;

            response.PasosCalculo.Add($"Paso 1: Calculando {numero}²");
            long cuadrado = (long)numero * numero;
            response.Cuadrado = cuadrado;
            operaciones++;

            response.PasosCalculo.Add($"Paso 2: {numero}² = {cuadrado}");

            string cuadradoStr = cuadrado.ToString();
            int longitud = cuadradoStr.Length;

            response.PasosCalculo.Add($"Paso 3: El cuadrado {cuadrado} tiene {longitud} dígitos");

            // Probar todas las posibles divisiones
            bool encontrado = false;
            for (int i = 1; i< longitud && !encontrado; i++)
            {
                operaciones++;

                string parteIzquierda = cuadradoStr.Substring(0, i);
                string parteDerecha = cuadradoStr.Substring(i);

                // Evitar parte derecha que sea solo ceros
                if (parteDerecha.All(c => c == '0'))
                    continue;
                
                int valorIzquierda = string.IsNullOrEmpty(parteIzquierda) ? 0 : int.Parse(parteIzquierda);
                int valorDerecha = int.Parse(parteDerecha);
                int suma = valorIzquierda + valorDerecha;

                response.PasosCalculo.Add($"Paso {3 + operaciones}: Probando división en posición {i}: {valorIzquierda} + {valorDerecha} = {suma}");
                operaciones++;

                if (suma == numero)
                {
                    encontrado = true;
                    response.EsKaprekar = true;
                    response.Descomposicion = $"{valorIzquierda} + {valorDerecha} = {suma}";
                    response.Explicacion = $"{numero} es un número Kaprekar porque {numero}² = {cuadrado} y {valorIzquierda} + {valorDerecha} = {suma}";
                }
            }

            if(!encontrado)
            {
                response.EsKaprekar = false;
                response.Explicacion = $"{numero} no es un número de Kaprekar.Ninguna división de {cuadrado} suma {numero}";
            }

            response.NumeroOperaciones = operaciones;
        }

        private static void CalcularFuerzaBruta(KaprekarResponse response)
        {
            int operaciones = 0;
            int numero = response.Numero;

            response.PasosCalculo.Add($"Método fuerza bruta: Probando todas las combinaciones posibles.");

            long cuadrado = (long)numero * numero;
            response.Cuadrado = cuadrado;
            operaciones++;

            string cuadradoStr = cuadrado.ToString();

            bool encontrado = false;
            for (int i = 1; i < cuadradoStr.Length && !encontrado; i++)
            {
                for (int j = 0; j <= i && !encontrado; j++)
                {
                    operaciones += 2;

                    string parte1 = cuadradoStr.Substring(0, i);
                    string parte2 = cuadradoStr.Substring(i);

                    if (parte2.All(c => c == '0')) continue;

                    int valor1 = string.IsNullOrEmpty(parte1) ? 0 : int.Parse(parte1);
                    int valor2 = int.Parse(parte2);

                    if (valor1 + valor2 == numero)
                    {
                        encontrado = true;
                        response.EsKaprekar = true;
                        response.Descomposicion = $"{valor1} + {valor2} = {numero}";
                        response.Explicacion = $"Encontrado por fuerza bruta: {numero}² = {cuadrado} -> {valor1} + {valor2} = {numero}";
                    }
                }
            }

            if (!encontrado)
            {
                response.EsKaprekar = false;
                response.Explicacion = $"Fuerza bruta: {numero} NO es Kaprekar después de {operaciones} operaciones";
            }

            response.NumeroOperaciones = operaciones;
        }

        private static void CalcularMatematico(KaprekarResponse response)
        {
            int operaciones = 0;
            int numero = response.Numero;

            response.PasosCalculo.Add($"Método matemático: Usando propiedades matemáticas de Kaprekar");

            long cuadrado = (long)numero * numero;
            response.Cuadrado = cuadrado;
            operaciones++;

            int digitos = numero.ToString().Length;
            long potencia10 = (long)Math.Pow(10, digitos);
            operaciones++;

            response.PasosCalculo.Add($"Número de dígitos: {digitos}, potencia de 10: {potencia10}");

            bool esKaprekar = false;
            for (int k = 1; k <= digitos && !esKaprekar; k++)
            {
                operaciones++;
                long mod = (long)Math.Pow(10, k) - 1;

                if (mod > 0 && cuadrado % mod == numero % mod)
                {
                    long divisor = (long)Math.Pow(10, k);
                    long parteIzq = cuadrado / divisor;
                    long parteDer = cuadrado % divisor;

                    operaciones += 2;

                    if (parteDer > 0 && parteIzq + parteDer == numero)
                    {
                        esKaprekar = true;
                        response.EsKaprekar = true;
                        response.Descomposicion = $"{parteIzq} + {parteDer} = {numero}";
                        response.Explicacion = $"Método matemático: {numero}² = {cuadrado} -> {parteIzq} + {parteDer} = {numero}";
                        response.PasosCalculo.Add($"Encontrado usando k={k}: {parteIzq} + {parteDer} = {numero}");
                    }
                }
            }

            if (!esKaprekar)
            {
                response.EsKaprekar = false;
                response.Explicacion = $"Método matemático: {numero} NO cumple las propiedades de Kaprekar";
            }

            response.NumeroOperaciones = operaciones;
        }
    }
}