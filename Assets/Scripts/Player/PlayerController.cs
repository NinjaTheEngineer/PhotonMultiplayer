using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    #region Variables

    [SerializeField]
    GameObject cameraHolder;

    [SerializeField]
    float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;
    
    [SerializeField]
    Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;
    [SerializeField]
    bool isGrounded;
    
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;

    #endregion

    #region Main Functions

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }
    private void Start()
    {
        if (PV.IsMine)
        {
            EquipItem(0);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }
    private void Update()
    {
        if (!PV.IsMine)
            return;

        Look();
        Move();
        Jump();
        WeaponChange();

        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use(PhotonNetwork.NickName);
        }

        if(transform.position.y < -10f)
        {
            Die();
        }
    }
    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    #endregion

    #region Player Controllers
    private void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }
    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
    private void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
    private void WeaponChange()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }

        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    #endregion

    #region Other Functions
    public void SetGroundedState(bool _grounded)
    {
        isGrounded = _grounded;
    }

    public void TakeDamageFromEnemy(float damage, string killerName) //Runs on shooter's computer
    {
        Debug.Log("TakeDamageFromEnemy - PlayerController - " + PhotonNetwork.NickName);
        PV.RPC("RPC_TakeDamageFromEnemy", RpcTarget.All, damage, killerName);
        Debug.Log("Shooters name: " + killerName);
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage - PlayerController - " + PhotonNetwork.NickName);
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamageFromEnemy(float damage, string killerName) //Runs on everyone's computer
    {
        if (!PV.IsMine) //This check makes it only run on the victim's computer
            return;

        currentHealth -= damage;
        if(currentHealth <= 0)
        {
            Die(PhotonNetwork.NickName, killerName);
        }
        Debug.Log("Took damage: " + damage);
    }

    private void Die(string userName, string killerName)
    {
        playerManager.Die(userName, killerName);
    }

    private void Die()
    {
        playerManager.Die();
    }

    #endregion

}
