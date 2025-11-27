using System;
using UsersMS.Domain.Common;

namespace UsersMS.Domain.Entities
{
    /// <summary>
    /// Historial de acciones realizadas por un usuario.
    /// </summary>
    public class UserHistory : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        
        protected UserHistory() { }

        /// <summary>
        /// Crea una nueva instancia de UserHistory.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="action">Acción realizada.</param>
        /// <param name="details">Detalles de la acción.</param>
        public UserHistory(Guid userId, string action, string details)
        {
            UserId = userId;
            Action = action;
            Details = details;
        }
    }
}