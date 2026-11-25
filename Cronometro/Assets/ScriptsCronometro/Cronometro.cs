using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Cronometro : MonoBehaviour
{
    private bool cronometroActivo = false;
    private float tiempoActual = 0f;
    private float direccionTiempo = 1f;

    // Variables para guardar la ida
    private float tiempoIda = 0f;
    private bool idaGuardada = false;

    [Header("Textos Principales")]
    public TMP_Text textoTiempo;
    public TMP_Text textoBotonIniciar;

    [Header("Grosor de las Letras")]
    [Range(0f, 0.5f)]
    public float grosorLetras = 0.2f;

    [Header("Sistema de Vueltas")]
    public GameObject prefabCajaVuelta;
    public Transform contenedorVueltas;

    // Estructura para guardar la ida y lo que queda para llegar a 0
    private struct RegistroTiempo
    {
        public float ida;
        public float restante;
    }

    private List<RegistroTiempo> topTiempos = new List<RegistroTiempo>();

    void Start()
    {
        // Engorda los números principales del cronómetro sin cambiar su color
        if (textoTiempo != null)
        {
            AplicarGrosor(textoTiempo);
        }
    }

    void AplicarGrosor(TMP_Text texto)
    {
        texto.fontStyle |= FontStyles.Bold;
        texto.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, grosorLetras);
    }

    void Update()
    {
        if (cronometroActivo)
        {
            tiempoActual += Time.deltaTime * direccionTiempo;

            if (tiempoActual <= 0f && direccionTiempo == -1f)
            {
                tiempoActual = 0f;
                cronometroActivo = false;
                direccionTiempo = 1f;
                textoBotonIniciar.text = "INICIAR";
            }
        }

        textoTiempo.text = FormatearTiempo(tiempoActual);
    }

    string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60F);
        int segundos = Mathf.FloorToInt(tiempo % 60F);
        int milisegundos = Mathf.FloorToInt((tiempo * 100F) % 100F);
        return string.Format("{0:00}:{1:00}.{2:00}", minutos, segundos, milisegundos);
    }

    public void BotonIniciarPulsado()
    {
        if (cronometroActivo)
        {
            cronometroActivo = false;
            textoBotonIniciar.text = "INICIAR";
        }
        else
        {
            cronometroActivo = true;
            direccionTiempo = 1f;
            textoBotonIniciar.text = "PARAR";
        }
    }

    public void BotonVueltaPulsado()
    {
        if (!cronometroActivo && tiempoActual > 0f)
        {
            // Guardamos la ida solo la primera vez que pulsas VUELTA
            if (!idaGuardada)
            {
                tiempoIda = tiempoActual;
                idaGuardada = true;
            }

            cronometroActivo = true;
            direccionTiempo = -1f;
            textoBotonIniciar.text = "PARAR";
        }
    }

    public void ReiniciarCronometro()
    {
        // Si nunca pulsaste VUELTA pero sí hiciste ida, guardamos esa ida
        if (!idaGuardada && tiempoActual > 0f)
        {
            tiempoIda = tiempoActual;
        }

        if (tiempoIda > 0f)
        {
            GuardarRegistroFinal();
        }

        cronometroActivo = false;
        tiempoActual = 0f;
        tiempoIda = 0f;
        idaGuardada = false;
        direccionTiempo = 1f;
        textoBotonIniciar.text = "INICIAR";

        textoTiempo.text = FormatearTiempo(tiempoActual);
    }

    void GuardarRegistroFinal()
    {
        RegistroTiempo nuevoRegistro = new RegistroTiempo
        {
            ida = tiempoIda,
            restante = tiempoActual // Lo que queda para llegar a 0
        };

        topTiempos.Add(nuevoRegistro);

        // Prioridad 1: tiempo de ida (menor a mayor)
        // Prioridad 2 (empate en ida): cuánto queda para llegar a 0 (menor a mayor)
        topTiempos.Sort((a, b) =>
        {
            int idaA = Mathf.FloorToInt(a.ida * 100f);
            int idaB = Mathf.FloorToInt(b.ida * 100f);

            int comparacionIda = idaA.CompareTo(idaB);
            if (comparacionIda != 0)
            {
                return comparacionIda;
            }

            return a.restante.CompareTo(b.restante);
        });

        if (topTiempos.Count > 5)
        {
            topTiempos.RemoveAt(5);
        }

        ActualizarPanelTop5();
    }

    void ActualizarPanelTop5()
    {
        foreach (Transform hijo in contenedorVueltas)
        {
            Destroy(hijo.gameObject);
        }

        for (int i = 0; i < topTiempos.Count; i++)
        {
            GameObject nuevaCaja = Instantiate(prefabCajaVuelta, contenedorVueltas);
            TMP_Text textoCaja = nuevaCaja.GetComponentInChildren<TMP_Text>();

            string textoIda = FormatearTiempo(topTiempos[i].ida);
            string textoRestante = FormatearTiempo(topTiempos[i].restante);

            textoCaja.text = string.Format("{0}º {1} / {2}", (i + 1), textoIda, textoRestante);
            AplicarGrosor(textoCaja);

            if (i == 0)
            {
                textoCaja.color = new Color(1f, 0.5f, 0f);
            }
            else if (i == topTiempos.Count - 1 && topTiempos.Count > 1)
            {
                textoCaja.color = Color.red;
            }
            else
            {
                textoCaja.color = Color.white;
            }
        }
    }
}