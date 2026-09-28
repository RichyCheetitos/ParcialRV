using UnityEngine;
using TMPro;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Representa un cubo interactivo numerado para la actividad de sumas.
    /// Puede ser manipulado mediante XRGrabInteractable en VR o con físicas estándar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CuboNumero : MonoBehaviour
    {
        [Header("Valor Numérico")]
        [Tooltip("El número que representa este cubo.")]
        public int valorNumero = 0;

        [Header("Referencias Visuales (Opcional)")]
        [Tooltip("Textos TMP en las caras del cubo donde se muestra el número.")]
        public TextMeshProUGUI[] textosUI;
        public TextMeshPro[] textos3D;

        // Memorizar transformación inicial para restablecer si se cae o falla
        private Vector3 _posicionInicial;
        private Quaternion _rotacionInicial;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _posicionInicial = transform.position;
            _rotacionInicial = transform.rotation;
        }

        private void Start()
        {
            ActualizarVisual();
        }

        private void OnValidate()
        {
            ActualizarVisual();
        }

        /// <summary>
        /// Asigna el valor numérico al cubo y actualiza sus etiquetas.
        /// </summary>
        public void ConfigurarNumero(int valor)
        {
            valorNumero = valor;
            ActualizarVisual();
        }

        public void ActualizarVisual()
        {
            string textoStr = valorNumero.ToString();

            if (textosUI != null)
            {
                foreach (var t in textosUI)
                {
                    if (t != null) t.text = textoStr;
                }
            }

            if (textos3D != null)
            {
                foreach (var t in textos3D)
                {
                    if (t != null) t.text = textoStr;
                }
            }
        }

        /// <summary>
        /// Guarda la posición actual como la nueva posición de reposo inicial.
        /// </summary>
        public void GuardarPosicionInicial()
        {
            _posicionInicial = transform.position;
            _rotacionInicial = transform.rotation;
        }

        /// <summary>
        /// Regresa el cubo a su posición y rotación original con velocidad cero.
        /// </summary>
        public void ReiniciarPosicion()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            transform.position = _posicionInicial;
            transform.rotation = _rotacionInicial;
        }

        /// <summary>
        /// Fija el cubo en la mesa cuando es la respuesta correcta para evitar que se mueva.
        /// </summary>
        public void BloquearEnPlataforma(Vector3 posicionObjetivo)
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
            transform.position = posicionObjetivo;
        }
    }
}
