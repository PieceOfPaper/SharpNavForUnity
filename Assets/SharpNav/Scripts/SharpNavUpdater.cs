using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;

public class SharpNavUpdater : MonoBehaviour
{
    public int groupID = 1;
    public UpdateType type = UpdateType.Update;

    public enum UpdateType
    {
        Update,
        FixedUpdate,
    }


    private void Update()
    {
        if (SharpNavManager.Instance == null) return;
        if (type == UpdateType.Update)
        {
            var navMesh = SharpNavManager.Instance.GetNavMeshByGroupID(groupID);
            if (navMesh == null)
                return;

            navMesh.Update(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (SharpNavManager.Instance == null) return;
        if (type == UpdateType.FixedUpdate)
        {
            var navMesh = SharpNavManager.Instance.GetNavMeshByGroupID(groupID);
            if (navMesh == null)
                return;

            navMesh.Update(Time.fixedDeltaTime);
        }
    }
}
