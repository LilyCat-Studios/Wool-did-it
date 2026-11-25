using System;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;

/// <summary>
/// Networked sheep: private clue list (owner only), one publicly visible
/// carried clue, and synced customisation.
/// </summary>
public class SheepController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _carryPoint;

    [Header("Validation")]
    [SerializeField] private float _maxInteractDistance = 3.5f;

    // Owner writes directly (ownerAuth); value syncs to everyone.
    [SerializeField] private SyncVar<SheepData> _sheepData = new(SheepData.Default, ownerAuth: true);

    // Server-authoritative: the clue currently carried (null if none).
    [SerializeField] private SyncVar<Clue> _carried = new();

    // Only ever filled on the owning client (via TargetRpc).
    private readonly HashSet<int> _clues = new();
    private Clue _attached;

    public event Action<int> OnClueCollected;          // owner only
    public event Action<Clue> OnCarriedChanged;    // everyone
    public event Action<SheepData> OnSheepDataChanged; // everyone

    public IReadOnlyCollection<int> ClueIds => _clues;
    public Clue Carried => _carried.value;
    public SheepData Data => _sheepData.value;

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        // "-=" first: a host receives OnSpawned twice and would otherwise subscribe twice.
        _carried.onChanged -= HandleCarriedChanged;
        _sheepData.onChanged -= HandleDataChanged;
        _carried.onChanged += HandleCarriedChanged;
        _sheepData.onChanged += HandleDataChanged;

        // Late joiners: onChanged does not fire for values that already exist.
        HandleCarriedChanged(_carried.value);
        HandleDataChanged(_sheepData.value);
    }

    protected override void OnDespawned(bool asServer)
    {
        if (asServer)
            _carried.value = null;

        // Release the clue locally so it isn't destroyed with this sheep.
        if (_attached != null)
            _attached.SetHeld(null);
        _attached = null;

        _carried.onChanged -= HandleCarriedChanged;
        _sheepData.onChanged -= HandleDataChanged;

        base.OnDespawned(asServer);
    }

    #region Clues

    public bool HasClue(int clueId) => _clues.Contains(clueId);

    // Called the interaction script on the owner's client.
    public void RequestPickupClue(Clue clue)
    {
        if (isOwner && clue != null && clue.Data.isMovable)
            PickupClueServer(clue);
    }

    public void RequestDestroyClue(Clue clue)
    {
        if (isOwner && clue != null && clue.Data.canBeDestroyed)
            DestroyClueServer(clue);
    }

    // For clues that are neither movable nor destroyable.
    public void RequestCheckClue(Clue clue)
    {
        if (isOwner && clue != null)
            CheckClueServer(clue);
    }

    public void RequestDropClue()
    {
        if (isOwner && _carried.value != null)
            DropClueServer();
    }

    [ServerRpc(requireOwnership: true)]
    private void PickupClueServer(Clue clue)
    {
        if (!CanInteract(clue) || !clue.Data.isMovable || clue.IsHeld) return;

        // Assigning replaces the old clue; HandleCarriedChanged drops it first.
        _carried.value = clue;
        Collect(clue);
    }

    [ServerRpc(requireOwnership: true)]
    private void DestroyClueServer(Clue clue)
    {
        if (!CanInteract(clue) || !clue.Data.canBeDestroyed || clue.IsHeld) return;

        Collect(clue);
        Destroy(clue.gameObject); // on the server this despawns it for all clients
    }

    [ServerRpc(requireOwnership: true)]
    private void CheckClueServer(Clue clue)
    {
        if (CanInteract(clue))
            Collect(clue);
    }

    [ServerRpc(requireOwnership: true)]
    private void DropClueServer() => _carried.value = null;

    // Records the first finder and adds the clue to this player's list.
    private void Collect(Clue clue)
    {
        clue.Check(owner.Value);
        GiveClueTarget(owner.Value, clue.ClueId);
    }

    private bool CanInteract(Clue clue) =>
        clue != null && owner.HasValue && InRange(clue.transform.position);

    [TargetRpc]
    private void GiveClueTarget(PlayerID target, int clueId)
    {
        if (_clues.Add(clueId))
            OnClueCollected?.Invoke(clueId);
    }

    private void HandleCarriedChanged(Clue newClue)
    {
        if (_attached == newClue) return;

        if (_attached != null) _attached.SetHeld(null);   // drop the old one first
        if (newClue != null) newClue.SetHeld(_carryPoint);
        _attached = newClue;

        OnCarriedChanged?.Invoke(newClue);
    }

    #endregion

    #region Customisation (call on the owner)

    public void SetWoolColor(Color color) { SheepData d = _sheepData.value; d.woolColor = color; Apply(d); }
    public void SetHat(int id)            { SheepData d = _sheepData.value; d.hatID = id;         Apply(d); }
    public void SetWoolShape(int id)      { SheepData d = _sheepData.value; d.woolShapeID = id;   Apply(d); }
    public void SetFootprint(int id)      { SheepData d = _sheepData.value; d.footprintID = id;   Apply(d); }

    private void Apply(SheepData data)
    {
        if (isOwner)
            _sheepData.value = data; // reassigning the whole struct triggers the sync
    }

    private void HandleDataChanged(SheepData data) => OnSheepDataChanged?.Invoke(data);

    #endregion

    private bool InRange(Vector3 pos) =>
        (transform.position - pos).sqrMagnitude <= _maxInteractDistance * _maxInteractDistance;
}