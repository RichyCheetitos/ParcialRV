using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimEdVR.Scripts.Core;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Actividad Práctica de Historia: Desafío de Verdadero o Falso.
    /// El jugador lee una afirmación histórica en el Canvas y debe pararse / teletransportarse
    /// a la plataforma que considere correcta (VERDADERO o FALSO) usando TeleportationAnchor.
    /// Deriva de BaseActividadInteractiva y actualiza SimEd_GameManager y PuntoInformacion.
    /// </summary>
    public class Actividad_HistoriaVF : BaseActividadInteractiva
    {
        [Header("Identificador de Punto en GameManager")]
        [Tooltip("ID del punto que representa esta actividad en GameManager y PuntoInformacion (ej: 'Punto B').")]
        public string idPunto = "Punto F";

        [Header("Canvas World Space de la Actividad")]
        [SerializeField] private Canvas canvasActividad;
        [SerializeField] private TextMeshProUGUI txtTitulo;
        [SerializeField] private TextMeshProUGUI txtAfirmacion;
        [SerializeField] private TextMeshProUGUI txtFeedback;
        [SerializeField] private Button btnEmpezar;

        [Header("Contenido Histórico")]
        [TextArea(2, 4)]
        [Tooltip("La afirmación histórica que el estudiante debe evaluar.")]
        public string afirmacionHistorica = "La Revolución Francesa y la Declaración de Independencia de EE.UU. ocurrieron en el mismo siglo (Siglo XVIII).";

        [Tooltip("Define si la afirmación es Verdadera o Falsa.")]
        public bool respuestaEsVerdadera = true;

        [TextArea(1, 3)]
        [Tooltip("Mensaje de retroalimentación pedagógica al acertar.")]
        public string explicacionAcierto = "¡Correcto! Ambos acontecimientos históricos ocurrieron a finales del siglo XVIII (1776 y 1789).";

        [TextArea(1, 3)]
        [Tooltip("Mensaje pedagógico al equivocarse.")]
        public string explicacionError = "Esa no es la respuesta correcta. Revisa las fechas y períodos históricos.";

        [Header("Plataformas en el Suelo")]
        [Tooltip("Plataforma asignada a la opción VERDADERO.")]
        public PlataformaRespuestaVF plataformaVerdadero;

        [Tooltip("Plataforma asignada a la opción FALSO.")]
        public PlataformaRespuestaVF plataformaFalso;

        [Tooltip("Contenedor opcional de los elementos 3D.")]
        public GameObject contenedorPlataformas;

        private bool _estaCompletada = false;
        private bool _actividadIniciada = false;

        public override bool EstaCompletada => _estaCompletada;

        private void Start()
        {
            ConfigurarCanvas();
            RegistrarEnGameManager();
            InicializarTextosUI();

            if (btnEmpezar != null)
            {
                btnEmpezar.onClick.AddListener(OnClickEmpezar);
            }

            if (plataformaVerdadero != null)
            {
                plataformaVerdadero.esPlataformaVerdadero = true;
                plataformaVerdadero.ConfigurarActividad(this);
            }

            if (plataformaFalso != null)
            {
                plataformaFalso.esPlataformaVerdadero = false;
                plataformaFalso.ConfigurarActividad(this);
            }
        }

        private void ConfigurarCanvas()
        {
            if (canvasActividad == null)
            {
                canvasActividad = GetComponentInChildren<Canvas>();
            }

            if (canvasActividad != null && canvasActividad.renderMode == RenderMode.WorldSpace && canvasActividad.worldCamera == null)
            {
                canvasActividad.worldCamera = Camera.main;
            }
        }

        private void RegistrarEnGameManager()
        {
            if (SimEd_GameManager.Instance != null && !string.IsNullOrEmpty(idPunto))
            {
                SimEd_GameManager.Instance.RegistrarPuntoPendiente(idPunto);
            }
        }

        private void InicializarTextosUI()
        {
            if (txtTitulo != null)
            {
                txtTitulo.text = "Actividad Práctica: Desafío de Historia";
            }

            if (txtAfirmacion != null)
            {
                txtAfirmacion.text = $"<b>Afirmación:</b>\n\"{afirmacionHistorica}\"\n\n<i>¿Es Verdadero o Falso? Teletranspórtate a la plataforma de tu elección.</i>";
            }

            if (txtFeedback != null)
            {
                txtFeedback.text = "Presiona 'Empezar' para activar las plataformas.";
            }

            if (btnEmpezar != null)
            {
                btnEmpezar.gameObject.SetActive(true);
                btnEmpezar.interactable = true;
            }
        }

        public void OnClickEmpezar()
        {
            IniciarActividad();
        }

        public override void IniciarActividad()
        {
            _actividadIniciada = true;
            _estaCompletada = false;

            if (btnEmpezar != null)
            {
                btnEmpezar.gameObject.SetActive(false);
            }

            if (txtFeedback != null)
            {
                txtFeedback.text = "<color=#F39C12><b>¡Actividad en curso!</b> Apunta tu rayo de teletransporte a la plataforma que creas correcta (VERDADERO o FALSO).</color>";
            }

            if (contenedorPlataformas != null)
            {
                contenedorPlataformas.SetActive(true);
            }

            if (SimEd_GameManager.Instance != null && !string.IsNullOrEmpty(idPunto))
            {
                SimEd_GameManager.Instance.NotificarActividadIniciada(idPunto);
            }

            Debug.Log($"[Actividad_HistoriaVF] Actividad iniciada. Respuesta correcta esperada: {(respuestaEsVerdadera ? "VERDADERO" : "FALSO")}");
        }

        public override void ReiniciarActividad()
        {
            _estaCompletada = false;
            _actividadIniciada = false;

            InicializarTextosUI();

            if (plataformaVerdadero != null) plataformaVerdadero.RestaurarColorNormal();
            if (plataformaFalso != null) plataformaFalso.RestaurarColorNormal();

            NotificarActividadReiniciada();
        }

        /// <summary>
        /// Evalúa la respuesta según la plataforma donde se paró / teletransportó el jugador.
        /// </summary>
        public void EvaluarRespuesta(PlataformaRespuestaVF plataforma)
        {
            if (!_actividadIniciada || _estaCompletada || plataforma == null) return;

            bool acerto = (plataforma.esPlataformaVerdadero == respuestaEsVerdadera);

            if (acerto)
            {
                // Acierto
                _estaCompletada = true;

                if (txtFeedback != null)
                {
                    txtFeedback.text = $"<color=#2ECC71><b>¡CORRECTO!</b>\n{explicacionAcierto}\n\n¡Actividad de Historia completada con éxito!</color>";
                }

                plataforma.MostrarFeedbackVisual(true);

                // Audio de éxito
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(true);
                    SimEd_GameManager.Instance.RegistrarPuntoCompletado(idPunto);
                }

                Debug.Log($"[Actividad_HistoriaVF] ¡Respuesta acertada! Punto '{idPunto}' completado.");
                NotificarActividadSuperada();
            }
            else
            {
                // Error
                if (txtFeedback != null)
                {
                    string eleccion = plataforma.esPlataformaVerdadero ? "VERDADERO" : "FALSO";
                    txtFeedback.text = $"<color=#E74C3C><b>INCORRECTO:</b> Elegiste {eleccion}.\n{explicacionError}\nTeletranspórtate a la otra plataforma para corregir tu respuesta.</color>";
                }

                plataforma.MostrarFeedbackVisual(false);

                // Audio de error
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(false);
                }

                Debug.Log("[Actividad_HistoriaVF] Respuesta incorrecta seleccionada.");
            }
        }
    }
}
