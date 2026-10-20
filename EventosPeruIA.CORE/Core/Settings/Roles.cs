namespace EventosPeruIA.CORE.Core.Settings
{
    // Nombres e Id de la tabla Rol (datos iniciales del script SQL)
    public static class Roles
    {
        public const string Asistente = "Asistente";
        public const string Organizador = "Organizador";
        public const string Administrador = "Administrador";

        public const int IdAsistente = 1;
        public const int IdOrganizador = 2;
        public const int IdAdministrador = 3;
    }

    public static class EstadosEvento
    {
        public const string Borrador = "Borrador";
        public const string Publicado = "Publicado";
        public const string Finalizado = "Finalizado";
        public const string Cancelado = "Cancelado";
        public const string Suspendido = "Suspendido";
    }

    public static class EstadosCompra
    {
        public const string Pendiente = "Pendiente";
        public const string Pagada = "Pagada";
        public const string Rechazada = "Rechazada";
        public const string Reembolsada = "Reembolsada";
    }

    public static class EstadosEntrada
    {
        public const string Valida = "Valida";
        public const string Utilizada = "Utilizada";
        public const string Anulada = "Anulada";
    }
}
