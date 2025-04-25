using UnityEngine;
using UnityEngine.Video; // Necesario para VideoPlayer
using System.Collections.Generic; // Necesario para List<>

// Asegurarse de que el GameObject tenga un VideoPlayer si se quiere usar la función automática
// [RequireComponent(typeof(VideoPlayer))] // Puedes añadirlo si siempre habrá un VideoPlayer aquí

public class TeleporterWithVideoSequence : MonoBehaviour // Nombre actualizado para reflejar secuencia
{
    [Header("Configuración de Teletransporte")]
    [Tooltip("El objeto Transform que marca la posición y rotación de destino (Punto B).")]
    public Transform destinationPoint;
    [Tooltip("(Opcional) El objeto específico a teletransportar. Si está vacío, se teletransportará este mismo objeto.")]
    public Transform objectToTeleport;

    [Header("Teletransporte Secundario")]
    [Tooltip("El objeto Player Controller que se teletransportará *hacia* el objeto principal después de que este último se mueva.")]
    public Transform playerController;

    [Header("Control de Vídeo Secuencial")]
    [Tooltip("El componente VideoPlayer que reproducirá la secuencia de clips.")]
    public VideoPlayer videoPlayer; // El ÚNICO VideoPlayer que usaremos
    [Tooltip("La secuencia de vídeos a reproducir uno tras otro en el destino.")]
    public List<VideoClip> videoSequence; // <-- ¡NUEVO! Lista de clips

    [Header("Control por Script (Opcional)")]
    [Tooltip("Poner a 'true' desde otro script para iniciar la teletransportación y la secuencia de vídeo.")]
    public bool shouldTeleportAndPlay = false;

    // --- Variables Internas ---
    private Transform _targetTransform;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _playerOriginalPosition;
    private Quaternion _playerOriginalRotation;
    private bool _hasValidPlayerToTrack = false;
    private bool _hasValidVideoPlayer = false;
    private bool _hasSavedOriginalState = false;
    private bool _isAtDestination = false;
    private int _currentVideoIndex = -1; // <-- ¡NUEVO! Índice del vídeo actual (-1 = no reproduciendo secuencia)

    void Awake()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // Suscribirse al evento
            _hasValidVideoPlayer = true;
            Debug.Log($"{GetType().Name} en {gameObject.name}: Suscrito al evento 'loopPointReached' de {videoPlayer.name}.");

            if (videoPlayer.playOnAwake)
            {
                Debug.LogWarning($"{GetType().Name} en {gameObject.name}: El VideoPlayer '{videoPlayer.name}' tiene 'Play On Awake' activado. Desactívalo para controlarlo por script.", videoPlayer);
                videoPlayer.playOnAwake = false; // Forzar desactivación
            }
            if (videoPlayer.isLooping)
            {
                 Debug.LogWarning($"{GetType().Name} en {gameObject.name}: El VideoPlayer '{videoPlayer.name}' tiene 'Loop' activado. Se desactivará para que la secuencia pueda avanzar.", videoPlayer);
                 videoPlayer.isLooping = false; // Forzar desactivación del loop para que 'loopPointReached' funcione para la secuencia
            }
        }
        else
        {
            _hasValidVideoPlayer = false;
            Debug.LogError($"{GetType().Name} en {gameObject.name}: ¡No se ha asignado un VideoPlayer! La funcionalidad de vídeo no funcionará.", this);
        }
    }

    void Start()
    {
        if (destinationPoint == null)
        {
            Debug.LogError($"¡Error en {GetType().Name}! 'Destination Point' no asignado para: {gameObject.name}", this);
            enabled = false;
            return;
        }
        if (!_hasValidVideoPlayer) // Si no hay VideoPlayer, desactivar script
        {
             Debug.LogError($"{GetType().Name}: Desactivando script por falta de VideoPlayer.", this);
             enabled = false;
             return;
        }
         if (videoSequence == null || videoSequence.Count == 0)
         {
             Debug.LogWarning($"{GetType().Name}: La lista 'videoSequence' está vacía o no asignada. No se reproducirán vídeos.", this);
         }

        AssignAndValidateTargets();
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd; // Desuscribirse
        }
    }


    void AssignAndValidateTargets()
    {
        if (objectToTeleport == null) { _targetTransform = this.transform; }
        else { _targetTransform = objectToTeleport; }
        Debug.Log($"{GetType().Name}: Objeto principal a teletransportar: {_targetTransform.name}");

        if (playerController != null && playerController != _targetTransform) { _hasValidPlayerToTrack = true; }
        else { _hasValidPlayerToTrack = false; }

        _hasSavedOriginalState = false;
        _isAtDestination = false;
        shouldTeleportAndPlay = false;
        _currentVideoIndex = -1; // Resetear índice
    }

    void Update()
    {
        if (shouldTeleportAndPlay && !_isAtDestination)
        {
            TeleportPlayAndResetFlag();
        }
    }

    void TeleportPlayAndResetFlag()
    {
        TeleportToDestinationAndStartSequence();
        shouldTeleportAndPlay = false;
    }

    // Método para teletransportar Y EMPEZAR la secuencia de vídeo
    void TeleportToDestinationAndStartSequence()
    {
        if (_targetTransform == null || destinationPoint == null) return;

        // --- 1. Guardar Estado Original ---
        if (!_hasSavedOriginalState)
        {
            _originalPosition = _targetTransform.position;
            _originalRotation = _targetTransform.rotation;
            if (_hasValidPlayerToTrack && playerController != null) {
                _playerOriginalPosition = playerController.position;
                _playerOriginalRotation = playerController.rotation;
            }
            _hasSavedOriginalState = true;
        }

        // --- 2. Teletransportar Objetos ---
        _targetTransform.position = destinationPoint.position;
        _targetTransform.rotation = destinationPoint.rotation;
        if (_hasValidPlayerToTrack && playerController != null) {
            playerController.position = _targetTransform.position;
            // Opcional: playerController.rotation = _targetTransform.rotation;
        }
        Debug.Log($"{GetType().Name}: Objetos teletransportados al destino ({destinationPoint.name}).");


        // --- 3. Iniciar la SECUENCIA de Vídeo ---
        _currentVideoIndex = -1; // Resetear índice
        if (_hasValidVideoPlayer && videoSequence != null && videoSequence.Count > 0)
        {
            PlayVideoAtIndex(0); // Empezar con el primer vídeo (índice 0)
        }
        else
        {
             Debug.LogWarning($"{GetType().Name}: No hay vídeos en la secuencia para reproducir.", this);
             // Como no hay videos, ¿deberíamos marcar como si ya hubiéramos terminado?
             // Podríamos llamar a ReturnToOriginalPosition() aquí si ese es el comportamiento deseado
             // O simplemente quedarse en el destino sin video. Por ahora, nos quedamos.
        }

        _isAtDestination = true;
    }

    // --- MÉTODO LLAMADO AUTOMÁTICAMENTE POR EL EVENTO DEL VIDEO PLAYER ---
    void OnVideoEnd(VideoPlayer vp)
    {
        // Asegurarse de que el evento viene del VideoPlayer correcto, no está en loop, y estábamos reproduciendo una secuencia
        if (_hasValidVideoPlayer && vp == videoPlayer && !vp.isLooping && _currentVideoIndex >= 0)
        {
            Debug.Log($"{GetType().Name}: Vídeo '{vp.clip?.name ?? "N/A"}' (Índice {_currentVideoIndex}) ha terminado.");

            // Calcular el índice del SIGUIENTE vídeo
            int nextVideoIndex = _currentVideoIndex + 1;

            // Comprobar si hay un siguiente vídeo en la lista
            if (videoSequence != null && nextVideoIndex < videoSequence.Count)
            {
                // Sí hay más vídeos, reproducir el siguiente
                 Debug.Log($"{GetType().Name}: Pasando al siguiente vídeo (Índice {nextVideoIndex}).");
                PlayVideoAtIndex(nextVideoIndex);
            }
            else
            {
                // No hay más vídeos (o la lista es nula/inválida), la secuencia ha terminado.
                Debug.Log($"{GetType().Name}: Fin de la secuencia de vídeo. Retornando objetos...");
                ReturnToOriginalPosition(); // <-- Solo retornamos al final de la secuencia
            }
        }
        else if (_hasValidVideoPlayer && vp == videoPlayer && vp.isLooping)
        {
             Debug.LogWarning($"{GetType().Name}: El vídeo '{vp.clip?.name ?? "N/A"}' está en loop. La secuencia no avanzará. Desactive el loop en el VideoPlayer.", vp);
        }
    }

    // Método auxiliar para reproducir un vídeo de la secuencia por su índice
    void PlayVideoAtIndex(int index)
    {
        if (!_hasValidVideoPlayer || videoPlayer == null || videoSequence == null || index < 0 || index >= videoSequence.Count)
        {
            Debug.LogError($"{GetType().Name}: No se puede reproducir vídeo en índice {index}. Revise VideoPlayer y la lista 'videoSequence'.", this);
             // Si falla la reproducción, podríamos querer retornar también
             // ReturnToOriginalPosition();
            _currentVideoIndex = -1; // Indicar que la secuencia se interrumpió
            return;
        }

        VideoClip clipToPlay = videoSequence[index];

        if (clipToPlay != null)
        {
            _currentVideoIndex = index; // Actualizar el índice actual
            videoPlayer.clip = clipToPlay;
            videoPlayer.Stop(); // Asegura empezar desde el principio
            videoPlayer.Play();
            Debug.Log($"{GetType().Name}: Reproduciendo vídeo '{clipToPlay.name}' (Índice {_currentVideoIndex}, {_currentVideoIndex + 1}/{videoSequence.Count}).");
        }
        else
        {
             Debug.LogWarning($"{GetType().Name}: El clip en el índice {index} de 'videoSequence' es nulo. Saltando este vídeo.", this);
             // Intentar pasar al siguiente automáticamente simulando que este terminó
             OnVideoEnd(videoPlayer); // Llamada recursiva simulada - ¡Cuidado! Esto podría ser problemático si hay muchos nulos seguidos.
                                      // Una alternativa más segura sería simplemente detener la secuencia o tener lógica más robusta en OnVideoEnd.
                                      // Por ahora, intentamos seguir. Si el *último* es nulo, OnVideoEnd manejará el retorno.
        }
    }


    // --- MÉTODO PÚBLICO (y llamado por OnVideoEnd) PARA RETORNAR ---
    [ContextMenu("Devolver Objetos al Origen y Detener Vídeo")]
    public void ReturnToOriginalPosition()
    {
        if (_hasSavedOriginalState && _isAtDestination)
        {
            if (_targetTransform == null) { return; }

            // --- 1. Detener el vídeo ---
             if (_hasValidVideoPlayer && videoPlayer != null && videoPlayer.isPlaying)
             {
                 videoPlayer.Stop();
                 videoPlayer.clip = null; // Opcional: quitar el último clip asignado
                 Debug.Log($"{GetType().Name}: Vídeo detenido.");
             }

            // --- 2. Retornar los objetos ---
            _targetTransform.position = _originalPosition;
            _targetTransform.rotation = _originalRotation;
            if (_hasValidPlayerToTrack && playerController != null) {
                playerController.position = _playerOriginalPosition;
                playerController.rotation = _playerOriginalRotation;
            }
            Debug.Log($"{GetType().Name}: Objetos retornados a la posición original.");

            // --- 3. Actualizar estado ---
            _isAtDestination = false;
            _currentVideoIndex = -1; // <-- ¡IMPORTANTE! Resetear índice de secuencia
        }
        else if (!_isAtDestination) {
             // Ya estamos en origen o no se inició el proceso, no hacer nada o log
             // Debug.LogWarning($"{GetType().Name}: Ya se está en la posición original o no se inició el teletransporte.", this);
        }

        // Asegurar que el flag esté bajo al intentar retornar o si ya estamos en origen
        shouldTeleportAndPlay = false;
    }
}