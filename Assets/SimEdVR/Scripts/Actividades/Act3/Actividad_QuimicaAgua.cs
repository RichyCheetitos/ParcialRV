using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using SimEdVR.Scripts.Core;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Actividad Práctica 2: Construcción de la molécula del agua (H2O).
    /// Requiere colocar dos cubos de Hidrógeno (H) y uno de Oxígeno (O) en las ranuras de la mesa,
    /// entre cubos distractores de otros elementos químicos.
    /// Deriva de BaseActividadInteractiva y se integra con SimEd_GameManager.
    /// </summary>
    public class Actividad_QuimicaAgua : BaseActividadInteractiva
    {
        [Header("Identificador de Punto")]
        [Tooltip("ID para registrar el progreso en el GameManager / PuntoInformacion.")]
        public string idPunto = "Punto I";

        [Header("Canvas World Space de la Actividad")]
        [SerializeField] private Canvas canvasActividad;
        [SerializeField] private TextMeshProUGUI txtTitulo;
        [SerializeField] private TextMeshProUGUI txtDescripcion;
        [SerializeField] private TextMeshProUGUI txtFeedback;
        [SerializeField] private Button btnEmpezar;

        [Header("Fórmula Química Requerida")]
        [Tooltip("Orden de los elementos en las ranuras de la mesa (por defecto: H, H, O para H2O).")]
        public string[] formulaEsperada = new string[3] { "H", "H", "O" };

        [Header("Elementos 3D en la Escena")]
        [Tooltip("Las 3 casillas o ranuras sobre la mesa receptora.")]
        public RanuraElementoQuimico[] ranuras;

        [Tooltip("Todos los cubos de elementos presentes en la estación (incluyendo distractores como C, N, etc.).")]
        public CuboElementoQuimico[] cubosDisponibles;

        [Tooltip("Contenedor raíz opcional de los cubos y mesa.")]
        public GameObject contenedorInteractivos3D;

        private bool _estaCompletada = false;
        private bool _actividadIniciada = false;
        private bool _reiniciandoPorError = false;
        private Coroutine _coroutineResetMesa;

        public override bool EstaCompletada => _estaCompletada;

        private void Start()
        {
            ConfigurarCanvas();
            RegistrarEnGameManager();
            InicializarTextosUI();
            InicializarRanuras();
            InicializarCubos();

            if (btnEmpezar != null)
            {
                btnEmpezar.onClick.AddListener(OnClickEmpezar);
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
                txtTitulo.text = "Actividad Práctica: Enlace Químico del Agua";
            }

            if (txtDescripcion != null)
            {
                string formulaStr = string.Join(" + ", formulaEsperada);
                txtDescripcion.text = $"<b>Desafío:</b> ¿Cuál es la fórmula química del <b>Agua</b>?\n\n" +
                                      $"Encuentra entre los cubos los elementos correctos (<b>dos de Hidrógeno 'H' y uno de Oxígeno 'O'</b>) " +
                                      $"y colócalos en orden sobre la mesa receptora.\n" +
                                      $"<i>¡Ten cuidado con los elementos distractores!</i>";
            }

            if (txtFeedback != null)
            {
                txtFeedback.text = "Presiona 'Empezar' para habilitar los cubos de elementos.";
            }

            if (btnEmpezar != null)
            {
                btnEmpezar.gameObject.SetActive(true);
                btnEmpezar.interactable = true;
            }
        }

        private void InicializarRanuras()
        {
            if (ranuras == null) return;

            for (int i = 0; i < ranuras.Length; i++)
            {
                if (ranuras[i] != null)
                {
                    string simbolo = (i < formulaEsperada.Length) ? formulaEsperada[i] : "H";
                    ranuras[i].ConfigurarRanura(this, i, simbolo);
                }
            }
        }

        private void InicializarCubos()
        {
            if (cubosDisponibles == null) return;

            foreach (var c in cubosDisponibles)
            {
                if (c != null)
                {
                    c.GuardarPosicionInicial();
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
                txtFeedback.text = "<color=#F39C12><b>¡Actividad en curso!</b> Toma los cubos con 'H' y 'O' y colócalos en las casillas correspondientes.</color>";
            }

            if (contenedorInteractivos3D != null)
            {
                contenedorInteractivos3D.SetActive(true);
            }

            if (SimEd_GameManager.Instance != null && !string.IsNullOrEmpty(idPunto))
            {
                SimEd_GameManager.Instance.NotificarActividadIniciada(idPunto);
            }

            Debug.Log("[Actividad_QuimicaAgua] Actividad iniciada. Esperando construcción de H2O.");
        }

        public override void ReiniciarActividad()
        {
            if (_coroutineResetMesa != null)
            {
                StopCoroutine(_coroutineResetMesa);
                _coroutineResetMesa = null;
            }
            _reiniciandoPorError = false;
            _estaCompletada = false;
            _actividadIniciada = false;

            InicializarTextosUI();

            if (ranuras != null)
            {
                foreach (var r in ranuras)
                {
                    if (r != null) r.LiberarRanura();
                }
            }

            if (cubosDisponibles != null)
            {
                foreach (var c in cubosDisponibles)
                {
                    if (c != null) c.ReiniciarPosicion();
                }
            }

            NotificarActividadReiniciada();
        }

        /// <summary>
        /// Evalúa si el cubo que ingresó a una ranura coincide con el elemento esperado.
        /// </summary>
        public void EvaluarCuboEnRanura(CuboElementoQuimico cubo, RanuraElementoQuimico ranura)
        {
            if (!_actividadIniciada || _estaCompletada || _reiniciandoPorError || cubo == null || ranura == null) return;

            // Comparar ignorando mayúsculas/minúsculas
            bool coincide = string.Equals(cubo.simboloElemento.Trim(), ranura.simboloEsperado.Trim(), StringComparison.OrdinalIgnoreCase);

            if (coincide)
            {
                // Acierto en esta ranura
                ranura.FijarCuboCorrecto(cubo);

                // Audio feedback positivo
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(true);
                }

                int completadas = ContarRanurasCompletadas();
                int total = ranuras.Length;

                if (completadas >= total)
                {
                    // ¡Molécula completa superada!
                    _estaCompletada = true;

                    if (txtFeedback != null)
                    {
                        txtFeedback.text = "<color=#2ECC71><b>¡EXCELENTE! Has formado correctamente la molécula de H₂O (Agua).</b>\n¡Fórmula química completada con éxito!</color>";
                    }

                    if (SimEd_GameManager.Instance != null && !string.IsNullOrEmpty(idPunto))
                    {
                        SimEd_GameManager.Instance.RegistrarPuntoCompletado(idPunto);
                    }

                    Debug.Log($"[Actividad_QuimicaAgua] ¡Molécula completada! Punto '{idPunto}' registrado.");
                    NotificarActividadSuperada();
                }
                else
                {
                    if (txtFeedback != null)
                    {
                        txtFeedback.text = $"<color=#2ECC71><b>¡Muy bien!</b> Has colocado el elemento <b>{cubo.nombreElemento} ({cubo.simboloElemento})</b>.</color>\nProgreso: <b>{completadas}/{total}</b> elementos en la mesa.";
                    }
                }
            }
            else
            {
                // Error: Elemento incorrecto o distractor
                ranura.MostrarFeedbackError();

                if (txtFeedback != null)
                {
                    txtFeedback.text = $"<color=#E74C3C><b>Elemento incorrecto:</b> {cubo.nombreElemento} ({cubo.simboloElemento}) no va en esta casilla.\nSe requiere: <b>{ranura.simboloEsperado}</b>.\n<i>Reiniciando la mesa en 2 segundos...</i></color>";
                }

                // Audio error
                if (SimEd_GameManager.Instance != null)
                {
                    SimEd_GameManager.Instance.NotificarPreguntaRespondida(false);
                }

                // Reiniciar toda la mesa tras 2 segundos para permitir reintentar limpiamente
                if (_coroutineResetMesa != null) StopCoroutine(_coroutineResetMesa);
                _coroutineResetMesa = StartCoroutine(RutinaReiniciarMesaTrasError(2.0f));
            }
        }

        private int ContarRanurasCompletadas()
        {
            if (ranuras == null) return 0;
            int contador = 0;
            foreach (var r in ranuras)
            {
                if (r != null && r.EstaOcupadaCorrectamente) contador++;
            }
            return contador;
        }

        private IEnumerator RutinaReiniciarMesaTrasError(float retraso)
        {
            _reiniciandoPorError = true;
            yield return new WaitForSeconds(retraso);

            if (!_estaCompletada)
            {
                if (ranuras != null)
                {
                    foreach (var r in ranuras)
                    {
                        if (r != null) r.LiberarRanura();
                    }
                }

                if (cubosDisponibles != null)
                {
                    foreach (var c in cubosDisponibles)
                    {
                        if (c != null) c.ReiniciarPosicion();
                    }
                }

                if (txtFeedback != null)
                {
                    txtFeedback.text = "<color=#F39C12><b>Mesa reiniciada.</b> Vuelve a intentar colocando los elementos correctos (H - H - O).</color>";
                }
            }

            _reiniciandoPorError = false;
            _coroutineResetMesa = null;
        }
    }
}
