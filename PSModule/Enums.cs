
namespace PSModule
{
    public enum RunType
    {
        Alm,
        AlmLabManagement,
        FileSystem,
        LoadRunner
    }

    public enum AlmRunMode
    {
        RUN_NONE,
        RUN_LOCAL,
        RUN_REMOTE,
        RUN_PLANNED_HOST
    }

    public enum RunTestType
    {
        TEST_SUITE,
        BUILD_VERIFICATION_SUITE
    }

    public enum ArtifactType
    {
        onlyReport,
        onlyArchive,
        bothReportArchive,
        None
    }

    public enum RunStatus
    {
        PASSED = 0,
        FAILED = -1,
        UNSTABLE = -2,
        CLOSED_BY_USER = -3,
        UNDEFINED = -9
    }
}
