using UCDArch.Web.Controller;
using UCDArch.Web.Attributes;

namespace StudentFarm.Controllers
{
    [Version(MajorVersion = 3)]
    //[ServiceMessage("StudentFarm", ViewDataKey = "ServiceMessages", MessageServiceAppSettingsKey = "MessageService")]
    public abstract class ApplicationController : SuperController
    {
    }
}