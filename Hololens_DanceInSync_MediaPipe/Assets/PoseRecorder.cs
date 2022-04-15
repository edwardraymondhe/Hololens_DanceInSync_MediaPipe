public class PoseRecorder
{
    public PoseSequence poseSequence = new PoseSequence();

    public PoseRecorder() { }
    public PoseRecorder(PoseFrame poseFrame)
    {
        poseSequence.Add(poseFrame);
    }

    public void RecordFrame(PoseFrame poseFrame)
    {
        poseSequence.Add(poseFrame);
    }

    public void SaveFrames()
    {
        foreach (var frame in poseSequence.poseFrames)
            Helper.Pose.SaveInstance(frame);
    }

    public void SaveSequence()
    {
        Helper.Pose.SaveInstance(poseSequence);
    }
}
