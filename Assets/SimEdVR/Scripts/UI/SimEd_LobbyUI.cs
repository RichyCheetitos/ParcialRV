using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimEdVR.Scripts.Managers;

namespace SimEdVR.Scripts.UI
{
    public class SimEd_LobbyUI : MonoBehaviour
    {
        [Header("Paneles UI (World Space)")]
        public GameObject panelBienvenida;
        public GameObject panelFinExperiencia;

        [Header("Textos")]
        public TextMeshProUGUI txtMensajeBienvenida;
        public TextMeshProUGUI txtMensajeFelicitaciones;

        [Header("Botones UI")]
        public Button btnJugar;
        public Button btnSalirInicial;
        public Button btnSalirFinal;

        [Header("Teleport Locomotion Provider")]
        public GameObject teleportProvider;

        private void OnEnable()
        {
            SimEdVR.Scripts.Managers.SimEd_GameManager.OnEstadoSalaActualizado += ListenerEstadoSala;
        }

        private void OnDisable()
        {
            SimEdVR.Scripts.Managers.SimEd_GameManager.OnEstadoSalaActualizado -= ListenerEstadoSala;
        }

        private void Start()
        {
           
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            if (panelBienvenida != null) panelBienvenida.SetActive(true);
            if (panelFinExperiencia != null) panelFinExperiencia.SetActive(false);

            if (btnJugar != null) btnJugar.onClick.AddListener(OnBotonJugarPresionado);
            if (btnSalirInicial != null) btnSalirInicial.onClick.AddListener(OnBotonSalirPresionado);
            if (btnSalirFinal != null) btnSalirFinal.onClick.AddListener(OnBotonSalirPresionado);

            if (teleportProvider != null) teleportProvider.SetActive(false);
        }

        public void OnBotonJugarPresionado()
        {
            if (panelBienvenida != null) panelBienvenida.SetActive(false);
            if (teleportProvider != null) teleportProvider.SetActive(true);

            if (SimEd_GameManager.Instance != null)
            {
                SimEd_GameManager.Instance.IniciarJuego();
            }
        }

        public void OnBotonSalirPresionado()
        {
            Debug.Log("Cerrando la aplicación...");
            Application.Quit();
        }

        private void ListenerEstadoSala(bool completada)
        {
            if (completada)
            {
                if (panelFinExperiencia != null) panelFinExperiencia.SetActive(true);
            }
        }
    }
}
