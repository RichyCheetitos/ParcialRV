using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using SimEdVR.Scripts.Core;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.VR
{
    /// <summary>
    /// Controlador de un Punto de Aprendizaje dentro de la sala temática.
    /// Cada punto de aprendizaje contiene exactamente 2 preguntas teóricas evaluativas.
    /// Al responder correctamente ambas preguntas, este punto se marca como completado
    /// en el SimEd_GameManager y en el tablero de pared (SimEd_PuntoInformacion).
    /// </summary>
    public class SimEd_PuntoAprendizaje : MonoBehaviour
    {
        [Header("Datos ScriptableObject del Punto")]
        [Tooltip("ScriptableObject que contiene la información del tema y sus 2 preguntas.")]
        public SimEd_PreguntaSO datosPunto;

        [Header("Contenedor Visual para Aislamiento (Opcional)")]
        [Tooltip("Objeto que se oculta durante el aislamiento cognitivo sin desactivar el GameObject raíz.")]
        public GameObject contenedorVisual;

        [Header("Actividad Práctica Asociada (Opcional)")]
        [Tooltip("Si este punto desbloquea una actividad 3D, se puede enlazar aquí.")]
        public BaseActividadInteractiva actividadInteractiva;

        [Header("Contenedores Principales UI")]
        public GameObject panelInicial;
        public GameObject panelPregunta;
        public GameObject panelTransicionActividad;
        public GameObject panelCompletado;

        [Header("UI Estado Inicial")]
        public TextMeshProUGUI txtTemaInicial;
        public TextMeshProUGUI txtDescripcionInicial;
        public Image imgInicial;
        public Button btnIniciar;

        [Header("UI Pregunta")]
        public TextMeshProUGUI txtTeorico;
        public TextMeshProUGUI txtPregunta;
        public TextMeshProUGUI txtFeedback;
        public Button btnRespuesta1;
        public TextMeshProUGUI txtRespuesta1;
        public Button btnRespuesta2;
        public TextMeshProUGUI txtRespuesta2;
        public Button btnRespuesta3;
        public TextMeshProUGUI txtRespuesta3;
        public Button btnContinuar;

        private int indicePreguntaActual = 0;
        private bool completado = false;
        private Coroutine corrutinaAvance;

        private void OnEnable()
        {
            SimEd_GameManager.OnPuntoIniciado += ListenerOtroPuntoIniciado;
            SimEd_GameManager.OnPuntoCompletado += ListenerOtroPuntoCompletado;

            if (actividadInteractiva != null)
            {
                actividadInteractiva.OnActividadSuperada += ManejarActividadSuperada;
            }
        }

        private void OnDisable()
        {
            SimEd_GameManager.OnPuntoIniciado -= ListenerOtroPuntoIniciado;
            SimEd_GameManager.OnPuntoCompletado -= ListenerOtroPuntoCompletado;

            if (actividadInteractiva != null)
            {
                actividadInteractiva.OnActividadSuperada -= ManejarActividadSuperada;
            }

            if (corrutinaAvance != null)
            {
                StopCoroutine(corrutinaAvance);
            }
        }

        private void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            if (datosPunto != null && SimEd_GameManager.Instance != null)
            {
                SimEd_GameManager.Instance.RegistrarPuntoPendiente(datosPunto.idPunto);
            }

            ConfigurarBotones();
            MostrarEstadoInicial();
        }

        private void ConfigurarBotones()
        {
            if (btnIniciar != null) btnIniciar.onClick.AddListener(OnClickIniciar);
            if (btnRespuesta1 != null) btnRespuesta1.onClick.AddListener(() => Responder(0));
            if (btnRespuesta2 != null) btnRespuesta2.onClick.AddListener(() => Responder(1));
            if (btnRespuesta3 != null) btnRespuesta3.onClick.AddListener(() => Responder(2));
            if (btnContinuar != null) btnContinuar.onClick.AddListener(OnClickContinuar);
        }

        public void MostrarEstadoInicial()
        {
            SetVisibilidadVisual(true);

            if (panelInicial != null) panelInicial.SetActive(true);
            if (panelPregunta != null) panelPregunta.SetActive(false);
            if (panelTransicionActividad != null) panelTransicionActividad.SetActive(false);
            if (panelCompletado != null) panelCompletado.SetActive(false);

            if (datosPunto != null)
            {
                if (txtTemaInicial != null) txtTemaInicial.text = datosPunto.tema;
                if (txtDescripcionInicial != null) txtDescripcionInicial.text = datosPunto.descripcion;
                if (imgInicial != null)
                {
                    if (datosPunto.imagen != null)
                    {
                        imgInicial.sprite = datosPunto.imagen;
                        imgInicial.gameObject.SetActive(true);
                    }
                    else
                    {
                        imgInicial.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void OnClickIniciar()
        {
            if (SimEd_GameManager.Instance != null && datosPunto != null)
            {
                SimEd_GameManager.Instance.NotificarPuntoIniciado(datosPunto.idPunto);
            }

            indicePreguntaActual = 0;
            CargarPreguntaActual();
        }

        private void CargarPreguntaActual()
        {
            if (datosPunto == null || datosPunto.preguntas == null || datosPunto.preguntas.Count == 0) return;

            if (panelInicial != null) panelInicial.SetActive(false);
            if (panelPregunta != null) panelPregunta.SetActive(true);
            if (panelTransicionActividad != null) panelTransicionActividad.SetActive(false);
            if (panelCompletado != null) panelCompletado.SetActive(false);

            PreguntaData p = datosPunto.preguntas[indicePreguntaActual];

            if (txtTeorico != null) txtTeorico.text = p.textoTeorico;
            if (txtPregunta != null) txtPregunta.text = $"<b>Pregunta {indicePreguntaActual + 1}/2:</b> {p.enunciadoPregunta}";
            if (txtRespuesta1 != null) txtRespuesta1.text = p.respuesta1;
            if (txtRespuesta2 != null) txtRespuesta2.text = p.respuesta2;
            if (txtRespuesta3 != null) txtRespuesta3.text = p.respuesta3;

            if (txtFeedback != null) txtFeedback.text = "";
            if (btnContinuar != null) btnContinuar.gameObject.SetActive(false);
            
            HabilitarBotonesRespuesta(true);
        }

        private void Responder(int indiceSeleccionado)
        {
            if (datosPunto == null || datosPunto.preguntas == null) return;
            PreguntaData p = datosPunto.preguntas[indicePreguntaActual];

            if (indiceSeleccionado == p.indiceRespuestaCorrecta)
            {
                // Acierto
                if (txtFeedback != null) txtFeedback.text = "<color=#2ECC71><b>¡Muy bien! Respuesta correcta.</b></color>";
                HabilitarBotonesRespuesta(false);

                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(true);
                }

                corrutinaAvance = StartCoroutine(RutinaAvanzarPregunta());
            }
            else
            {
                // Error
                if (txtFeedback != null) txtFeedback.text = "<color=#E74C3C><b>Respuesta incorrecta. Inténtalo de nuevo.</b></color>";

                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(false);
                }
            }
        }

        private IEnumerator RutinaAvanzarPregunta()
        {
            yield return new WaitForSeconds(1.2f);

            // Si aún queda la segunda pregunta, avanzar
            if (indicePreguntaActual + 1 < datosPunto.preguntas.Count && indicePreguntaActual + 1 < 2)
            {
                indicePreguntaActual++;
                CargarPreguntaActual();
            }
            else
            {
                // Ambas preguntas superadas -> ¡ESTE PUNTO DE APRENDIZAJE QUEDA COMPLETADO!
                FinalizarPuntoDeAprendizaje();
            }
        }

        /// <summary>
        /// Marca este Punto de Aprendizaje como completado en el GameManager y actualiza la pared.
        /// </summary>
        private void FinalizarPuntoDeAprendizaje()
        {
            completado = true;

            // 1. Notificar inmediatamente al GameManager para actualizar este punto en la pared (Punto A, Punto B, etc.)
            if (SimEd_GameManager.Instance != null && datosPunto != null)
            {
                Debug.Log($"[SimEd_PuntoAprendizaje] ¡Punto '{datosPunto.idPunto}' completado con éxito!");
                SimEd_GameManager.Instance.RegistrarPuntoCompletado(datosPunto.idPunto);
            }

            // 2. Mostrar feedback visual de completado en este atril
            HabilitarBotonesRespuesta(false);

            if (panelCompletado != null)
            {
                if (panelPregunta != null) panelPregunta.SetActive(false);
                panelCompletado.SetActive(true);
            }
            else
            {
                if (txtFeedback != null)
                {
                    txtFeedback.text = "<color=#2ECC71><b>¡Punto completado con éxito! Has superado las 2 preguntas.</b></color>";
                }
                if (btnContinuar != null)
                {
                    btnContinuar.gameObject.SetActive(true);
                }
            }

            // 3. Si tiene una actividad práctica 3D asociada directamente, la inicia
            if (actividadInteractiva != null)
            {
                actividadInteractiva.IniciarActividad();
            }
        }

        private void ManejarActividadSuperada()
        {
            if (panelCompletado != null)
            {
                panelCompletado.SetActive(true);
            }
        }

        public void OnClickContinuar()
        {
            MostrarEstadoInicial();
        }

        private void HabilitarBotonesRespuesta(bool habilitar)
        {
            if (btnRespuesta1 != null) btnRespuesta1.interactable = habilitar;
            if (btnRespuesta2 != null) btnRespuesta2.interactable = habilitar;
            if (btnRespuesta3 != null) btnRespuesta3.interactable = habilitar;
        }

        #region Aislamiento Cognitivo
        private void SetVisibilidadVisual(bool visible)
        {
            if (contenedorVisual != null)
            {
                contenedorVisual.SetActive(visible);
            }
            else
            {
                if (panelInicial != null) panelInicial.SetActive(visible && !completado);
                if (panelPregunta != null) panelPregunta.SetActive(visible && !completado);
            }
        }

        private void ListenerOtroPuntoIniciado(string idIniciado)
        {
            if (datosPunto != null && datosPunto.idPunto != idIniciado)
            {
                SetVisibilidadVisual(false);
            }
        }

        private void ListenerOtroPuntoCompletado(string idCompletado)
        {
            SetVisibilidadVisual(true);

            if (completado && panelCompletado != null)
            {
                panelCompletado.SetActive(true);
            }
            else if (!completado)
            {
                MostrarEstadoInicial();
            }
        }
        #endregion
    }
}
