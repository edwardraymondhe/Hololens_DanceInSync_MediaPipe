using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary> Joints Name
/// rShldrBend 0, rForearmBend 1, rHand 2, rThumb2 3, rMid1 4,
/// lShldrBend 5, lForearmBend 6, lHand 7, lThumb2 8, lMid1 9,
/// lEar 10, lEye 11, rEar 12, rEye 13, Nose 14,
/// rThighBend 15, rShin 16, rFoot 17, rToe 18,
/// lThighBend 19, lShin 20, lFoot 21, lToe 22,    
/// abdomenUpper 23,
/// hip 24, head 25, neck 26, spine 27
/// </summary>

public class HumanoidController : MonoBehaviour
{
    public Animator animator;
    public MediaPipeServer server;
    public HumanoidController humanoidController;
    public PoseFrame poseFrame;

    public int updateMode = -1;
    
    // Keypoint coordinates and related infos in threeDpose
    int[] parent = new int[] { 26, 0, 1, 2, 2, 26, 5, 6, 7, 7, 25, 10, 11, 12, 13, 24, 15, 16, 17, 24, 19, 20, 21, 24, -1, 26, 27, 23 }; // 28 Keypoints
    public List<List<Vector3>> pose3D = new List<List<Vector3>>();
    private Vector3 initPos;
    
    // Humanoid movement
    public Transform root, spine, neck, head, leye, reye, lshoulder, lelbow, lhand, lthumb2, lmid1, rshoulder, relbow, rhand, rthumb2, rmid1, lhip, lknee, lfoot, ltoe, rhip, rknee, rfoot, rtoe;
    private Quaternion midRoot, midSpine, midNeck, midHead, midLshoulder, midLelbow, midLhand, midRshoulder, midRelbow, midRhand, midLhip, midLknee, midLfoot, midRhip, midRknee, midRfoot;
    public Transform indicator;
    public Transform nose;

    // Coordinate setting
    public bool moveRoot = false;
    public float x = 1.98f;
    public float y = -1.02f;
    public float z = 0.25f;


    public float xLerp = 5f;
    public float yLerp = 1f;
    public float zLerp = 5f;

    public float lerpFactor = 10f;
    public bool isLerp = true;

    void Start()
    {
        /////////////////////////////////////////////////// Bone Definition ///////////////////////////////////////////////////
        
        // 0, 7, 9, 10, 11, 12, 13, 14, 15, 16, 1, 2, 3, 4, 5, 6

        // Body
        root = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        head = animator.GetBoneTransform(HumanBodyBones.Head);

        leye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        reye = animator.GetBoneTransform(HumanBodyBones.RightEye);
        
        // Left arm
        lshoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        lelbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        lhand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

        lthumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        lmid1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        
        // Right arm
        rshoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        relbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rhand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        rthumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rmid1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        
        // Left leg
        lhip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        lknee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        lfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

        ltoe = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        
        // Right leg
        rhip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        rknee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        rfoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        rtoe = animator.GetBoneTransform(HumanBodyBones.RightToes);

        initPos = root.position;
        /////////////////////////////////////////////////// Bone Sandwich Transform matrix ///////////////////////////////////////////////////
        
        // Current Rotation = lookforward * Transform matrix
        
        // For initial pose, current rotation is the initiali rotation, calculate the sandwich transform matrix for each points according to bone direction and human direction
        Vector3 forward = TriangleNormal(root.position, lhip.position, rhip.position);

        // midLshoulder, midLelbow, midLhand, midRscapular, midRshoulder, midRelbow, midRhand, midLhip, midLknee, midLfoot, midLtoe, midRhip, midRknee, midRfoot, midRtoe        
        
        // Root
        midRoot = Quaternion.Inverse(root.rotation) * Quaternion.LookRotation(forward);
        
        // Body
        midSpine = Quaternion.Inverse(spine.rotation) * Quaternion.LookRotation(spine.position - neck.position, forward);
        midNeck = Quaternion.Inverse(neck.rotation) * Quaternion.LookRotation(neck.position - head.position, forward);
        
        // Head
        midHead = Quaternion.Inverse(head.rotation) * Quaternion.LookRotation(nose.position - head.position);
        
        // Left arm
        midLshoulder = Quaternion.Inverse(lshoulder.rotation) * Quaternion.LookRotation(lshoulder.position - lelbow.position, forward);
        midLelbow = Quaternion.Inverse(lelbow.rotation) * Quaternion.LookRotation(lelbow.position - lhand.position, forward);
        midLhand = Quaternion.Inverse(lhand.rotation) * Quaternion.LookRotation(
            lthumb2.position - lmid1.position,
            TriangleNormal(lhand.position, lthumb2.position, lmid1.position)
            );
        
        // Right arm
        midRshoulder = Quaternion.Inverse(rshoulder.rotation) * Quaternion.LookRotation(rshoulder.position - relbow.position, forward);
        midRelbow = Quaternion.Inverse(relbow.rotation) * Quaternion.LookRotation(relbow.position - rhand.position, forward);
        midRhand = Quaternion.Inverse(rhand.rotation) * Quaternion.LookRotation(
            rthumb2.position - rmid1.position,
            TriangleNormal(rhand.position, rthumb2.position, rmid1.position)
            );
        
        // Left leg
        midLhip = Quaternion.Inverse(lhip.rotation) * Quaternion.LookRotation(lhip.position - lknee.position, forward);
        midLknee = Quaternion.Inverse(lknee.rotation) * Quaternion.LookRotation(lknee.position - lfoot.position, forward);
        midLfoot = Quaternion.Inverse(lfoot.rotation) * Quaternion.LookRotation(lfoot.position - ltoe.position, lknee.position - lfoot.position);
        // Right leg
        midRhip = Quaternion.Inverse(rhip.rotation) * Quaternion.LookRotation(rhip.position - rknee.position, forward);
        midRknee = Quaternion.Inverse(rknee.rotation) * Quaternion.LookRotation(rknee.position - rfoot.position, forward);
        midRfoot = Quaternion.Inverse(rfoot.rotation) * Quaternion.LookRotation(rfoot.position - rtoe.position, rknee.position - rfoot.position);
    }
    
    public List<Quaternion> UpdateByRealTime(List<Vector3> pred3D)
    {
        #region From ONNX -> MediaPipe
        /*
            * rShldrBend 0 - 12
		    * rForearmBend 1 - 14
		    * rHand 2 - 16
		    * rThumb2 3 - 22
		    * rMid1 4 - 18, 20
		    * 
            * lShldrBend 5 - 11
		    * lForearmBend 6 - 13
		    * lHand 7 - 15
		    * lThumb2 8 - 21
		    * lMid1 9 - 19, 17
		    * 
            * lEar 10 - 7
		    * lEye 11 - 2
		    * rEar 12 - 8
		    * rEye 13 - 5
		    * Nose 14 - 0
		    * 
            * rThighBend 15 - 24
		    * rShin 16 - 26
		    * rFoot 17 - 28
		    * rToe 18 - 32, 30
		    * 
            * lThighBend 19 - 23
		    * lShin 20 - 25
		    * lFoot 21 - 27
		    * lToe 22 - 29, 31
		    *    
            * abdomenUpper 23 - 肚脐
		    * 
            * hip 24 - 23, 24
		    * head 25 - 7, 8
		    * neck 26 - 11, 12
		    * spine 27 - 11, 12, 23, 24
        */
        #endregion
        
        //////////////////////  Update Positions //////////////////////
        float tallShin = (Vector3.Distance(pred3D[26], pred3D[28]) + Vector3.Distance(pred3D[25], pred3D[27])) / 2.0f;
        float tallThigh = (Vector3.Distance(pred3D[24], pred3D[26]) + Vector3.Distance(pred3D[23], pred3D[25])) / 2.0f;
        float tallUnity = (Vector3.Distance(lhip.position, lknee.position) + Vector3.Distance(lknee.position, lfoot.position)) / 2.0f +
            (Vector3.Distance(rhip.position, rknee.position) + Vector3.Distance(rknee.position, rfoot.position)) / 2.0f;

        // TODO: Optimize, more realistic
        Vector3 neck_pos = (pred3D[11] + pred3D[12]) / 2.0f;

        // TODO: Optimize, more realistic
        Vector3 hip_pos = (pred3D[23] + pred3D[24]) / 2.0f;

        // TODO: Optimize, more realistic, should be lower than the center point
        Vector3 spine_pos = (pred3D[23] + pred3D[24] + pred3D[11] + pred3D[12]) / 4.0f;

        Vector3 head_pos = (pred3D[7] + pred3D[8]) / 2.0f;
        Vector3 rMid_pos = (pred3D[18] + pred3D[20]) / 2.0f;
        Vector3 lMid_pos = (pred3D[17] + pred3D[19]) / 2.0f;

        Vector3 rToe_pos = (pred3D[32] + pred3D[30]) / 2.0f;
        Vector3 lToe_pos = (pred3D[31] + pred3D[29]) / 2.0f;

        if (moveRoot)
        {
            // TODO: Use low pass filter on this
            var t = hip_pos * (tallUnity / (tallThigh + tallShin));
            // var o = t - (hip_pos / (tallThigh + tallShin));
            var x = root.position.x;
            var y = root.position.y;
            var z = root.position.z;
            root.position = new Vector3(
                Mathf.Lerp(x, t.x, xLerp * Time.deltaTime),
                Mathf.Lerp(y, t.y, yLerp * Time.deltaTime),
                Mathf.Lerp(z, t.z, zLerp * Time.deltaTime));
        }
        else
            root.position = initPos;

        //////////////////////  Update Rotations  //////////////////////
        Vector3 forward = TriangleNormal(spine_pos, pred3D[23], pred3D[24]);
        
        // Root
        Quaternion root_rotation = Quaternion.LookRotation(forward) * Quaternion.Inverse(midRoot);
         
        // Body
        Quaternion spine_rotation = Quaternion.LookRotation(spine_pos - neck_pos, forward) * Quaternion.Inverse(midSpine);
        Quaternion neck_rotation = Quaternion.LookRotation(neck_pos - head_pos, forward) * Quaternion.Inverse(midNeck);
        
        // Head
        Quaternion head_rotation = Quaternion.LookRotation(pred3D[0] - head_pos, TriangleNormal(pred3D[0], pred3D[8], pred3D[7])) * Quaternion.Inverse(midHead);
        
        // Left arm
        Quaternion lshoulder_rotation = Quaternion.LookRotation(pred3D[11] - pred3D[13], forward) * Quaternion.Inverse(midLshoulder);
        Quaternion lelbow_rotation = Quaternion.LookRotation(pred3D[13] - pred3D[15], forward) * Quaternion.Inverse(midLelbow);
        Quaternion lhand_rotation = Quaternion.LookRotation(
            pred3D[21] - lMid_pos,
            TriangleNormal(pred3D[15], pred3D[21], lMid_pos)) * Quaternion.Inverse(midLhand);
        
        // Right arm
        Quaternion rshoulder_rotation = Quaternion.LookRotation(pred3D[12] - pred3D[14], forward) * Quaternion.Inverse(midRshoulder);
        Quaternion relbow_rotation = Quaternion.LookRotation(pred3D[14] - pred3D[16], forward) * Quaternion.Inverse(midRelbow);
        Quaternion rhand_rotation = Quaternion.LookRotation(
            pred3D[22] - rMid_pos,
            TriangleNormal(pred3D[16], pred3D[22], rMid_pos)) * Quaternion.Inverse(midRhand);
        
        // Left leg
        Quaternion lhip_rotation = Quaternion.LookRotation(pred3D[23] - pred3D[25], forward) * Quaternion.Inverse(midLhip);
        Quaternion lknee_rotation = Quaternion.LookRotation(pred3D[25] - pred3D[27], forward) * Quaternion.Inverse(midLknee);
        Quaternion lfoot_rotation = Quaternion.LookRotation(pred3D[27] - lToe_pos, pred3D[25] - pred3D[27]) * Quaternion.Inverse(midLfoot);
        
        // Right leg
        Quaternion rhip_rotation = Quaternion.LookRotation(pred3D[24] - pred3D[26], forward) * Quaternion.Inverse(midRhip);
        Quaternion rknee_rotation = Quaternion.LookRotation(pred3D[26] - pred3D[28], forward) * Quaternion.Inverse(midRknee);
        Quaternion rfoot_rotation = Quaternion.LookRotation(pred3D[28] - rToe_pos, pred3D[26] - pred3D[28]) * Quaternion.Inverse(midRfoot);

        SetRotation(root, root_rotation, lerpFactor);
        SetRotation(spine, spine_rotation, lerpFactor);
        SetRotation(neck, neck_rotation, lerpFactor);
        SetRotation(head, head_rotation, lerpFactor);
        SetRotation(lshoulder, lshoulder_rotation, lerpFactor);
        SetRotation(lelbow, lelbow_rotation, lerpFactor);
        SetRotation(lhand, lhand_rotation, lerpFactor);
        SetRotation(rshoulder, rshoulder_rotation, lerpFactor);
        SetRotation(relbow, relbow_rotation, lerpFactor);
        SetRotation(rhand, rhand_rotation, lerpFactor);
        SetRotation(lhip, lhip_rotation, lerpFactor);
        SetRotation(lknee, lknee_rotation, lerpFactor);
        SetRotation(lfoot, lfoot_rotation, lerpFactor);
        SetRotation(rhip, rhip_rotation, lerpFactor);
        SetRotation(rknee, rknee_rotation, lerpFactor);
        SetRotation(rfoot, rfoot_rotation, lerpFactor);

        return new List<Quaternion>
        {
            root.localRotation,
            lhip.localRotation,
            rhip.localRotation,
            lknee.localRotation,
            rknee.localRotation,
            lfoot.localRotation,
            rfoot.localRotation,
            spine.localRotation,
            new Quaternion(),
            neck.localRotation,
            head.localRotation,
            new Quaternion(),
            new Quaternion(),
            lshoulder.localRotation,
            rshoulder.localRotation,
            lelbow.localRotation,
            relbow.localRotation,
            lhand.localRotation,
            rhand.localRotation
        };
    }
    
    void SetRotation(Transform t, Quaternion q_o, float lerpFactor)
    {
        /*
        if (isLerp)
            t.rotation = Quaternion.Lerp(t.rotation, q, Time.deltaTime * lerpFactor);
        else
            t.rotation = Quaternion.Slerp(t.rotation, q, Time.deltaTime * lerpFactor);
        */

        Quaternion q = Quaternion.Inverse(t.parent.rotation) * q_o;

        if (isLerp)
            t.localRotation = Quaternion.Lerp(t.localRotation, q, Time.deltaTime * lerpFactor);
        else
            t.localRotation = Quaternion.Slerp(t.localRotation, q, Time.deltaTime * lerpFactor);
    }

    void SetLocalRotation(Transform t, Quaternion q, float lerpFactor)
    {
        if (isLerp)
            t.localRotation = Quaternion.Lerp(t.localRotation, q, Time.deltaTime * lerpFactor);
        else
            t.localRotation = Quaternion.Slerp(t.localRotation, q, Time.deltaTime * lerpFactor);
    }

    void SetPosition(Transform t, Vector3 p, float lerpFactor)
    {
        if (isLerp)
            t.position = Vector3.Lerp(t.position, p, Time.deltaTime * lerpFactor);
        else
            t.position = Vector3.Slerp(t.position, p, Time.deltaTime * lerpFactor);
    }

    void SetLocalPosition(Transform t, Vector3 p, float lerpFactor)
    {
        if (isLerp)
            t.localPosition = Vector3.Lerp(t.localPosition, p, Time.deltaTime * lerpFactor);
        else
            t.localPosition = Vector3.Slerp(t.localPosition, p, Time.deltaTime * lerpFactor);
    }

    public void UpdateByHumanoid(HumanoidController controller, bool instant = false)
    {
        float factor = instant ? 1000.0f : this.lerpFactor;
        // Root
        SetLocalRotation(root, controller.root.localRotation, factor);
        // 躯干
        SetLocalRotation(spine, controller.spine.localRotation, factor);
        SetLocalRotation(neck, controller.neck.localRotation, factor);
        // 头部
        SetLocalRotation(head, controller.head.localRotation, factor);
        // 左臂
        SetLocalRotation(lshoulder, controller.lshoulder.localRotation, factor);
        SetLocalRotation(lelbow, controller.lelbow.localRotation, factor);
        SetLocalRotation(lhand, controller.lhand.localRotation, factor);
        // 右臂
        SetLocalRotation(rshoulder, controller.rshoulder.localRotation, factor);
        SetLocalRotation(relbow, controller.relbow.localRotation, factor);
        SetLocalRotation(rhand, controller.rhand.localRotation, factor);
        // 左腿
        SetLocalRotation(lhip, controller.lhip.localRotation, factor);
        SetLocalRotation(lknee, controller.lknee.localRotation, factor);
        SetLocalRotation(lfoot, controller.lfoot.localRotation, factor);
        // 右腿
        SetLocalRotation(rhip, controller.rhip.localRotation, factor);
        SetLocalRotation(rknee, controller.rknee.localRotation, factor);
        SetLocalRotation(rfoot, controller.rfoot.localRotation, factor);
    }

    public void UpdateByFrame(PoseFrame poseFrame, bool instant = false)
    {
        float factor = instant ? 1000.0f : this.lerpFactor;

        // Root
        SetLocalRotation(root, poseFrame.boneQuaternions[(int)HumanBodyBones.Hips], factor);
        // 躯干
        SetLocalRotation(spine, poseFrame.boneQuaternions[(int)HumanBodyBones.Spine], factor);
        SetLocalRotation(neck, poseFrame.boneQuaternions[(int)HumanBodyBones.Neck], factor);
        // 头部
        SetLocalRotation(head, poseFrame.boneQuaternions[(int)HumanBodyBones.Head], factor);
        // 左臂
        SetLocalRotation(lshoulder, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftUpperArm], factor);
        SetLocalRotation(lelbow, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftLowerArm], factor);
        SetLocalRotation(lhand, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftHand], factor);
        // 右臂
        SetLocalRotation(rshoulder, poseFrame.boneQuaternions[(int)HumanBodyBones.RightUpperArm], factor);
        SetLocalRotation(relbow, poseFrame.boneQuaternions[(int)HumanBodyBones.RightLowerArm], factor);
        SetLocalRotation(rhand, poseFrame.boneQuaternions[(int)HumanBodyBones.RightHand], factor);
        // 左腿
        SetLocalRotation(lhip, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftUpperLeg], factor);
        SetLocalRotation(lknee, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftLowerLeg], factor);
        SetLocalRotation(lfoot, poseFrame.boneQuaternions[(int)HumanBodyBones.LeftFoot], factor);
        // 右腿
        SetLocalRotation(rhip, poseFrame.boneQuaternions[(int)HumanBodyBones.RightUpperLeg], factor);
        SetLocalRotation(rknee, poseFrame.boneQuaternions[(int)HumanBodyBones.RightLowerLeg], factor);
        SetLocalRotation(rfoot, poseFrame.boneQuaternions[(int)HumanBodyBones.RightFoot], factor);
    }

    /// <summary>
    /// Calculate the normal of a triangle
    /// </summary>
    private Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
}
