using UnityEngine;
using TMPro;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.UI
{
    public class SimEd_PuntoInformacion : MonoBehaviour
    {
        [Header("IDs de los Puntos de la Sala")]
        public string idPuntoA = "Punto A";
        public string idPuntoB = "Punto B";
        public string idPuntoC = "Punto C";

        [Header("Textos de Estado de los Puntos (UI)")]
        public TextMeshProUGUI txtEstadoPuntoA;
        public TextMeshProUGUI txtEstadoPuntoB;
        public TextMeshProUGUI txtEstadoPuntoC;

        [Header("Texto de Estado Global de la Sala")]
        public TextMeshProUGUI txtEstadoSalaGlobal;

        [Header("Configuración de Mensajes Personalizables")]
        [TextArea(2, 4)]
        public string mensajeSalaPendiente = "Sala pendiente por completar";
        [TextArea(2, 4)]
        public string mensajeSalaCompletada = "¡Sala completada con éxito!";

        [Header("Configuración de Colores")]
        public Color colorCompletado = Color.green;
        public Color colorPendiente = new Color(1.0f, 0.6f, 0.0f); 

        private void OnEnable()
        {
            SimEd_GameManager.OnPuntoCompletado += ListenerPuntoCompletado;
            SimEd_GameManager.OnEstadoSalaActualizado += ListenerEstadoSalaActualizado;
        }

        private void OnDisable()
        {
            SimEd_GameManager.OnPuntoCompletado -= ListenerPuntoCompletado;
            SimEd_GameManager.OnEstadoSalaActualizado -= ListenerEstadoSalaActualizado;
        }

        private void Start()
        {
            
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            ActualizarUI();
        }

        public void ActualizarUI()
        {
            if (SimEd_GameManager.Instance == null) return;

            string hexCompletado = ColorUtility.ToHtmlStringRGB(colorCompletado);
            string hexPendiente = ColorUtility.ToHtmlStringRGB(colorPendiente);

            
            if (txtEstadoPuntoA != null)
            {
                bool compA = SimEd_GameManager.Instance.EsPuntoCompletado(idPuntoA);
                txtEstadoPuntoA.text = $"{idPuntoA}: {(compA ? $"<color=#{hexCompletado}>Completado</color>" : $"<color=#{hexPendiente}>Pendiente</color>")}";
            }

            if (txtEstadoPuntoB != null)
            {
                bool compB = SimEd_GameManager.Instance.EsPuntoCompletado(idPuntoB);
                txtEstadoPuntoB.text = $"{idPuntoB}: {(compB ? $"<color=#{hexCompletado}>Completado</color>" : $"<color=#{hexPendiente}>Pendiente</color>")}";
            }

            if (txtEstadoPuntoC != null)
            {
                bool compC = SimEd_GameManager.Instance.EsPuntoCompletado(idPuntoC);
                txtEstadoPuntoC.text = $"{idPuntoC}: {(compC ? $"<color=#{hexCompletado}>Completado</color>" : $"<color=#{hexPendiente}>Pendiente</color>")}";
            }

            if (txtEstadoSalaGlobal != null)
            {
                if (SimEd_GameManager.Instance.salaCompletada)
                {
                    txtEstadoSalaGlobal.text = $"<color=#{hexCompletado}><b>{mensajeSalaCompletada}</b></color>";
                }
                else
                {
                    txtEstadoSalaGlobal.text = $"<color=#{hexPendiente}><b>{mensajeSalaPendiente}</b></color>";
                }
            }
        }

        private void ListenerPuntoCompletado(string idPunto)
        {
            ActualizarUI();
        }

        private void ListenerEstadoSalaActualizado(bool completada)
        {
            ActualizarUI();
        }
    }
}
