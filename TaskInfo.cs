using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BambooMonitor
{
    class TaskInfoParser
    {
        internal static TaskInfo FromHtml(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return null;

            string status = GetStatus(htmlContent);
            if (string.IsNullOrEmpty(status))
                return null;

            return new TaskInfo(
                HasEngineer(htmlContent, ASSIGNEE_TOKEN),
                HasEngineer(htmlContent, REVIEWER_TOKEN),
                HasEngineer(htmlContent, VALIDATOR_TOKEN),
                IsAlreadyIntegrated(htmlContent),
                status);
        }

        static string GetStatus(string htmlContent)
        {
            int statusIndex = htmlContent.IndexOf(STATUS_TOKEN);
            if (statusIndex < 0)
                return string.Empty;

            statusIndex += STATUS_TOKEN.Length;

            int nextTagIdx = htmlContent.IndexOf(TAG_START_TOKEN, statusIndex);
            return htmlContent.Substring(statusIndex, nextTagIdx - statusIndex);
        }

        static bool IsAlreadyIntegrated(string htmlContent)
        {
            int editIntegratedIndex = htmlContent.IndexOf(EDIT_INTEGRATED_TOKEN);
            if (editIntegratedIndex < 0)
                return false;

            int rowEnd = htmlContent.IndexOf(ROW_END_TOKEN, editIntegratedIndex);
            int releaseReportIndex = htmlContent.IndexOf(
                RELEASE_REPORT_TOKEN, editIntegratedIndex);

            return releaseReportIndex > 0 && releaseReportIndex < rowEnd;
        }

        static bool HasEngineer(string htmlContent, string role)
        {
            int roleIdx = htmlContent.IndexOf(role);
            if (roleIdx < 0)
                return false;

            int nextRowIdx = htmlContent.IndexOf(ROW_START_TOKEN, roleIdx);
            int nextRowEndIdx = htmlContent.IndexOf(ROW_END_TOKEN, nextRowIdx);

            return htmlContent.Substring(nextRowIdx, nextRowEndIdx).Contains(AVATAR_TOKEN);
        }

        const string STATUS_TOKEN = "\"defect-status\">";
        const string EDIT_INTEGRATED_TOKEN = "<a href=\"editintegratedrelease.php?iddefect=";
        const string RELEASE_REPORT_TOKEN = "<a href=\"releasereport.php?fRelease=";
        const string AVATAR_TOKEN = "<span class='avatarname'>";

        const string ASSIGNEE_TOKEN = "Assigned engineer";
        const string REVIEWER_TOKEN = "Reviewer";
        const string VALIDATOR_TOKEN = "Validator";

        const string TAG_START_TOKEN = "<";
        const string ROW_START_TOKEN = "<td";
        const string ROW_END_TOKEN = "</td>";
    }

    class TaskInfo
    {
        internal bool CanBeIntegrated()
        {
            if (mbIsAlreadyIntegrated)
                return false;

            if (mbHasValidator && mStatus == VALIDATED_STATUS)
                return true;

            if (mbHasReviewer && mStatus == REVIEWED_STATUS)
                return true;

            return mbHasAssignee && mStatus == RESOLVED_STATUS;
        }

        internal TaskInfo(
            bool hasAssignee,
            bool hasReviewer,
            bool hasValidator,
            bool isAlreadyIntegrated,
            string status)
        {
            mbHasAssignee = hasAssignee;
            mbHasReviewer = hasReviewer;
            mbHasValidator = hasValidator;
            mbIsAlreadyIntegrated = isAlreadyIntegrated;
            mStatus = status;
        }

        bool mbHasAssignee;
        bool mbHasReviewer;
        bool mbHasValidator;
        string mStatus;
        bool mbIsAlreadyIntegrated;

        const string VALIDATED_STATUS = "Validated";
        const string REVIEWED_STATUS = "Reviewed";
        const string RESOLVED_STATUS = "Resolved";
    }
}
