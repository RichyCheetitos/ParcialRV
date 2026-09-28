using UnityEngine;

namespace SimEdVR.Scripts.Managers
{
    /// <summary>
    /// Administrador de audio 2D/3D de SimEdVR (Singleton).
    /// Observa eventos de SimEd_GameManager para retroalimentación auditiva inmediata en VR.
    /// </summary>
    public class SimEd_AudioManager : MonoBehaviour
    {
        public static SimEd_AudioManager Instance { get; private set; }

        [Header("Fuentes de Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Clips de Retroalimentación en Preguntas")]
        [SerializeField] private AudioClip clipAcierto;
        [SerializeField] private AudioClip clipError;

        [Header("Clips de Logros y Avance")]
        [SerializeField] private AudioClip clipClickUI;
        [SerializeField] private AudioClip clipPuntoCompletado;
        [SerializeField] private AudioClip clipFanfarriaFinal;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                VerificarAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SimEd_GameManager.OnPreguntaRespondida += ManejarPreguntaRespondida;
            SimEd_GameManager.OnPuntoCompletado += ManejarPuntoCompletado;
            SimEd_GameManager.OnTodasSalasCompletadas += ManejarTodasCompletadas;
        }

        private void OnDisable()
        {
            SimEd_GameManager.OnPreguntaRespondida -= ManejarPreguntaRespondida;
            SimEd_GameManager.OnPuntoCompletado -= ManejarPuntoCompletado;
            SimEd_GameManager.OnTodasSalasCompletadas -= ManejarTodasCompletadas;
        }

        private void VerificarAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = false;
            }
        }

        private void ManejarPreguntaRespondida(bool correcta)
        {
            if (correcta) ReproducirSFX(clipAcierto);
            else ReproducirSFX(clipError);
        }

        private void ManejarPuntoCompletado(string idPunto)
        {
            ReproducirSFX(clipPuntoCompletado);
        }

        private void ManejarTodasCompletadas()
        {
            if (clipFanfarriaFinal != null && musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = clipFanfarriaFinal;
                musicSource.Play();
            }
            else
            {
                ReproducirSFX(clipFanfarriaFinal);
            }
        }

        public void ReproducirSFX(AudioClip clip, float volumen = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumen);
            }
        }

        public void ReproducirClickUI()
        {
            ReproducirSFX(clipClickUI, 0.7f);
        }
    }
}
