using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void Use(string usersName)
    {
        Shoot(usersName);
        Debug.Log("Using gun " + itemInfo.itemName + " >> user - " + usersName);
    }

    void Shoot(string usersName)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f , 0.5f));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Shooting from >" + usersName + "<)");
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamageFromEnemy(((GunInfo)itemInfo).damage, usersName);
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if(colliders.Length != 0)
        {
            GameObject bulletImpactObject = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObject, 10f);
            bulletImpactObject.transform.SetParent(colliders[0].transform);
        }
    }
}
