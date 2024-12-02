// NegocioPDF/Repositories/OperacionesPDFRepository.cs
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using NegocioPDF.Models;
using System.Linq;

namespace NegocioPDF.Repositories
{
    public class OperacionesPDFRepository
    {
        private readonly string _connectionString;

        public OperacionesPDFRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Método para registrar una nueva operación de PDF
        public bool RegistrarOperacionPDF(int usuarioId, string tipoOperacion)
{
    using (var connection = new MySqlConnection(_connectionString))
    {
        connection.Open();

        // Verificar si el usuario puede realizar la operación
        var suscripcion = connection.QueryFirstOrDefault<DetalleSuscripcion>(
            "SELECT * FROM detalles_suscripciones WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId }
        );

        if (suscripcion.tipo_suscripcion == "basico" && suscripcion.operaciones_realizadas >= 5)
        {
            return false; // El usuario no puede realizar más operaciones
        }

        // Insertar la operación en la tabla `operaciones_pdf`
        connection.Execute(
            @"INSERT INTO operaciones_pdf (usuario_id, TipoOperacion, fechaOperacion) 
              VALUES (@UsuarioId, @TipoOperacion, NOW())",
            new { UsuarioId = usuarioId, TipoOperacion = tipoOperacion }
        );

        // Actualizar contador de operaciones si es suscripción básica
        if (suscripcion.tipo_suscripcion == "basico")
        {
            suscripcion.operaciones_realizadas++;
            connection.Execute(
                "UPDATE detalles_suscripciones SET operaciones_realizadas = @OperacionesRealizadas WHERE usuario_id = @UsuarioId",
                new { OperacionesRealizadas = suscripcion.operaciones_realizadas, UsuarioId = usuarioId }
            );
        }

        return true; // Operación registrada exitosamente
    }
}

        // Método para obtener todas las operaciones de PDF realizadas por un usuario
        public IEnumerable<OperacionPDF> ObtenerOperacionesPorUsuario(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT * FROM operaciones_pdf 
                    WHERE usuario_id = @UsuarioId
                    ORDER BY fecha_operacion DESC";

                return connection.Query<OperacionPDF>(query, new { UsuarioId = usuarioId }).ToList();
            }
        }

        // Método para contar las operaciones realizadas por un usuario (para la suscripción básica)
        public int ContarOperacionesRealizadas(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) 
                    FROM operaciones_pdf 
                    WHERE usuario_id = @UsuarioId";

                return connection.QuerySingle<int>(query, new { UsuarioId = usuarioId });
            }
        }
        // Validar si el usuario puede realizar más operaciones
        public bool ValidarOperacion(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT operaciones_realizadas, tipo_suscripcion FROM detalles_suscripciones WHERE usuario_id = @UsuarioId";
                var resultado = connection.QueryFirstOrDefault(query, new { UsuarioId = usuarioId });

                // Si es 'basico' y ya realizó 5 operaciones, no puede hacer más
                if (resultado.tipo_suscripcion == "basico" && resultado.operaciones_realizadas >= 5)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
