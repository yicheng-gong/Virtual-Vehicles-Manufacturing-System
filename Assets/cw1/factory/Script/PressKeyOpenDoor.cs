using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PressKeyOpenDoor : MonoBehaviour
{
    public GameObject Instruction;
    public GameObject AnimeObject;
    public GameObject ThisTrigger;
    public bool Action = false;
    public XRNode controllerNode = XRNode.RightHand;

    void Start()
    {
        Instruction.SetActive(false);

    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.tag == "Player")
        {
            Instruction.SetActive(true);
            Action = true;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        Instruction.SetActive(false);
        Action = false;
    }


    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool bButtonPressed;
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bButtonPressed) && bButtonPressed)
        {
            if (Action == true)
            {
                Instruction.SetActive(false);
                AnimeObject.GetComponent<Animator>().Play("DoorOpen");
                ThisTrigger.SetActive(false);
                Action = false;
            }
        }

    }


}