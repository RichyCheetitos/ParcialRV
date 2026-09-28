using UnityEngine;
using TMPro;

namespace SimEdVR.Scripts.Actividades
{
    /// <summary>
    /// Representa una ranura o casilla de colocación en la mesa química (ej: Ranura 1 [H], Ranura 2 [H], Ranura 3 [O]).
    /// Cuenta con un trigger de detección para evaluar si el elemento colocado es el que corresponde en la fórmula.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RanuraElementoQuimico : MonoBehaviour
    {
        [Header("Configuración de la Ranura")]
        [Tooltip("Índice de posición en la fórmula (0 para la 1ra, 1 para la 2da, etc.).")]
        public int indiceRanura = 0;

        [Tooltip("Símbolo que debe colocarse en esta casilla (ej: 'H' o 'O').")]
        public string simboloEsperado = "H";

        [Header("Punto de Fijación")]
        [Tooltip("Transform exacto donde encaja el cubo. Si es nulo, usa la posición de esta ranura.")]
        [SerializeField] private Transform puntoFijacion;

        [Header("Guía Visual (Opcional)")]
        [Tooltip("Texto guía que muestra qué elemento va aquí (ej: 'H').")]
        [SerializeField] private TextMeshPro textoGuia;

        [Tooltip("Renderer de la ranura para cambiar de color al estar ocupada correctamente.")]
        [SerializeField] private Renderer meshIndicador;
        [SerializeField] private Color colorVacia = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color colorCorrecta = Color.green;
        [SerializeField] private Color colorIncorrecta = Color.red;

        private Actividad_QuimicaAgua _actividad;
        private CuboElementoQuimico _cuboOcupante;
        private bool _estaOcupadaCorrectamente = false;

        public Transform PuntoFijacion => puntoFijacion != null ? puntoFijacion : transform;
        public bool EstaOcupadaCorrectamente => _estaOcupadaCorrectamente;
        public CuboElementoQuimico CuboOcupante => _cuboOcupante;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            ActualizarVisualVacia();
        }

        public void ConfigurarRanura(Actividad_QuimicaAgua actividad, int indice, string simbolo)
        {
            _actividad = actividad;
            indiceRanura = indice;
            simboloEsperado = simbolo;

            if (textoGuia != null)
            {
                textoGuia.text = simboloEsperado;
            }

            ActualizarVisualVacia();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_actividad == null || _estaOcupadaCorrectamente) return;

            CuboElementoQuimico cubo = other.GetComponent<CuboElementoQuimico>();
            if (cubo == null)
            {
                cubo = other.GetComponentInParent<CuboElementoQuimico>();
            }

            if (cubo != null && !cubo.BloqueadoEnRanura)
            {
                _actividad.EvaluarCuboEnRanura(cubo, this);
            }
        }

        public void FijarCuboCorrecto(CuboElementoQuimico cubo)
        {
            _cuboOcupante = cubo;
            _estaOcupadaCorrectamente = true;
            cubo.BloquearEnRanura(PuntoFijacion);

            if (meshIndicador != null)
            {
                meshIndicador.material.color = colorCorrecta;
            }
        }

        public void MostrarFeedbackError()
        {
            if (meshIndicador != null)
            {
                meshIndicador.material.color = colorIncorrecta;
                Invoke(nameof(ActualizarVisualVacia), 1.2f);
            }
        }

        public void LiberarRanura()
        {
            if (_cuboOcupante != null)
            {
                _cuboOcupante.LiberarDeRanura();
                _cuboOcupante = null;
            }
            _estaOcupadaCorrectamente = false;
            ActualizarVisualVacia();
        }

        private void ActualizarVisualVacia()
        {
            if (meshIndicador != null && !_estaOcupadaCorrectamente)
            {
                meshIndicador.material.color = colorVacia;
            }
        }
    }
}
