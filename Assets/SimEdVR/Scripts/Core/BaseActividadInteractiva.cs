using UnityEngine;
using System;

namespace SimEdVR.Scripts.Core
{
    /// <summary>
    /// Contrato base abstracto para cualquier minijuego o actividad interactiva 3D en las salas de SimEdVR.
    /// Permite el principio Open/Closed (SOLID): nuevas actividades (mecánicas con objetos físicos,
    /// palancas, ensamble, dibujo, etc.) pueden desarrollarse sin modificar el controlador de sala ni el GameManager.
    /// </summary>
    public abstract class BaseActividadInteractiva : MonoBehaviour
    {
        /// <summary>
        /// Evento emitido cuando el usuario completa exitosamente el desafío práctico de la actividad.
        /// </summary>
        public event Action OnActividadSuperada;

        /// <summary>
        /// Evento emitido cuando la actividad se reinicia o restablece.
        /// </summary>
        public event Action OnActividadReiniciada;

        /// <summary>
        /// Indica si la actividad ya fue completada exitosamente.
        /// </summary>
        public abstract bool EstaCompletada { get; }

        /// <summary>
        /// Inicializa o activa los elementos interactivos 3D de la sala (física, interactables XRI, canvas guía).
        /// Invocado por el controlador de la sala temática tras superar las 2 preguntas teóricas.
        /// </summary>
        public abstract void IniciarActividad();

        /// <summary>
        /// Restaura la actividad a su estado inicial en caso de reinicio de la estación o museo.
        /// </summary>
        public abstract void ReiniciarActividad();

        /// <summary>
        /// Método de conveniencia para que las clases derivadas disparen el evento de superación.
        /// </summary>
        protected void NotificarActividadSuperada()
        {
            OnActividadSuperada?.Invoke();
        }

        /// <summary>
        /// Método de conveniencia para que las clases derivadas disparen el evento de reinicio.
        /// </summary>
        protected void NotificarActividadReiniciada()
        {
            OnActividadReiniciada?.Invoke();
        }
    }
}
