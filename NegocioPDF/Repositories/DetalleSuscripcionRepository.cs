// NegocioPDF/Repositories/DetalleSuscripcionRepository.cs
using Dapper;
using MySql.Data.MySqlClient;
using NegocioPDF.Models;
using System.Linq;

namespace NegocioPDF.Repositories
{
    public class DetalleSuscripcionRepository
    {
        private readonly string _connectionString;

        public DetalleSuscripcionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Obtener la suscripción de un usuario por su ID
        public DetalleSuscripcion ObtenerPorUsuarioId(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Consulta SQL para obtener los detalles de suscripción y la información del usuario
                var query = @"
                    SELECT ds.*, 
                        u.id AS UsuarioId, 
                        u.Nombre, 
                        u.Correo, 
                        u.Password 
                    FROM detalles_suscripciones ds
                    INNER JOIN usuarios u ON ds.usuario_id = u.id
                    WHERE ds.usuario_id = @UsuarioId
                    ORDER BY ds.id DESC 
                    LIMIT 1";

                // Usar Dapper para mapear el resultado
                var detalle = connection.Query<DetalleSuscripcion, Usuario, DetalleSuscripcion>(
                    query,
                    (detalleSuscripcion, usuario) =>
                    {
                        detalleSuscripcion.Usuario = usuario;
                        return detalleSuscripcion;
                    },
                    new { UsuarioId = usuarioId },
                    splitOn: "UsuarioId").FirstOrDefault();

                // Verificar si los datos se han mapeado correctamente
                if (detalle == null)
                {
                    Console.WriteLine("No se encontró ninguna suscripción para el usuario con ID: " + usuarioId);
                }
                else
                {
                    Console.WriteLine("Tipo de Suscripción: " + detalle.tipo_suscripcion);
                    Console.WriteLine("Nombre de Usuario: " + detalle.Usuario.Nombre);
                }

                return detalle;
            }
        }
        // NegocioPDF/Repositories/DetalleSuscripcionRepository.cs
       // NegocioPDF/Repositories/DetalleSuscripcionRepository.cs
        public void ActualizarSuscripcion(DetalleSuscripcion suscripcion)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE detalles_suscripciones 
                    SET tipo_suscripcion = @tipo_suscripcion, 
                        fecha_inicio = @fecha_inicio, 
                        fecha_final = @fecha_final, 
                        precio = @precio, 
                        operaciones_realizadas = @operaciones_realizadas
                    WHERE usuario_id = @UsuarioId"; // Actualizamos usando el UsuarioId
                        
                connection.Execute(query, suscripcion);
            }
        }
    }
}