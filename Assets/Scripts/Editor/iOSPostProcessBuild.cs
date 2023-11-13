using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class iOSPostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            ModifyXcodeProject(pathToBuildProject);
        }
    }

    private static void ModifyXcodeProject(string pathToBuildProject)
    {
        string projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projPath);

        string target = project.GetUnityFrameworkTargetGuid();

        project.AddBuildProperty(target, "OTHER_LDFLAGS", "-ld_classic");

        // Save changes
        project.WriteToFile(projPath);
    }
}