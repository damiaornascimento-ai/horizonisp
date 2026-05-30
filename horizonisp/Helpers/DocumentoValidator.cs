using System.Text.RegularExpressions;

namespace horizonisp.Helpers
{
    public static class DocumentoValidator
    {
        public static string Normalizar(string documento) =>
            Regex.Replace(documento ?? string.Empty, @"[^\d]", string.Empty);

        public static bool EhValido(string documento)
        {
            var numeros = Normalizar(documento);
            return numeros.Length switch
            {
                11 => ValidarCpf(numeros),
                14 => ValidarCnpj(numeros),
                _ => false
            };
        }

        public static string Formatar(string documento)
        {
            var numeros = Normalizar(documento);
            return numeros.Length switch
            {
                11 => $"{numeros[..3]}.{numeros[3..6]}.{numeros[6..9]}-{numeros[9..]}",
                14 => $"{numeros[..2]}.{numeros[2..5]}.{numeros[5..8]}/{numeros[8..12]}-{numeros[12..]}",
                _ => documento.Trim()
            };
        }

        private static bool ValidarCpf(string cpf)
        {
            if (cpf.Distinct().Count() == 1)
            {
                return false;
            }

            var soma = 0;
            for (var i = 0; i < 9; i++)
            {
                soma += (cpf[i] - '0') * (10 - i);
            }

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;
            if (cpf[9] - '0' != digito1)
            {
                return false;
            }

            soma = 0;
            for (var i = 0; i < 10; i++)
            {
                soma += (cpf[i] - '0') * (11 - i);
            }

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;
            return cpf[10] - '0' == digito2;
        }

        private static bool ValidarCnpj(string cnpj)
        {
            if (cnpj.Distinct().Count() == 1)
            {
                return false;
            }

            int[] mult1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] mult2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            var soma = 0;
            for (var i = 0; i < 12; i++)
            {
                soma += (cnpj[i] - '0') * mult1[i];
            }

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;
            if (cnpj[12] - '0' != digito1)
            {
                return false;
            }

            soma = 0;
            for (var i = 0; i < 13; i++)
            {
                soma += (cnpj[i] - '0') * mult2[i];
            }

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;
            return cnpj[13] - '0' == digito2;
        }
    }
}
