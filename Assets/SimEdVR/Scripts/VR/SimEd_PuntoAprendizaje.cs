using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimEdVR.Scripts.Core;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.VR
{
    /// <summary>
    /// Controlador del Punto de Aprendizaje / Sala Temática en SimEdVR.
    /// Mantiene compatibilidad total con la configuración y prefabs existentes,
    /// añadiendo el ciclo de 2 preguntas, integración con BaseActividadInteractiva,
    /// y aislamiento cognitivo sin desactivar el GameObject raíz.
    /// </summary>
    public class SimEd_PuntoAprendizaje : MonoBehaviour
    {
        [Header("Datos ScriptableObject")]
        public SimEd_PreguntaSO datosPunto;

        [Header("Contenedor Visual para Aislamiento (Opcional)")]
        [Tooltip("Si se asigna, este contenedor se oculta durante el aislamiento. Si no, se ocultan panelInicial y panelPregunta sin apagar el GameObject raíz.")]
        public GameObject contenedorVisual;

        [Header("Actividad Práctica Asociada (Opcional / Modular)")]
        [Tooltip("Componente derivado de BaseActividadInteractiva (ej: ActividadPlaceholder) para la fase práctica 3D.")]
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
            // Suscripciones estrictas al Observer
            SimEd_GameManager.OnPuntoIniciado += ListenerOtroPuntoIniciado;
            SimEd_GameManager.OnPuntoCompletado += ListenerOtroPuntoCompletado;

            if (actividadInteractiva != null)
            {
                actividadInteractiva.OnActividadSuperada += ManejarActividadSuperada;
            }
        }

        private void OnDisable()
        {
            // Desuscripciones estrictas
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

            // Ciclo de exactamente 2 preguntas teóricas
            if (indicePreguntaActual + 1 < datosPunto.preguntas.Count && indicePreguntaActual + 1 < 2)
            {
                indicePreguntaActual++;
                CargarPreguntaActual();
            }
            else
            {
                // Fase teórica concluida -> Desbloquear actividad práctica
                DesbloquearFaseActividad();
            }
        }

        private void DesbloquearFaseActividad()
        {
            if (panelPregunta != null) panelPregunta.SetActive(false);

            if (panelTransicionActividad != null)
            {
                panelTransicionActividad.SetActive(true);
            }

            if (SimEd_GameManager.Instance != null && datosPunto != null)
            {
                SimEd_GameManager.Instance.NotificarActividadIniciada(datosPunto.idPunto);
            }

            if (actividadInteractiva != null)
            {
                actividadInteractiva.IniciarActividad();
            }
            else
            {
                // Si no hay actividad conectada aún, habilitamos el botón Continuar como fallback
                if (btnContinuar != null)
                {
                    btnContinuar.gameObject.SetActive(true);
                }
            }
        }

        private void ManejarActividadSuperada()
        {
            completado = true;

            if (panelTransicionActividad != null) panelTransicionActividad.SetActive(false);
            if (panelPregunta != null) panelPregunta.SetActive(false);

            if (panelCompletado != null)
            {
                panelCompletado.SetActive(true);
            }
            else if (txtFeedback != null)
            {
                txtFeedback.text = "<color=#2ECC71><b>¡Sala completada con éxito!</b></color>";
            }

            if (SimEd_GameManager.Instance != null && datosPunto != null)
            {
                SimEd_GameManager.Instance.RegistrarPuntoCompletado(datosPunto.idPunto);
            }
        }

        public void OnClickContinuar()
        {
            ManejarActividadSuperada();
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
                // Fallback: Si no hay contenedor explícito, togglear paneles directamente
                if (panelInicial != null) panelInicial.SetActive(visible && !completado);
                if (panelPregunta != null) panelPregunta.SetActive(visible && !completado);
            }
        }

        private void ListenerOtroPuntoIniciado(string idIniciado)
        {
            if (datosPunto != null && datosPunto.idPunto != idIniciado)
            {
                // Ocultar elementos visuales sin apagar el GameObject raíz
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
