using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SmoothMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    // Variables <<
    [SerializeField]
    Vector3 lastPos, velocity;

    [SerializeField]
    Quaternion lastRot;

    Quaternion rotationLastPacket = Quaternion.identity;

    float currentTime;
    double currentPacket, lastPacket;
    bool valuesReceived;
    private Vector3 positionLastPacket = Vector3.zero;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        TryNetworkMovement();
    }


    void TryNetworkMovement()
    {
        if (!photonView.IsMine && valuesReceived)
        {
            Debug.Log("NetworkSmooth");
            double num = currentPacket - lastPacket;
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(positionLastPacket, lastPos, (float)(currentTime / num));
            transform.rotation = Quaternion.Lerp(rotationLastPacket, lastRot, (float)(currentTime / num));
            rb.velocity = velocity;
            //rb.angularVelocity = angularVelocity;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Stream writing");
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
            //stream.SendNext(rb.angularVelocity);
            return;
        }
        lastPos = (Vector3)stream.ReceiveNext();
        lastRot = (Quaternion)stream.ReceiveNext();
        velocity = (Vector3)stream.ReceiveNext();
        //angularVelocity = (Vector2)stream.ReceiveNext();
        valuesReceived = true;
        currentTime = 0f;
        lastPacket = currentPacket;
        currentPacket = info.SentServerTime;
        positionLastPacket = transform.position;
        rotationLastPacket = transform.rotation;
    }
}
