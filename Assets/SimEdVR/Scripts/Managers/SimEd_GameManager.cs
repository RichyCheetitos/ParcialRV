using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimEdVR.Scripts.Managers
{
    /// <summary>
    /// Administrador de estado central del simulador educativo (Singleton).
    /// Centraliza el estado de cada punto de aprendizaje / sala temática y coordina
    /// los eventos del patrón Observer para UI, Audio y locomoción.
    /// </summary>
    public class SimEd_GameManager : MonoBehaviour
    {
        public static SimEd_GameManager Instance { get; private set; }

        [Header("Estado Global")]
        public bool juegoIniciado = false;
        public bool salaCompletada = false;

        // Diccionario de seguimiento: ID de Punto -> ¿Completado?
        private Dictionary<string, bool> estadoPuntos = new Dictionary<string, bool>();

        #region Eventos Observer (Publicador - Suscriptor)
        /// <summary>Emitido cuando el usuario presiona 'Jugar' en el Lobby.</summary>
        public static event Action OnJuegoIniciado;

        /// <summary>Emitido cuando se activa un punto de aprendizaje (para aislamiento visual).</summary>
        public static event Action<string> OnPuntoIniciado;
        public static event Action<string> OnSalaIniciada // Alias para compatibilidad
        {
            add => OnPuntoIniciado += value;
            remove => OnPuntoIniciado -= value;
        }

        /// <summary>Emitido tras responder una pregunta: true si fue acertada, false si falló.</summary>
        public static event Action<bool> OnPreguntaRespondida;

        /// <summary>Emitido cuando se inicia la actividad práctica de una sala.</summary>
        public static event Action<string> OnActividadIniciada;

        /// <summary>Emitido cuando un punto de aprendizaje culmina su ciclo completo (teoría + actividad).</summary>
        public static event Action<string> OnPuntoCompletado;
        public static event Action<string> OnSalaCompletada // Alias para compatibilidad
        {
            add => OnPuntoCompletado += value;
            remove => OnPuntoCompletado -= value;
        }

        /// <summary>Emitido cuando el estado global de la sala/museo se actualiza.</summary>
        public static event Action<bool> OnEstadoSalaActualizado;
        public static event Action OnTodasSalasCompletadas;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Comienza la sesión educativa desde el Lobby.
        /// </summary>
        public void IniciarJuego()
        {
            juegoIniciado = true;
            Debug.Log("[SimEd_GameManager] Juego iniciado.");
            OnJuegoIniciado?.Invoke();
        }

        /// <summary>
        /// Notifica que un punto de aprendizaje fue iniciado (activa aislamiento en los demás).
        /// </summary>
        public void NotificarPuntoIniciado(string idPunto)
        {
            Debug.Log($"[SimEd_GameManager] Punto iniciado: {idPunto}");
            OnPuntoIniciado?.Invoke(idPunto);
        }

        /// <summary>
        /// Emite el feedback de respuesta para audio y analíticas.
        /// </summary>
        public void NotificarPreguntaRespondida(bool correcta)
        {
            OnPreguntaRespondida?.Invoke(correcta);
        }

        /// <summary>
        /// Notifica el inicio de la actividad interactiva de un punto.
        /// </summary>
        public void NotificarActividadIniciada(string idPunto)
        {
            Debug.Log($"[SimEd_GameManager] Actividad iniciada para: {idPunto}");
            OnActividadIniciada?.Invoke(idPunto);
        }

        /// <summary>
        /// Registra un punto de aprendizaje como completado.
        /// </summary>
        public void RegistrarPuntoCompletado(string idPunto)
        {
            if (string.IsNullOrEmpty(idPunto)) return;

            estadoPuntos[idPunto] = true;
            Debug.Log($"[SimEd_GameManager] Punto completado: {idPunto}");
            OnPuntoCompletado?.Invoke(idPunto);

            VerificarEstadoSala();
        }

        /// <summary>
        /// Registra un punto como pendiente si aún no estaba en el diccionario.
        /// </summary>
        public void RegistrarPuntoPendiente(string idPunto)
        {
            if (string.IsNullOrEmpty(idPunto)) return;

            if (!estadoPuntos.ContainsKey(idPunto))
            {
                estadoPuntos[idPunto] = false;
                Debug.Log($"[SimEd_GameManager] Punto registrado como pendiente: {idPunto}");
            }
        }

        /// <summary>
        /// Consulta si un punto ya fue completado.
        /// </summary>
        public bool EsPuntoCompletado(string idPunto)
        {
            if (estadoPuntos.TryGetValue(idPunto, out bool completado))
            {
                return completado;
            }
            return false;
        }

        /// <summary>
        /// Retorna el conteo actual de puntos completados vs total.
        /// </summary>
        public (int completados, int total) ObtenerConteoPuntos()
        {
            int completados = 0;
            foreach (var kvp in estadoPuntos)
            {
                if (kvp.Value) completados++;
            }
            return (completados, estadoPuntos.Count);
        }

        /// <summary>
        /// Verifica si todos los puntos registrados han sido completados.
        /// </summary>
        public void VerificarEstadoSala()
        {
            if (estadoPuntos.Count == 0) return;

            bool todosCompletados = true;
            foreach (var kvp in estadoPuntos)
            {
                if (!kvp.Value)
                {
                    todosCompletados = false;
                    break;
                }
            }

            salaCompletada = todosCompletados;
            OnEstadoSalaActualizado?.Invoke(salaCompletada);

            if (salaCompletada)
            {
                Debug.Log("[SimEd_GameManager] ¡Todas las salas y puntos completados!");
                OnTodasSalasCompletadas?.Invoke();
            }
        }
    }
}
