
using UnityEngine;
using TMPro;
using PurrNet;
using System.Collections;
using System.Collections.Generic;

public class PlayerMessage : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private float popInDuration = 0.3f;
    [SerializeField] private float popOutDuration = 0.25f;

    [Header("Pop-Up Feel")]
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float floatUpDistance = 0.15f;

    [Header("Distance Scaling (Other Players)")]
    [SerializeField] private float shrinkStartDistance = 10f;
    [SerializeField] private float disappearDistance = 25f;

    [Tooltip("Extra buffer distance on the server.")]
    [SerializeField] private float networkSendBuffer = 5f;

    [Header("Settings")]
    [SerializeField] private int maxCharacterLength = 120;

    private Camera localCamera;
    private Canvas messageCanvas;
    private Coroutine animationCoroutine;

    private Vector3 originalScale;
    private Vector3 originalLocalPos;

    private Vector3 currentAnimScale;
    private float currentAnimAlpha;

    private float disappearDistanceSqr;
    private float shrinkStartDistanceSqr;
    private float maxNetworkDistanceSqr;

    private void Awake()
    {
        disappearDistanceSqr = disappearDistance * disappearDistance;
        shrinkStartDistanceSqr = shrinkStartDistance * shrinkStartDistance;

        float maxNetworkDistance = disappearDistance + networkSendBuffer;
        maxNetworkDistanceSqr = maxNetworkDistance * maxNetworkDistance;

        if (canvasGroup == null) return;

        messageCanvas = canvasGroup.GetComponentInParent<Canvas>();

        Transform messageTransform = canvasGroup.transform;

        originalScale = messageTransform.localScale;
        originalLocalPos = messageTransform.localPosition;

        // Initialize animation values
        currentAnimScale = originalScale;
        currentAnimAlpha = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
        {
            localCamera = Camera.main;
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

        if (message.Length > maxCharacterLength)
            message = message.Substring(0, maxCharacterLength);

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
        if (canvasGroup == null || messageText == null)
            return;

        if (!isOwner)
        {
            if (localCamera == null)
                localCamera = Camera.main;

            if (localCamera != null)
            {
                float sqrDistance =
                    (canvasGroup.transform.position -
                    localCamera.transform.position).sqrMagnitude;

                if (sqrDistance >= disappearDistanceSqr)
                {
                    HideMessageImmediate();
                    return;
                }
            }
        }

        messageText.text = message;

        canvasGroup.gameObject.SetActive(true);

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

        currentAnimScale = originalScale;
        currentAnimAlpha = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.transform.localScale = originalScale;
        canvasGroup.transform.localPosition = originalLocalPos;

        canvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator PopUpRoutine()
    {
        Transform messageTransform = canvasGroup.transform;

        Vector3 startPos =
            originalLocalPos - new Vector3(0f, floatUpDistance, 0f);

        Vector3 minScale = originalScale * startScale;

        // 1. Pop In
        float timer = 0f;

        while (timer < popInDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / popInDuration);

            float bounceT = EaseOutBack(t);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentAnimAlpha = smoothT;

            currentAnimScale = Vector3.LerpUnclamped(
                minScale,
                originalScale,
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
        currentAnimScale = originalScale;

        messageTransform.localPosition = originalLocalPos;

        // 2. Hold
        yield return new WaitForSeconds(messageDuration);

        // 3. Pop Out
        timer = 0f;

        while (timer < popOutDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / popOutDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentAnimAlpha = Mathf.Lerp(1f, 0f, smoothT);

            currentAnimScale = Vector3.Lerp(
                originalScale,
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
        if (canvasGroup == null || !canvasGroup.gameObject.activeSelf)
            return;

        if (localCamera == null)
            localCamera = Camera.main;

        if (localCamera == null)
            return;

        Transform messageTransform = canvasGroup.transform;

        Vector3 direction =
            messageTransform.position - localCamera.transform.position;

        float distanceFactor = 1f;

        if (!isOwner)
        {
            float sqrDistance = direction.sqrMagnitude;

            // Completely hide beyond maximum distance
            if (sqrDistance >= disappearDistanceSqr)
            {
                if (messageCanvas != null && messageCanvas.enabled)
                    messageCanvas.enabled = false;

                messageTransform.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;

                return;
            }

            // Re-enable canvas inside range
            if (messageCanvas != null && !messageCanvas.enabled)
                messageCanvas.enabled = true;

            // Distance scaling
            if (sqrDistance > shrinkStartDistanceSqr)
            {
                float distance = Mathf.Sqrt(sqrDistance);

                distanceFactor = 1f - Mathf.InverseLerp(
                    shrinkStartDistance,
                    disappearDistance,
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
        canvasGroup.alpha = currentAnimAlpha * distanceFactor;
    }
}
