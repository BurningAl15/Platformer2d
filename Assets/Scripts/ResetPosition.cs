using UnityEngine;

/// <summary>
/// Guarda la posición inicial del GameObject al arrancar y
/// la restaura cuando se llama a Reset() o cuando el objeto
/// se reactiva (OnEnable).
///
/// Útil para obstáculos que deben volver a su posición de inicio
/// después de que el jugador muere y respawnea.
/// </summary>
public class ResetPosition : MonoBehaviour
{
    [Tooltip("Si está activo, resetea la posición automáticamente cuando el GameObject se reactiva.")]
    [SerializeField] private bool resetOnEnable = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (resetOnEnable)
            Reset();
    }

    /// <summary>Restaura posición y rotación iniciales.</summary>
    public void Reset()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
