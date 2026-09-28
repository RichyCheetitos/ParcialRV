using UnityEngine;

namespace SimEdVR.Scripts.Managers
{
    /// <summary>
    /// Administrador de audio central de SimEdVR (Singleton).
    /// Escucha los eventos globales del SimEd_GameManager para reproducir sonidos automáticamente cuando:
    /// 1. Se acierta algo (pregunta teórica o desafío práctico).
    /// 2. Se falla algo (respuesta incorrecta o elemento erróneo).
    /// 3. Se completa una sala / punto temático.
    /// 4. Se completan todas las salas del museo.
    /// </summary>
    public class SimEd_AudioManager : MonoBehaviour
    {
        public static SimEd_AudioManager Instance { get; private set; }

        [Header("Fuentes de Audio")]
        [Tooltip("AudioSource para efectos de sonido (SFX). Se crea automáticamente si está vacío.")]
        [SerializeField] private AudioSource sfxSource;

        [Tooltip("AudioSource para fanfarria o música final.")]
        [SerializeField] private AudioSource musicSource;

        [Header("1. Sonidos de Evaluación")]
        [Tooltip("Sonido cuando el usuario acierta una pregunta teórica o actividad práctica.")]
        [SerializeField] private AudioClip clipAcierto;

        [Tooltip("Sonido cuando el usuario se equivoca.")]
        [SerializeField] private AudioClip clipError;

        [Header("2. Sonidos de Avance y Conclusión")]
        [Tooltip("Sonido al completar exitosamente una sala o estación.")]
        [SerializeField] private AudioClip clipSalaCompletada;

        [Tooltip("Fanfarria triunfal al completar el 100% de las salas del museo.")]
        [SerializeField] private AudioClip clipTodasCompletadas;

        [Header("3. Sonidos de Interfaz (Opcional)")]
        [Tooltip("Sonido de clic al presionar botones en VR.")]
        [SerializeField] private AudioClip clipClickUI;

        [Header("Volúmenes")]
        [Range(0f, 1f)] public float volumenAcierto = 1f;
        [Range(0f, 1f)] public float volumenError = 0.8f;
        [Range(0f, 1f)] public float volumenSala = 1f;
        [Range(0f, 1f)] public float volumenFinal = 1f;
        [Range(0f, 1f)] public float volumenClickUI = 0.6f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ConfigurarAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // Suscripción estricta al patrón Observer de SimEd_GameManager
            SimEd_GameManager.OnPreguntaRespondida += ManejarPreguntaRespondida;
            SimEd_GameManager.OnPuntoCompletado += ManejarSalaCompletada;
            SimEd_GameManager.OnTodasSalasCompletadas += ManejarTodasCompletadas;
        }

        private void OnDisable()
        {
            // Desuscripción obligatoria para evitar errores de referencia
            SimEd_GameManager.OnPreguntaRespondida -= ManejarPreguntaRespondida;
            SimEd_GameManager.OnPuntoCompletado -= ManejarSalaCompletada;
            SimEd_GameManager.OnTodasSalasCompletadas -= ManejarTodasCompletadas;
        }

        private void ConfigurarAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // Audio 2D estéreo directo al oído en el casco VR
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = false;
                musicSource.spatialBlend = 0f;
            }
        }

        #region Callbacks del Observer
        private void ManejarPreguntaRespondida(bool correcta)
        {
            if (correcta)
            {
                ReproducirAcierto();
            }
            else
            {
                ReproducirError();
            }
        }

        private void ManejarSalaCompletada(string idSala)
        {
            ReproducirSalaCompletada();
        }

        private void ManejarTodasCompletadas()
        {
            ReproducirTodasCompletadas();
        }
        #endregion

        #region Métodos Públicos de Reproducción
        /// <summary>
        /// Reproduce el sonido de acierto (respuesta o mecánica correcta).
        /// </summary>
        public void ReproducirAcierto()
        {
            Debug.Log("[SimEd_AudioManager] 🎵 Reproduciendo sonido: ACIERTO");
            ReproducirSFX(clipAcierto, volumenAcierto);
        }

        /// <summary>
        /// Reproduce el sonido de error (respuesta o mecánica fallida).
        /// </summary>
        public void ReproducirError()
        {
            Debug.Log("[SimEd_AudioManager] 🎵 Reproduciendo sonido: ERROR");
            ReproducirSFX(clipError, volumenError);
        }

        /// <summary>
        /// Reproduce el sonido de sala temática completada.
        /// </summary>
        public void ReproducirSalaCompletada()
        {
            Debug.Log("[SimEd_AudioManager] 🎵 Reproduciendo sonido: SALA COMPLETADA");
            ReproducirSFX(clipSalaCompletada, volumenSala);
        }

        /// <summary>
        /// Reproduce la fanfarria de fin de experiencia (todas las salas superadas).
        /// </summary>
        public void ReproducirTodasCompletadas()
        {
            Debug.Log("[SimEd_AudioManager] 🎵 Reproduciendo: TODAS LAS SALAS COMPLETADAS (Fanfarria)");
            if (clipTodasCompletadas != null && musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = clipTodasCompletadas;
                musicSource.volume = volumenFinal;
                musicSource.Play();
            }
            else
            {
                ReproducirSFX(clipTodasCompletadas, volumenFinal);
            }
        }

        /// <summary>
        /// Sonido de clic en botones de UI en VR.
        /// </summary>
        public void ReproducirClickUI()
        {
            ReproducirSFX(clipClickUI, volumenClickUI);
        }

        public void ReproducirSFX(AudioClip clip, float volumen = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumen);
            }
            else if (clip == null)
            {
                Debug.LogWarning("[SimEd_AudioManager] Se intentó reproducir un sonido, pero el AudioClip no está asignado en el Inspector.");
            }
        }
        #endregion

        #region Menús de Prueba en el Editor (Clic derecho en el componente)
        [ContextMenu("Probar Sonido Acierto")]
        private void TestAcierto() => ReproducirAcierto();

        [ContextMenu("Probar Sonido Error")]
        private void TestError() => ReproducirError();

        [ContextMenu("Probar Sala Completada")]
        private void TestSala() => ReproducirSalaCompletada();

        [ContextMenu("Probar Todas Completadas")]
        private void TestTodas() => ReproducirTodasCompletadas();
        #endregion
    }
}
