using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimEdVR.Scripts.Core
{
    [Serializable]
    public class PreguntaData
    {
        [TextArea(3, 5)]
        public string textoTeorico;
        
        [TextArea(2, 4)]
        public string enunciadoPregunta;
        
        public string respuesta1;
        public string respuesta2;
        public string respuesta3;
        
        [Range(0, 2)]
        [Tooltip("Índice de la respuesta correcta: 0 para Respuesta 1, 1 para Respuesta 2, 2 para Respuesta 3")]
        public int indiceRespuestaCorrecta;

        /// <summary>
        /// Obtiene el texto de una respuesta según su índice (0, 1 o 2).
        /// </summary>
        public string ObtenerRespuesta(int indice)
        {
            switch (indice)
            {
                case 0: return respuesta1;
                case 1: return respuesta2;
                case 2: return respuesta3;
                default: return string.Empty;
            }
        }
    }

    [CreateAssetMenu(fileName = "NuevoPuntoAprendizajeData", menuName = "SimEdVR/PuntoAprendizajeData", order = 1)]
    public class SimEd_PreguntaSO : ScriptableObject
    {
        [Header("Información del Punto de Aprendizaje")]
        public string idPunto = "Punto A";
        public string tema;
        [TextArea(3, 6)]
        public string descripcion;
        public Sprite imagen;

        [Header("Lista de Preguntas (Mínimo 2)")]
        public List<PreguntaData> preguntas = new List<PreguntaData>();
    }
}
