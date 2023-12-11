// Copyright 2023 Niantic, Inc. All Rights Reserved.

using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAvatar: NetworkTransform
{
  [HideInInspector]
  private Transform _arCameraTransform;

  protected override bool OnIsServerAuthoritative()
  {
    return false;
  }

  public override void OnNetworkSpawn()
  {
    if (IsOwner)
    {
      if (Camera.main)
      {
        _arCameraTransform = Camera.main.transform;
      }
    }

    base.OnNetworkSpawn();
  }

  new void Update()
  {
    if (IsOwner)
    {
      if (_arCameraTransform)
      {
        // Get local AR camera transform
        _arCameraTransform.GetPositionAndRotation(out var pos, out var rot);
        // Since using the ClientNetworkTransform, just update world transform of the cube matching with the
        // AR Camera's worldTransform. it's local transform will be synced.
        transform.SetPositionAndRotation(pos, rot);
      }
    }

    base.Update();
  }
}