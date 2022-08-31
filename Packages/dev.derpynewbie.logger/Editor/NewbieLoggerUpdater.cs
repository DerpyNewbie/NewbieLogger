using DerpyNewbie.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace DerpyNewbie.Logger.Editor
{
    public class NewbieLoggerUpdater : MonoBehaviour
    {
        [MenuItem("DerpyNewbie/Update/Update NewbieLogger")]
        public static void UpdateNewbieCommonsPackage()
        {
            NewbieCommonsPackageUpdater.UpdatePackage("NewbieLogger",
                "https://github.com/DerpyNewbie/NewbieLogger.git?path=/Packages/dev.derpynewbie.logger");
        }
    }
}