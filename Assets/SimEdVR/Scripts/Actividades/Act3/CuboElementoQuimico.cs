using UnityEngine;
using TMPro;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Representa un cubo interactivo 3D con un elemento de la tabla periódica (H, O, C, N, etc.).
    /// Se manipula mediante XRGrabInteractable en VR.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CuboElementoQuimico : MonoBehaviour
    {
        [Header("Datos Químicos")]
        [Tooltip("Símbolo químico del elemento (ej: 'H', 'O', 'C', 'N').")]
        public string simboloElemento = "H";

        [Tooltip("Nombre legible del elemento (ej: 'Hidrógeno', 'Oxígeno').")]
        public string nombreElemento = "Hidrógeno";

        [Header("Referencias Visuales")]
        [Tooltip("Textos TMP en las caras del cubo que muestran el símbolo.")]
        public TextMeshProUGUI[] textosUI;
        public TextMeshPro[] textos3D;

        [Tooltip("Renderer para colorear el cubo según la convención química (opcional).")]
        public Renderer meshRenderer;

        [Header("Color Personalizado (Opcional)")]
        public bool usarColorPersonalizado = true;
        public Color colorElemento = Color.white;

        private Vector3 _posicionInicial;
        private Quaternion _rotacionInicial;
        private Rigidbody _rb;
        private bool _bloqueadoEnRanura = false;

        public bool BloqueadoEnRanura => _bloqueadoEnRanura;

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

        public void ConfigurarElemento(string simbolo, string nombre, Color color)
        {
            simboloElemento = simbolo;
            nombreElemento = nombre;
            colorElemento = color;
            ActualizarVisual();
        }

        public void ActualizarVisual()
        {
            if (textosUI != null)
            {
                foreach (var t in textosUI)
                {
                    if (t != null) t.text = simboloElemento;
                }
            }

            if (textos3D != null)
            {
                foreach (var t in textos3D)
                {
                    if (t != null) t.text = simboloElemento;
                }
            }

            if (meshRenderer != null && usarColorPersonalizado)
            {
                meshRenderer.material.color = colorElemento;
            }
        }

        public void GuardarPosicionInicial()
        {
            _posicionInicial = transform.position;
            _rotacionInicial = transform.rotation;
        }

        public void ReiniciarPosicion()
        {
            if (_bloqueadoEnRanura) return;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = false;
            }

            transform.position = _posicionInicial;
            transform.rotation = _rotacionInicial;
        }

        public void BloquearEnRanura(Transform puntoRanura)
        {
            _bloqueadoEnRanura = true;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            transform.position = puntoRanura.position;
            transform.rotation = puntoRanura.rotation;
        }

        public void LiberarDeRanura()
        {
            _bloqueadoEnRanura = false;
            if (_rb != null)
            {
                _rb.isKinematic = false;
            }
            ReiniciarPosicion();
        }
    }
}
