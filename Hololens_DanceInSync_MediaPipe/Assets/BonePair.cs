using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helper.Pose;

[System.Serializable]
public class BonePair
{
    public BonePairLink bonePairLink;
    public BonePairStatus bonePairStatus;
    public BonePair() { }
    public BonePair(BonePairLink link, BonePairStatus stat)
    {
        bonePairLink = link;
        bonePairStatus = stat;
    }
}

[System.Serializable]
public class BonePairLink
{
    #region Helper Functions
    public static float GetAngle(Vector3 first, Vector3 second, Vector3 third)
    {
        float angle = Vector3.Angle(second - first, third - second);
        if (angle < 0.0f)
            angle += 360.0f;
        return 180.0f - angle;
    }
    private static Vector3Int GetBoneConnection(Vector2Int firstBoneIdx, Vector2Int secondBoneIdx)
    {
        int a_1 = firstBoneIdx.x;
        int b_1 = firstBoneIdx.y;
        int a_2 = secondBoneIdx.x;
        int b_2 = secondBoneIdx.y;

        if (a_1 == a_2)
            return new Vector3Int(b_1, a_1, b_2);
        else if (a_1 == b_2)    // b1, a1&b2, a2
            return new Vector3Int(b_1, a_1, a_2);
        else if (b_1 == a_2)    // a1, a2&b1, b2
            return new Vector3Int(a_1, a_2, b_2);
        else    // a1, b1&b2, a2
            return new Vector3Int(a_1, b_1, a_2);
    }
    private static Vector3 GetImagePosition(Landmark landmark, float width, float height, float depth)
    {
        return new Vector3(landmark.x * width, landmark.y * height, landmark.z * depth);
    }
    #endregion

    public HumanBodyBones firstBone;
    public HumanBodyBones secondBone;

    public BonePairLink() { }
    public BonePairLink(HumanBodyBones firstBone, HumanBodyBones secondBone) { this.firstBone = firstBone; this.secondBone = secondBone; }

    // Can convert to other scores such as position (e.g. touching toe, makes sense if hand is on toe)
    public string GetBoneString()
    {
        return firstBone + " -> " + secondBone;
    }


    public List<Vector3> GetImagePositions(Landmarks landmarks, float widthFactor, float heightFactor, float depthFactor)
    {
        Vector3 firstPosition = Vector3.zero;
        Vector3 secondPosition = Vector3.zero;
        Vector3 thirdPosition = Vector3.zero;
        // Gets path
        Vector2Int firstBoneIdx = linePair[firstBone];
        Vector2Int secondBoneIdx = linePair[secondBone];

        if (firstBone == HumanBodyBones.Spine && (secondBone == HumanBodyBones.RightUpperArm || secondBone == HumanBodyBones.RightUpperLeg))
        {
            if (secondBone == HumanBodyBones.RightUpperArm)
            {
                firstPosition = GetImagePosition(landmarks.landmarks[14], widthFactor, heightFactor, depthFactor);
                secondPosition = GetImagePosition(landmarks.landmarks[12], widthFactor, heightFactor, depthFactor);
                thirdPosition = GetImagePosition(landmarks.landmarks[24], widthFactor, heightFactor, depthFactor);
            }
            else
            {
                firstPosition = GetImagePosition(landmarks.landmarks[12], widthFactor, heightFactor, depthFactor);
                secondPosition = GetImagePosition(landmarks.landmarks[24], widthFactor, heightFactor, depthFactor);
                thirdPosition = GetImagePosition(landmarks.landmarks[26], widthFactor, heightFactor, depthFactor);
            }
        }
        else
        {
            Vector3Int connectionIdx = GetBoneConnection(firstBoneIdx, secondBoneIdx);
            firstPosition = GetImagePosition(landmarks.landmarks[connectionIdx.x], widthFactor, heightFactor, depthFactor);
            secondPosition = GetImagePosition(landmarks.landmarks[connectionIdx.y], widthFactor, heightFactor, depthFactor);
            thirdPosition = GetImagePosition(landmarks.landmarks[connectionIdx.z], widthFactor, heightFactor, depthFactor);
        }

        return new List<Vector3> { firstPosition, secondPosition, thirdPosition };
    }

    #region Override Comparison Functions
    public override bool Equals(object obj)
    {
        BonePairLink b = obj as BonePairLink;
        //对变量的所有的属性都要进行比较  只有都相同才返回true
        if (firstBone == b.firstBone && secondBone == b.secondBone)
            return true;
        else
            return false;
    }
    public override int GetHashCode()
    {
        int hashCode1 = firstBone.GetHashCode();
        int hashCode2 = secondBone.GetHashCode();

        return (hashCode1.ToString() + " " + hashCode2.ToString()).GetHashCode();
    }
    #endregion
}

[System.Serializable]
public class BonePairStatus
{
    public float angle = 0.0f;
    public float velocity = 0.0f;
    public BonePairStatus() { }
    public BonePairStatus(float ang, float vel) { angle = ang; velocity = vel; }
}