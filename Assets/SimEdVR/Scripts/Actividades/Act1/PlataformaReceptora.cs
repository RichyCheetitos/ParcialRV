using UnityEngine;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Zona de detección en la plataforma o mesa receptora.
    /// Detecta cuando un CuboNumero es colocado sobre ella e informa a la actividad de sumas.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlataformaReceptora : MonoBehaviour
    {
        [Header("Referencia a la Actividad")]
        [Tooltip("Controlador de la actividad de sumas.")]
        [SerializeField] private Actividad_SumasCubos actividad;

        [Header("Punto de Fijación")]
        [Tooltip("Transform donde se anclará el cubo si la respuesta es correcta. Si es nulo, se usa la posición de esta plataforma.")]
        [SerializeField] private Transform puntoFijacion;

        [Header("Feedback Visual en la Mesa (Opcional)")]
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Color colorNormal = Color.white;
        [SerializeField] private Color colorAcierto = Color.green;
        [SerializeField] private Color colorError = Color.red;

        public Transform PuntoFijacion => puntoFijacion != null ? puntoFijacion : transform;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            if (meshRenderer != null)
            {
                meshRenderer.material.color = colorNormal;
            }
        }

        public void ConfigurarActividad(Actividad_SumasCubos nuevaActividad)
        {
            actividad = nuevaActividad;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (actividad == null) return;

            CuboNumero cubo = other.GetComponent<CuboNumero>();
            if (cubo == null)
            {
                cubo = other.GetComponentInParent<CuboNumero>();
            }

            if (cubo != null)
            {
                actividad.EvaluarCubo(cubo);
            }
        }

        public void MostrarFeedbackVisual(bool correcto)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = correcto ? colorAcierto : colorError;
                if (!correcto)
                {
                    Invoke(nameof(RestaurarColorNormal), 1.5f);
                }
            }
        }

        private void RestaurarColorNormal()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = colorNormal;
            }
        }
    }
}
