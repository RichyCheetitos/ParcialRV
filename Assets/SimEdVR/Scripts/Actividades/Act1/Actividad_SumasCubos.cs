using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using SimEdVR.Scripts.Core;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Actividad Práctica 1: Resolución de sumas colocando cubos 3D interactivos en una plataforma.
    /// Deriva de BaseActividadInteractiva y se integra con SimEd_GameManager y SimEd_PuntoInformacion
    /// como el 3er punto de avance de la Sala 1 (por defecto: 'Punto C').
    /// </summary>
    public class Actividad_SumasCubos : BaseActividadInteractiva
    {
        [Header("Identificador de Punto en SalaInfo")]
        [Tooltip("ID del punto que representa esta actividad en GameManager y PuntoInformacion (ej: 'Punto C').")]
        public string idPunto = "Punto C";

        [Header("Canvas World Space de la Actividad")]
        [SerializeField] private Canvas canvasActividad;
        [SerializeField] private GameObject panelUI;
        [SerializeField] private TextMeshProUGUI txtTitulo;
        [SerializeField] private TextMeshProUGUI txtDescripcion;
        [SerializeField] private TextMeshProUGUI txtFeedback;
        [SerializeField] private Button btnEmpezar;

        [Header("Contenido Pedagógico de la Suma")]
        [Tooltip("Primer sumando.")]
        public int sumandoA = 2;

        [Tooltip("Segundo sumando.")]
        public int sumandoB = 3;

        [Tooltip("Valores para los 3 cubos (uno debe ser exactamente la suma correcta).")]
        public int[] valoresCubos = new int[3] { 4, 5, 7 };

        [Header("Elementos 3D en Escena")]
        [Tooltip("Los 3 cubos numerados disponibles para manipular.")]
        public CuboNumero[] cubos;

        [Tooltip("La plataforma o mesa donde el usuario debe colocar el cubo.")]
        public PlataformaReceptora plataforma;

        [Tooltip("Contenedor raíz opcional de los cubos y plataforma (para activarlos/desactivarlos si se desea).")]
        public GameObject contenedorInteractivos3D;

        private bool _estaCompletada = false;
        private bool _actividadIniciada = false;
        private Coroutine _coroutineResetCubo;

        public override bool EstaCompletada => _estaCompletada;
        public int ResultadoCorrecto => sumandoA + sumandoB;

        private void Start()
        {
            ConfigurarCanvas();
            RegistrarEnGameManager();
            InicializarTextosUI();
            InicializarCubos();

            if (btnEmpezar != null)
            {
                btnEmpezar.onClick.AddListener(OnClickEmpezar);
            }

            if (plataforma != null)
            {
                plataforma.ConfigurarActividad(this);
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
                txtTitulo.text = "Actividad Práctica: Sumas";
            }

            if (txtDescripcion != null)
            {
                txtDescripcion.text = $"<b>Desafío:</b> ¿Cuánto es <b>{sumandoA} + {sumandoB}</b>?\n\nObserva los 3 cubos sobre la mesa, toma el que tenga el resultado correcto y colócalo sobre la plataforma receptora.";
            }

            if (txtFeedback != null)
            {
                txtFeedback.text = "Presiona 'Empezar' para habilitar los cubos interactivos.";
            }

            if (btnEmpezar != null)
            {
                btnEmpezar.gameObject.SetActive(true);
                btnEmpezar.interactable = true;
            }
        }

        private void InicializarCubos()
        {
            if (cubos == null || cubos.Length == 0) return;

            // Asegurarse de que al menos uno de los cubos tenga el resultado correcto
            bool resultadoIncluido = false;
            for (int i = 0; i < cubos.Length && i < valoresCubos.Length; i++)
            {
                if (valoresCubos[i] == ResultadoCorrecto)
                {
                    resultadoIncluido = true;
                    break;
                }
            }

            if (!resultadoIncluido && valoresCubos.Length > 0)
            {
                valoresCubos[0] = ResultadoCorrecto;
            }

            for (int i = 0; i < cubos.Length; i++)
            {
                if (cubos[i] != null)
                {
                    int val = (i < valoresCubos.Length) ? valoresCubos[i] : ResultadoCorrecto;
                    cubos[i].ConfigurarNumero(val);
                    cubos[i].GuardarPosicionInicial();
                }
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
                txtFeedback.text = "<color=#F39C12><b>¡Actividad en curso!</b> Agarra el cubo con la respuesta y colócalo en la plataforma.</color>";
            }

            if (contenedorInteractivos3D != null)
            {
                contenedorInteractivos3D.SetActive(true);
            }

            if (SimEd_GameManager.Instance != null && !string.IsNullOrEmpty(idPunto))
            {
                SimEd_GameManager.Instance.NotificarActividadIniciada(idPunto);
            }

            Debug.Log($"[Actividad_SumasCubos] Actividad iniciada. Operación: {sumandoA} + {sumandoB} = {ResultadoCorrecto}");
        }

        public override void ReiniciarActividad()
        {
            _estaCompletada = false;
            _actividadIniciada = false;

            InicializarTextosUI();

            if (cubos != null)
            {
                foreach (var c in cubos)
                {
                    if (c != null) c.ReiniciarPosicion();
                }
            }

            NotificarActividadReiniciada();
        }

        /// <summary>
        /// Evalúa el cubo colocado en la plataforma receptora.
        /// </summary>
        public void EvaluarCubo(CuboNumero cubo)
        {
            if (!_actividadIniciada || _estaCompletada || cubo == null) return;

            if (cubo.valorNumero == ResultadoCorrecto)
            {
                // Acierto
                _estaCompletada = true;

                if (txtFeedback != null)
                {
                    txtFeedback.text = $"<color=#2ECC71><b>¡CORRECTO! {sumandoA} + {sumandoB} = {ResultadoCorrecto}.</b>\n¡Has completado con éxito la actividad práctica!</color>";
                }

                if (plataforma != null)
                {
                    plataforma.MostrarFeedbackVisual(true);
                    cubo.BloquearEnPlataforma(plataforma.PuntoFijacion.position);
                }

                // Audio de éxito
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(true);

                    // Registrar como 3er punto completado para SalaInfo / Tablero
                    SimEd_GameManager.Instance.RegistrarPuntoCompletado(idPunto);
                }

                Debug.Log($"[Actividad_SumasCubos] ¡Respuesta correcta colocada! Punto '{idPunto}' completado.");
                NotificarActividadSuperada();
            }
            else
            {
                // Error
                if (txtFeedback != null)
                {
                    txtFeedback.text = $"<color=#E74C3C><b>Incorrecto:</b> {sumandoA} + {sumandoB} no es {cubo.valorNumero}.\nIntenta con otro cubo.</color>";
                }

                if (plataforma != null)
                {
                    plataforma.MostrarFeedbackVisual(false);
                }

                // Audio de error
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(false);
                }

                // Regresar el cubo incorrecto tras 1.2 segundos para liberar la mesa
                if (_coroutineResetCubo != null)
                {
                    StopCoroutine(_coroutineResetCubo);
                }
                _coroutineResetCubo = StartCoroutine(RutinaReiniciarCuboErroneo(cubo));
            }
        }

        private IEnumerator RutinaReiniciarCuboErroneo(CuboNumero cubo)
        {
            yield return new WaitForSeconds(1.2f);
            if (!_estaCompletada && cubo != null)
            {
                cubo.ReiniciarPosicion();
            }
        }
    }
}
