using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SwissAcademic;
using SwissAcademic.Citavi;
using SwissAcademic.Citavi.Shell;
using SwissAcademic.Citavi.Shell.Controls.Preview;
using SwissAcademic.Pdf;
using SwissAcademic.Pdf.Analysis;

namespace ReferencesToolbox
{
    class ToolboxReference
    {
        public static void ChangeShorthands()
        {

            bool handled;

            List<Reference> references = new List<Reference>();
            SwissAcademic.Citavi.Project activeProject = Program.ActiveProjectShell.Project;

            List<Reference> activeProjectReferencesList = new List<Reference>();

            // We copy the active project references in a list so that the changes don't affect the order of the list we later loop through

            foreach (Reference reference in activeProject.References)
            {
                if (reference.ReferenceType != ReferenceType.CourtDecision) continue;
                activeProjectReferencesList.Add(reference);
            }

            // Select the references to be renamed	

            DialogResult dialogResult = MessageBox.Show("Do you want to modify the value of the short title (citation key) field of the selected references (including their parallel reporters)?",
                "Change Short Title (Citation Key)", MessageBoxButtons.OKCancel);

            if (dialogResult == DialogResult.OK)
            {
                List<Reference> selectedReferences = Program.ActiveProjectShell.PrimaryMainForm.GetSelectedReferences();
                foreach (Reference reference in selectedReferences)
                {
                    references.Add(reference);
                }
            }
            else if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            string shortTitleString;
            int i = 0;

            List<Reference> dateChangedList = new List<Reference>();

            List<string> shortTitlesList = new List<string>();

            foreach (Reference currentReference in references)
            {
                string data;
                string ret = ReferenceForms.NewCitationKeyForm("Please enter the new citation key:", out data);

                shortTitleString = currentReference.ShortTitle.ToStringSafe();
                currentReference.CitationKey = ret;
                currentReference.CitationKeyUpdateType = UpdateType.Manual;
                i = i + 1;
                shortTitlesList.Add(shortTitleString);
                dateChangedList.Add(currentReference);

                string Date = currentReference.Date;

                foreach (Reference reference in activeProjectReferencesList)
                {
                    if (reference == currentReference) continue;

                    if (reference.Periodical == null) continue;
                    if (!currentReference.Organizations.SequenceEqual(reference.Organizations)) continue;
                    if (currentReference.Title != reference.Title) continue;
                    if (String.IsNullOrEmpty(currentReference.Title) && (String.IsNullOrEmpty(currentReference.SpecificField2) || currentReference.SpecificField2 != reference.SpecificField2)) continue;
                    shortTitleString = reference.ShortTitle.ToStringSafe();
                    shortTitlesList.Add(shortTitleString);
                    reference.CitationKey = ret;
                    reference.CitationKeyUpdateType = UpdateType.Manual;
                    dateChangedList.Add(reference);
                    i = i + 1;
                    continue;
                }
            }
            DialogResult showFailed = MessageBox.Show(i.ToStringSafe() +
                " references have been changed:\n   •" +
                String.Join("\n   • ", shortTitlesList) +
                "\nWould you like to show a selection of references where the short title (citation key) has been adjusted?",
                "Citavi Macro", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (showFailed == DialogResult.Yes)
            {
                var filter = new ReferenceFilter(dateChangedList, "Date changed", false);
                Program.ActiveProjectShell.PrimaryMainForm.ReferenceEditorFilterSet.Filters.ReplaceBy(new List<ReferenceFilter> { filter });
            }

        }
        public static void CopyLocationClipboard()
        {
            List<Location> locations = Program.ActiveProjectShell.PrimaryMainForm.GetSelectedElectronicLocations();
            List<String> output = new List<String>();

            Program.ActiveProjectShell.PrimaryMainForm.PreviewControl.ShowNoPreview();

            foreach (Location location in locations)
            {
                if (output.Count > 0) output.Add("\n");
                output.Add("\"");
                output.Add(location.Address.Resolve().LocalPath);
                output.Add("\"");
            }
            Clipboard.SetText(String.Join("", output));
        }
    }
}
