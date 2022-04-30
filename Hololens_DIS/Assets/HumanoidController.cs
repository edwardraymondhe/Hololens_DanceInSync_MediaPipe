using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoneOffset
{
    public HumanBodyBones bone;
    public Vector3 offset = Vector3.zero;
}

public class HumanoidController : MonoBehaviour
{
    public Animator animator;
    public MediaPipeServer server;
    public List<BoneOffset> boneOffsets = new List<BoneOffset>();
    public List<Transform> boneTransforms = new List<Transform>();
    public GameObject body;
    // Start is called before the first frame update
    void Start()
    {
        boneTransforms = new List<Transform>(body.GetComponentInChildren<SkinnedMeshRenderer>().bones);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAnimatorIK(int layerIndex)
    {
        // Helper.Pose.Landmarks poseLandmarks
        if (server.poseLandmarks.landmarks.Count > 0)
        {
            foreach (var bonePair in server.currentPoseFrame.bonePairs)
            {
                var bone = bonePair.bonePairLink.secondBone;
                BoneOffset boneOffset = boneOffsets.Find(e => e.bone == bone);
                if (boneOffset == null)
                {
                    boneOffset = new BoneOffset { bone = bone };
                    boneOffsets.Add(boneOffset);
                }

                // Have an initial position, which facing forward is zero
                // Quaternion.LookRotation(new Vector3(1, 0, 0))

                // The "transform" should be the bone's transform

                // var relative = animator.GetBoneTransform(bonePair.bonePairLink.secondBone).InverseTransformDirection(bonePair.bonePairStatus.dir);

                // var quat = Quaternion.LookRotation(relative) * Quaternion.Euler(boneOffset.offset);    // This is actually world, I need to transform it to local

                // animator.SetBoneLocalRotation(bonePair.bonePairLink.secondBone, quat);

                // animator.GetBoneTransform(bonePair.bonePairLink.secondBone).rotation = Quaternion.LookRotation(bonePair.bonePairStatus.dir);
                animator.GetBoneTransform(bonePair.bonePairLink.secondBone).rotation.SetLookRotation(bonePair.bonePairStatus.dir);

            }
        }
    }
}
