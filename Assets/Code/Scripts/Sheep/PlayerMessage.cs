
using UnityEngine;
using TMPro;
using PurrNet;
using System.Collections;
using System.Collections.Generic;

public class PlayerMessage : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] internal CanvasGroup _canvasGroup;
    [SerializeField] internal TMP_Text _messageText;

    [Header("Timing")]
    [SerializeField] internal float _messageDuration = 3f;
    [SerializeField] internal float _popInDuration = 0.3f;
    [SerializeField] internal float _popOutDuration = 0.25f;

    [Header("Pop-Up Feel")]
    [SerializeField] private float _startScale = 0.5f;
    [SerializeField] private float _floatUpDistance = 0.15f;

    [Header("Distance Scaling (Other Players)")]
    [SerializeField] internal float _shrinkStartDistance = 10f;
    [SerializeField] internal float _disappearDistance = 25f;

    [Tooltip("Extra buffer distance on the server.")]
    [SerializeField] private float _networkSendBuffer = 5f;

    [Header("Settings")]
    [SerializeField] internal int _maxCharacterLength = 120;

    internal Camera _localCamera;
    private Canvas _messageCanvas;
    private Coroutine _animationCoroutine;

    private Vector3 _originalScale;
    private Vector3 _originalLocalPos;

    private Vector3 _currentAnimScale;
    private float _currentAnimAlpha;

    private float _disappearDistanceSqr;
    private float _shrinkStartDistanceSqr;
    private float _maxNetworkDistanceSqr;

    private void Awake()
    {
        _disappearDistanceSqr = _disappearDistance * _disappearDistance;
        _shrinkStartDistanceSqr = _shrinkStartDistance * _shrinkStartDistance;

        float maxNetworkDistance = _disappearDistance + _networkSendBuffer;
        maxNetworkDistanceSqr = maxNetworkDistance * maxNetworkDistance;

        if (_canvasGroup == null) return;

        messageCanvas = _canvasGroup.GetComponentInParent<Canvas>();

        Transform messageTransform = _canvasGroup.transform;

        _originalScale = messageTransform.localScale;
        originalLocalPos = messageTransform.localPosition;

        // Force TMP & Canvas to generate meshes/shaders now while hidden
        _canvasGroup.gameObject.SetActive(true);
        if (_messageText != null)
        {
            _messageText.text = " ";
            _messageText.ForceMeshUpdate();
            _messageText.text = string.Empty;
        }

        HideMessageImmediate();
    }

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
        {
            _localCamera = Camera.main;
        }
    }

    public void ShowMessage(string message)
    {
        if (!isOwner || string.IsNullOrWhiteSpace(message))
            return;

        SendMessageServerRpc(message);
    }


    [ServerRpc(requireOwnership: true)]
    private void SendMessageServerRpc(string message, RPCInfo info = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        message = message.Trim();

        if (message.Length > _maxCharacterLength)
            message = message.Substring(0, _maxCharacterLength);

        Vector3 senderPos = transform.position;

        foreach (PlayerID player in networkManager.players)
        {
            // Always send to the sender
            if (player == info.sender)
            {
                ShowMessageTargetRpc(player, message);
                continue;
            }

            // Enumerate all network objects owned by this player
            foreach (NetworkIdentity netObj in
                     networkManager.EnumerateAllPlayerOwnedIds(player, true))
            {
                if (netObj == null)
                    continue;

                float sqrDistance =
                    (netObj.transform.position - senderPos).sqrMagnitude;

                if (sqrDistance <= maxNetworkDistanceSqr)
                {
                    ShowMessageTargetRpc(player, message);
                    break;
                }
            }
        }
    }


    [TargetRpc]
    private void ShowMessageTargetRpc(PlayerID target, string message)
    {
        if (_canvasGroup == null || _messageText == null)
            return;

        if (!isOwner)
        {
            if (_localCamera == null)
                _localCamera = Camera.main;

            if (_localCamera != null)
            {
                float sqrDistance =
                    (_canvasGroup.transform.position -
                    _localCamera.transform.position).sqrMagnitude;

                if (sqrDistance >= _disappearDistanceSqr)
                {
                    HideMessageImmediate();
                    return;
                }
            }
        }

        _messageText.text = message;

        if (messageCanvas != null) messageCanvas.enabled = true;

        if (messageCanvas != null && !messageCanvas.enabled)
            messageCanvas.enabled = true;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PopUpRoutine());
    }

    private void HideMessageImmediate()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        currentAnimScale = _originalScale;
        currentAnimAlpha = 0f;

        _canvasGroup.alpha = 0f;
        _canvasGroup.transform.localScale = _originalScale;
        _canvasGroup.transform.localPosition = originalLocalPos;

        if (messageCanvas != null) messageCanvas.enabled = false;
    }

    private IEnumerator PopUpRoutine()
    {
        Transform messageTransform = _canvasGroup.transform;

        Vector3 startPos =
            originalLocalPos - new Vector3(0f, _floatUpDistance, 0f);

        Vector3 minScale = _originalScale * _startScale;

        // Set initial visual state immediately
        currentAnimAlpha = 0f;
        currentAnimScale = minScale;
        messageTransform.localPosition = startPos;

        // Wait one frame so TMP mesh generation doesn't eat into _popInDuration
        yield return null;

        // 1. Pop In
        float timer = 0f;
        while (timer < _popInDuration)
        {
            timer += Mathf.Min(Time.deltaTime, 0.05f);

            float t = Mathf.Clamp01(timer / _popInDuration);

            float bounceT = EaseOutBack(t);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentAnimAlpha = smoothT;

            currentAnimScale = Vector3.LerpUnclamped(
                minScale,
                _originalScale,
                bounceT
            );

            messageTransform.localPosition = Vector3.Lerp(
                startPos,
                originalLocalPos,
                smoothT
            );

            yield return null;
        }

        currentAnimAlpha = 1f;
        currentAnimScale = _originalScale;

        messageTransform.localPosition = originalLocalPos;

        // 2. Hold
        yield return new WaitForSeconds(_messageDuration);

        // 3. Pop Out
        timer = 0f;

        while (timer < _popOutDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / _popOutDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentAnimAlpha = Mathf.Lerp(1f, 0f, smoothT);

            currentAnimScale = Vector3.Lerp(
                _originalScale,
                minScale,
                smoothT
            );

            yield return null;
        }

        HideMessageImmediate();
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f +
               c3 * Mathf.Pow(t - 1f, 3f) +
               c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void LateUpdate()
    {
        if (_canvasGroup == null || (messageCanvas != null && !messageCanvas.enabled)) return;

        if (_localCamera == null)
            _localCamera = Camera.main;

        if (_localCamera == null) return;

        Transform messageTransform = _canvasGroup.transform;

        Vector3 direction =
            messageTransform.position - _localCamera.transform.position;

        float distanceFactor = 1f;

        if (!isOwner)
        {
            float sqrDistance = direction.sqrMagnitude;

            // Completely hide beyond maximum distance
            if (sqrDistance >= _disappearDistanceSqr)
            {
                if (messageCanvas != null && messageCanvas.enabled)
                    messageCanvas.enabled = false;

                messageTransform.localScale = Vector3.zero;
                _canvasGroup.alpha = 0f;

                return;
            }

            // Re-enable canvas inside range
            if (messageCanvas != null && !messageCanvas.enabled)
                messageCanvas.enabled = true;

            // Distance scaling
            if (sqrDistance > _shrinkStartDistanceSqr)
            {
                float distance = Mathf.Sqrt(sqrDistance);

                distanceFactor = 1f - Mathf.InverseLerp(
                    _shrinkStartDistance,
                    _disappearDistance,
                    distance
                );
            }
        }

        // Billboard
        if (direction.sqrMagnitude > 0.0001f)
        {
            messageTransform.rotation = Quaternion.LookRotation(direction);
        }

        // Apply animation and distance scaling
        messageTransform.localScale = currentAnimScale * distanceFactor;
        _canvasGroup.alpha = currentAnimAlpha * distanceFactor;
    }
}
