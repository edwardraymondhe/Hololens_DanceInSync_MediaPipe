public class PoseRecorder
{
    public PoseSequence continuousPoseSequence = new PoseSequence();
    public PoseSequence scatteredPoseSequence = new PoseSequence();

    public PoseRecorder() { }

    public void SaveInstantFrame(PoseFrame poseFrame)
    {
        Helper.Pose.SaveInstance(poseFrame);
    }

    public void AddScatteredFrame(PoseFrame poseFrame)
    {
        scatteredPoseSequence.Add(poseFrame);
    }
    public void AddContinuousFrame(PoseFrame poseFrame)
    {
        continuousPoseSequence.Add(poseFrame);
    }

    public void SaveScatteredFrames()
    {
        foreach (var frame in continuousPoseSequence.poseFrames)
            Helper.Pose.SaveInstance(frame);
    }
    public void SaveScatteredSequence()
    {
        Helper.Pose.SaveInstance(scatteredPoseSequence);
    }


    public void SaveContinuousFrames()
    {
        foreach (var frame in continuousPoseSequence.poseFrames)
            Helper.Pose.SaveInstance(frame);
    }

    public void SaveContinuousSequence()
    {
        Helper.Pose.SaveInstance(continuousPoseSequence);
    }
}
