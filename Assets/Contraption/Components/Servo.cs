using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Extensions;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.XR;
using UnityEngine;

public class Servo : MonoBehaviour, IGraspable, IComponent, IVariable
{
    public float Angle;
    /// <summary>
    /// // This property fulfils INetworkSpawnable. Spawnable objects need to 
    /// have their Ids set by the Object Spawner before they are registered, so
    /// all spawned objects can communicate with eachother.
    /// </summary>
    public NetworkId NetworkId { get; set; }
    public NetworkId Last_Operator { get; set; }
    public float Value
    {
        get => Angle;
        set => Angle = value;
    }


    private FollowHelper follow;
    private NetworkContext context;
    private ContraptionManager manager;
    private HingeJoint joint;

    public void Grasp(Hand controller)
    {
        follow.Grasp(controller);
    }

    public void Release(Hand controller)
    {
        follow.Release(controller);
        Attach();
    }

    private void Awake()
    {
        follow = new FollowHelper(transform);
        joint = GetComponent<HingeJoint>();
        joint.useLimits = true;
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        manager = context.Scene.GetClosestComponent<ContraptionManager>();
    }

    void Update()
    {
        if (follow.Update())
        {
            SendUpdate();
        }
        var motor = joint.motor;
        var limits = joint.limits;
        var axis = joint.axis;
        if (Angle <= 180)
        {
            limits.max = Angle;
            limits.min = 0;
            motor.targetVelocity = 50f;
        }
        else
        {
            limits.max = 0;
            limits.min = Angle - 360f;
            motor.targetVelocity = -50f;
        }
        joint.motor = motor;
        joint.limits = limits;

    }

    public void Attach()
    {
        Attach(manager.GetTouchingRigidBodies(GetComponent<Collider>()).
            Where(c => c != GetComponent<Rigidbody>()).
            FirstOrDefault());
        SendUpdate();
    }

    private void Attach(Rigidbody parent)
    {
        if (parent != null)
        {
            joint.connectedBody = parent.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = true;
            transform.parent = parent.transform;
        }
        else
        {
            joint.connectedBody = null;
            transform.parent = null;
        }
    }

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public NetworkId attachedId;
        public NetworkId Last_Operator;
    }

    public void SendUpdate()
    {
        context.SendJson(new Message()
        {
            position = manager.GetLocalPosition(transform),
            rotation = manager.GetLocalRotation(transform),
            attachedId = manager.GetNetworkId(transform.parent),
            Last_Operator = Last_Operator
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();
        transform.position = manager.GetWorldPosition(message.position);
        transform.rotation = manager.GetWorldRotation(message.rotation);
        Attach(manager.GetComponentRigidBody(message.attachedId));
        Last_Operator = message.Last_Operator;
    }


    public void OnTriggerStay(Collider other)
    {
        if (other.name != "Manipulator")
        {
            follow.attchment_transform = other.transform;
            follow.contacting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        follow.contacting = false;
    }


}
