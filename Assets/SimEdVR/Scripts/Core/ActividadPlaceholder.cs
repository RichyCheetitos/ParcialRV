using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimEdVR.Scripts.Core
{
    /// <summary>
    /// Implementación provisional (Placeholder) de BaseActividadInteractiva.
    /// Permite probar y validar el flujo integral de evaluación de la sala (Pregunta 1 -> Pregunta 2 -> Actividad -> Sala Completada)
    /// mediante un botón interactivo o trigger de prueba antes de construir los minijuegos 3D definitivos.
    /// </summary>
    public class ActividadPlaceholder : BaseActividadInteractiva
    {
        [Header("Contenedor Visual de la Actividad")]
        [Tooltip("Objeto raíz de los elementos visuales/interactivos de esta actividad.")]
        [SerializeField] private GameObject contenedorActividad;

        [Header("Elementos de Prueba en UI")]
        [Tooltip("Botón para simular la resolución exitosa de la actividad.")]
        [SerializeField] private Button btnCompletarPrueba;

        [Tooltip("Texto informativo sobre la actividad provisional.")]
        [SerializeField] private TextMeshProUGUI txtInstrucciones;

        [Header("Textos Personalizados")]
        [TextArea(2, 4)]
        [SerializeField] private string mensajeEnCurso = "¡Fase Práctica Desbloqueada!\n(Actividad Placeholder: Presiona el botón para validar la mecánica práctica).";
        
        [TextArea(2, 4)]
        [SerializeField] private string mensajeSuperado = "¡Actividad práctica superada con éxito!";

        private bool _estaCompletada = false;
        public override bool EstaCompletada => _estaCompletada;

        private void Awake()
        {
            if (btnCompletarPrueba != null)
            {
                btnCompletarPrueba.onClick.AddListener(SimularExitoActividad);
            }

            // Iniciar desactivada hasta que el controlador de la sala la llame
            if (contenedorActividad != null)
            {
                contenedorActividad.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (btnCompletarPrueba != null)
            {
                btnCompletarPrueba.onClick.RemoveListener(SimularExitoActividad);
            }
        }

        public override void IniciarActividad()
        {
            _estaCompletada = false;

            if (contenedorActividad != null)
            {
                contenedorActividad.SetActive(true);
            }

            if (btnCompletarPrueba != null)
            {
                btnCompletarPrueba.gameObject.SetActive(true);
                btnCompletarPrueba.interactable = true;
            }

            if (txtInstrucciones != null)
            {
                txtInstrucciones.text = mensajeEnCurso;
            }

            Debug.Log($"[ActividadPlaceholder] Actividad iniciada en {gameObject.name}. Esperando resolución...");
        }

        public override void ReiniciarActividad()
        {
            _estaCompletada = false;

            if (contenedorActividad != null)
            {
                contenedorActividad.SetActive(false);
            }

            NotificarActividadReiniciada();
        }

        /// <summary>
        /// Simula el completado exitoso del minijuego 3D (llamado por el botón UI o un XR Simple Interactable).
        /// </summary>
        public void SimularExitoActividad()
        {
            if (_estaCompletada) return;

            _estaCompletada = true;

            if (btnCompletarPrueba != null)
            {
                btnCompletarPrueba.interactable = false;
            }

            if (txtInstrucciones != null)
            {
                txtInstrucciones.text = $"<color=green>{mensajeSuperado}</color>";
            }

            Debug.Log($"[ActividadPlaceholder] Desafío completado en {gameObject.name}. Disparando evento...");
            NotificarActividadSuperada();
        }
    }
}
