using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Plataforma interactiva en el suelo para la actividad de Historia (VERDADERO o FALSO).
    /// Detecta la llegada del jugador mediante:
    /// 1. Teletransporte con TeleportationAnchor (al apuntar y soltar el rayo de teleport).
    /// 2. Entrada física por Trigger Collider (OnTriggerEnter) para locomoción continua o room-scale.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlataformaRespuestaVF : MonoBehaviour
    {
        [Header("Valor de la Plataforma")]
        [Tooltip("True para la plataforma VERDADERO, False para la plataforma FALSO.")]
        public bool esPlataformaVerdadero = true;

        [Header("Referencias")]
        [Tooltip("Controlador de la actividad de Historia.")]
        [SerializeField] private Actividad_HistoriaVF actividad;

        [Tooltip("Componente TeleportationAnchor adjunto a esta plataforma o en un hijo.")]
        [SerializeField] private TeleportationAnchor teleportAnchor;

        [Header("Visuales")]
        [Tooltip("Texto 3D o World Space con la etiqueta 'VERDADERO' o 'FALSO'.")]
        [SerializeField] private TextMeshPro textoEtiqueta;

        [Tooltip("Renderer para iluminar la plataforma al interactuar.")]
        [SerializeField] private Renderer meshRenderer;

        [Header("Colores")]
        [SerializeField] private Color colorNormal = Color.white;
        [SerializeField] private Color colorAcierto = Color.green;
        [SerializeField] private Color colorError = Color.red;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (teleportAnchor == null)
            {
                teleportAnchor = GetComponent<TeleportationAnchor>() ?? GetComponentInChildren<TeleportationAnchor>();
            }

            ActualizarEtiquetaVisual();
            RestaurarColorNormal();
        }

        private void OnEnable()
        {
            if (teleportAnchor != null)
            {
                teleportAnchor.teleporting.AddListener(OnTeleportingToAnchor);
            }
        }

        private void OnDisable()
        {
            if (teleportAnchor != null)
            {
                teleportAnchor.teleporting.RemoveListener(OnTeleportingToAnchor);
            }
        }

        public void ConfigurarActividad(Actividad_HistoriaVF nuevaActividad)
        {
            actividad = nuevaActividad;
        }

        private void ActualizarEtiquetaVisual()
        {
            if (textoEtiqueta != null)
            {
                textoEtiqueta.text = esPlataformaVerdadero ? "VERDADERO" : "FALSO";
            }
        }

        #region Detección de Llegada del Jugador
        // 1. Detección por TeleportationAnchor (XRI)
        private void OnTeleportingToAnchor(TeleportingEventArgs args)
        {
            NotificarEleccion();
        }

        // 2. Detección por Trigger Físico (Caminar / Locomoción)
        private void OnTriggerEnter(Collider other)
        {
            // Detectar si lo que entró es el jugador (Main Camera, CharacterController, o XR Origin)
            if (other.CompareTag("Player") || 
                other.GetComponent<CharacterController>() != null || 
                other.GetComponentInParent<Camera>() != null ||
                (Camera.main != null && (other.transform == Camera.main.transform || other.transform.IsChildOf(Camera.main.transform.root))))
            {
                NotificarEleccion();
            }
        }

        private void NotificarEleccion()
        {
            if (actividad != null)
            {
                actividad.EvaluarRespuesta(this);
            }
        }
        #endregion

        #region Feedback Visual
        public void MostrarFeedbackVisual(bool correcta)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = correcta ? colorAcierto : colorError;

                if (!correcta)
                {
                    Invoke(nameof(RestaurarColorNormal), 1.5f);
                }
            }
        }

        public void RestaurarColorNormal()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = colorNormal;
            }
        }
        #endregion
    }
}
