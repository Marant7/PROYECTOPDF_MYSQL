// Importar las bibliotecas necesarias
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient; // Para manejar conexiones con MySQL
using Dapper; // Para simplificar las operaciones con la base de datos
using NegocioPDF.Models; // Importar el modelo de Usuario

namespace NegocioPDF.Repositories
{
    // Repositorio de usuario que maneja la interacción con la base de datos
    public class UsuarioRepository
    {
        // Cadena de conexión a la base de datos
        private readonly string _connectionString;

        // Constructor que recibe la cadena de conexión
        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

         // Método para iniciar sesión con correo y contraseña
        public Usuario Login(string correo, string password)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Buscar el usuario con el correo y la contraseña proporcionados
                var usuario = connection.QueryFirstOrDefault<Usuario>(
                    "SELECT * FROM Usuarios WHERE Correo = @Correo AND Password = @Password",
                    new { Correo = correo, Password = password });

                return usuario; // Retorna el usuario si existe, de lo contrario retorna null
            }
        }

        // NegocioPDF/Repositories/UsuarioRepository.cs
        public void RegistrarUsuario(Usuario usuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Verificar si ya existe un usuario con el mismo correo
                var existeUsuario = connection.QueryFirstOrDefault<Usuario>(
                    "SELECT * FROM Usuarios WHERE Correo = @Correo", new { Correo = usuario.Correo });

                if (existeUsuario != null)
                {
                    throw new Exception("El correo ya está registrado");
                }

                // Insertar el usuario en la base de datos
                var sqlInsertUser = "INSERT INTO Usuarios (Nombre, Correo, Password) VALUES (@Nombre, @Correo, @Password)";
                connection.Execute(sqlInsertUser, usuario);

                // Obtener el ID del usuario recién creado
                usuario.Id = connection.QuerySingle<int>("SELECT LAST_INSERT_ID();");

                // Registrar la suscripción por defecto 'basico' con un máximo de 5 operaciones
                var sqlInsertSubscription = @"
                    INSERT INTO detalles_suscripciones (usuario_id, tipo_suscripcion, fecha_inicio, fecha_final, precio, operaciones_realizadas) 
                    VALUES (@UsuarioId, 'basico', NOW(), DATE_ADD(NOW(), INTERVAL 1 YEAR), 0.00, 0)";
                
                connection.Execute(sqlInsertSubscription, new { UsuarioId = usuario.Id });
            }
        }
        // Método para obtener un usuario por su ID
            public Usuario ObtenerUsuarioPorId(int idUsuario)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Usuarios WHERE Id = @IdUsuario";
                    return connection.QueryFirstOrDefault<Usuario>(query, new { IdUsuario = idUsuario });
                }
            }

        // Método para obtener todos los usuarios de la base de datos
        public IEnumerable<Usuario> ObtenerUsuarios()
        {
            // Usar "using" para asegurar que la conexión se cierre después de usarse
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Abrir la conexión a la base de datos
                connection.Open();

                // Ejecutar el comando SQL para obtener todos los usuarios y devolverlos
                return connection.Query<Usuario>("SELECT * FROM Usuarios");
            }
        }
    }
}