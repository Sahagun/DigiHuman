using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum HandPoints : int
{
    Wrist,
    ThumbFirst, //first thumb bone (nearest to the wrist)
    ThumbSecond, //second
    ThumbThird, //third
    ThumbFourth, //fourth
    IndexFingerFirst,
    IndexFingerSecond, 
    IndexFingerThird, 
    IndexFingerFourth, 
    MiddleFingerFirst,
    MiddleFingerSecond, 
    MiddleFingerThird, 
    MiddleFingerFourth, 
    RingFingerFirst,
    RingFingerSecond, 
    RingFingerThird, 
    RingFingerFourth, 
    PinkyFirst, //little finger = pinky
    PinkySecond, 
    PinkyThird, 
    PinkyFourth, 
}

public class HandsPreprocessor : CharacterMapper
{
    private JointPoint[] leftHand;
    private JointPoint[] rightHand;

    protected override void InitializationHumanoidPose()
    {
        InitializeRightHand();
        InitializeLeftHand();
        SetupInverseAndDistance(rightHand);
        SetupInverseAndDistance(leftHand);
    }

    public override void Predict3DPose(PoseJsonVector poseJsonVector)
    {
        throw new NotImplementedException();
    }

    public void Predict3DPose(HandJsonVector poseJsonVector)
    {
        
        //right hand
        BodyPartVector[] handR = poseJsonVector.handsR;
        for (int i = 0; i < handR.Length; i++)
        {
            jointsDebug[i].transform.position = handR[i].position;
        }
        for (int i = 0; i < handR.Length; i++)
        {
            rightHand[i].LandmarkPose = handR[i].position;
        }
        
        //setting bone positions
        for (int i = 0; i < rightHand.Length; i++)
        {
            JointPoint bone = rightHand[i];
            if (bone.Child != null)
            {
                JointPoint child = bone.Child;
                float distance = bone.DistanceFromChild;
                Vector3 direction = (-bone.LandmarkPose + child.LandmarkPose) / (-bone.LandmarkPose + child.LandmarkPose).magnitude;
                Debug.Log(direction);
                child.Transform.position = bone.Transform.position + direction * distance;
                Debug.Log(distance + "  " + Vector3.Distance(child.Transform.position,bone.Transform.position));
            }

        }
        
        Vector3 forward = -rightHand[(int) HandPoints.Wrist].Transform.forward;

        //rotation
        
        for (int i = 0; i < rightHand.Length; i++)
        {
            JointPoint bone = rightHand[i];
            if (bone.Parent != null)
            {
                Vector3 fv = bone.Parent.Transform.position - bone.Transform.position;
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, fv) * bone.InverseRotation;
            }
            
            else if (bone.Child != null)
            {
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, forward) * bone.InverseRotation;
            }
            /*
            if (bone.Parent != null)
            {
                Vector3 fv = bone.Parent.Transform.position - bone.Transform.position;
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, fv) * bone.InverseRotation;
            }
            
            else if (bone.Child != null)
            {
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, forward) * bone.InverseRotation;
            }
            */
        }
        
        
    }

    private void SetupInverseAndDistance(JointPoint[] jointPoints)
    {
        for (int i = 0; i < jointPoints.Length; i++)
        {
            if (jointPoints[i].Child != null)
            {
                jointPoints[i].DistanceFromChild = Vector3.Distance(jointPoints[i].Child.Transform.position,
                    jointPoints[i].Transform.position);
            }
        }
        // Set Inverse
        Vector3 a = jointPoints[(int) HandPoints.PinkyFirst].Transform.position;
        Vector3 b = jointPoints[(int) HandPoints.Wrist].Transform.position;
        Vector3 c = jointPoints[(int) HandPoints.ThumbFirst].Transform.position;
        var forward = b.TriangleNormal(a,c);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, forward));
                jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
            }
        }
    }
    private void InitializeRightHand()
    {
        // Right Hand
        rightHand = new JointPoint[21];
        for (var i = 0; i < rightHand.Length; i++) rightHand[i] = new JointPoint();
        
        //Wrist
        rightHand[(int) HandPoints.Wrist].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);

        //thumb
        rightHand[(int) HandPoints.ThumbFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        rightHand[(int) HandPoints.ThumbSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rightHand[(int) HandPoints.ThumbThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        //child and parent
        rightHand[(int) HandPoints.ThumbFirst].Child = rightHand[(int) HandPoints.ThumbSecond];
        rightHand[(int) HandPoints.ThumbSecond].Child = rightHand[(int) HandPoints.ThumbThird];
        rightHand[(int) HandPoints.ThumbSecond].Parent = rightHand[(int) HandPoints.ThumbFirst];
        
        //index
        rightHand[(int) HandPoints.IndexFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        rightHand[(int) HandPoints.IndexFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        rightHand[(int) HandPoints.IndexFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        //child and parent
        rightHand[(int) HandPoints.IndexFingerFirst].Child = rightHand[(int) HandPoints.IndexFingerSecond];
        rightHand[(int) HandPoints.IndexFingerSecond].Child = rightHand[(int) HandPoints.IndexFingerThird];
        rightHand[(int) HandPoints.IndexFingerSecond].Parent = rightHand[(int) HandPoints.IndexFingerFirst];
        //rightHand[(int) HandPoints.IndexFingerSecond].Parent = rightHand[(int) HandPoints.IndexFingerFirst];
        
        //middle
        rightHand[(int) HandPoints.MiddleFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        rightHand[(int) HandPoints.MiddleFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        rightHand[(int) HandPoints.MiddleFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        //child and parent
        rightHand[(int) HandPoints.MiddleFingerFirst].Child = rightHand[(int) HandPoints.MiddleFingerSecond];
        rightHand[(int) HandPoints.MiddleFingerSecond].Child = rightHand[(int) HandPoints.MiddleFingerThird];
        rightHand[(int) HandPoints.MiddleFingerSecond].Parent = rightHand[(int) HandPoints.MiddleFingerFirst];
        
        //ring
        rightHand[(int) HandPoints.RingFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
        rightHand[(int) HandPoints.RingFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        rightHand[(int) HandPoints.RingFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingDistal);
        //child and parent
        rightHand[(int) HandPoints.RingFingerFirst].Child = rightHand[(int) HandPoints.RingFingerSecond];
        rightHand[(int) HandPoints.RingFingerSecond].Child = rightHand[(int) HandPoints.RingFingerThird];
        rightHand[(int) HandPoints.RingFingerSecond].Parent = rightHand[(int) HandPoints.RingFingerFirst];
        
        //pinky
        rightHand[(int) HandPoints.PinkyFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        rightHand[(int) HandPoints.PinkySecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        rightHand[(int) HandPoints.PinkyThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        //child and parent
        rightHand[(int) HandPoints.PinkyFirst].Child = rightHand[(int) HandPoints.PinkySecond];
        rightHand[(int) HandPoints.PinkySecond].Child = rightHand[(int) HandPoints.PinkyThird];
        rightHand[(int) HandPoints.PinkySecond].Parent = rightHand[(int) HandPoints.PinkyFirst];

    }

    private void InitializeLeftHand()
    {
        // Right Hand
        leftHand = new JointPoint[21];
        for (var i = 0; i < leftHand.Length; i++) leftHand[i] = new JointPoint();
        
        //Wrist
        leftHand[(int) HandPoints.Wrist].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);

        //thumb
        leftHand[(int) HandPoints.ThumbFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        leftHand[(int) HandPoints.ThumbSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        leftHand[(int) HandPoints.ThumbThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        //child and parent
        leftHand[(int) HandPoints.ThumbFirst].Child = leftHand[(int) HandPoints.ThumbSecond];
        leftHand[(int) HandPoints.ThumbSecond].Child = leftHand[(int) HandPoints.ThumbThird];
        leftHand[(int) HandPoints.ThumbSecond].Parent = leftHand[(int) HandPoints.ThumbFirst];
        
        //index
        leftHand[(int) HandPoints.IndexFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        leftHand[(int) HandPoints.IndexFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        leftHand[(int) HandPoints.IndexFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        //child and parent
        leftHand[(int) HandPoints.IndexFingerFirst].Child = leftHand[(int) HandPoints.IndexFingerSecond];
        leftHand[(int) HandPoints.IndexFingerSecond].Child = leftHand[(int) HandPoints.IndexFingerThird];
        leftHand[(int) HandPoints.IndexFingerSecond].Parent = leftHand[(int) HandPoints.IndexFingerFirst];
        
        //middle
        leftHand[(int) HandPoints.MiddleFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        leftHand[(int) HandPoints.MiddleFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        leftHand[(int) HandPoints.MiddleFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        //child and parent
        leftHand[(int) HandPoints.MiddleFingerFirst].Child = leftHand[(int) HandPoints.MiddleFingerSecond];
        leftHand[(int) HandPoints.MiddleFingerSecond].Child = leftHand[(int) HandPoints.MiddleFingerThird];
        leftHand[(int) HandPoints.MiddleFingerSecond].Parent = leftHand[(int) HandPoints.MiddleFingerFirst];
        
        //ring
        leftHand[(int) HandPoints.RingFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        leftHand[(int) HandPoints.RingFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        leftHand[(int) HandPoints.RingFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        //child and parent
        leftHand[(int) HandPoints.RingFingerFirst].Child = leftHand[(int) HandPoints.RingFingerSecond];
        leftHand[(int) HandPoints.RingFingerSecond].Child = leftHand[(int) HandPoints.RingFingerThird];
        leftHand[(int) HandPoints.RingFingerSecond].Parent = leftHand[(int) HandPoints.RingFingerFirst];
        
        //pinky
        leftHand[(int) HandPoints.PinkyFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        leftHand[(int) HandPoints.PinkySecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        leftHand[(int) HandPoints.PinkyThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        //child and parent
        leftHand[(int) HandPoints.PinkyFirst].Child = leftHand[(int) HandPoints.PinkySecond];
        leftHand[(int) HandPoints.PinkySecond].Child = leftHand[(int) HandPoints.PinkyThird];
        leftHand[(int) HandPoints.PinkySecond].Parent = leftHand[(int) HandPoints.PinkyFirst];
    }
}
