using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SwissAcademic.Citavi;
using SwissAcademic.Controls;
using SwissAcademic.Citavi.Shell;

using Infragistics.Win.UltraWinToolbars;

namespace ReferencesToolbox
{
    class ReferencesToolboxUI
                :
        CitaviAddOn
    {
        public override AddOnHostingForm HostingForm
        {
            get { return AddOnHostingForm.MainForm; }
        }
        protected override void OnHostingFormLoaded(System.Windows.Forms.Form hostingForm)
        {
            MainForm mainForm = (MainForm)hostingForm;

            var referencesMenu = mainForm.GetMainCommandbarManager().GetReferenceEditorCommandbar(MainFormReferenceEditorCommandbarId.Menu).GetCommandbarMenu(MainFormReferenceEditorCommandbarMenuId.References);

            var commandbarButtonParallelReportersParse = referencesMenu.AddCommandbarButton("ParallelReportersParse", ReferencesToolboxLocalizations.ParallelReportersParse);
            commandbarButtonParallelReportersParse.Shortcut = ((Shortcut)(Keys.Control | Keys.Shift | Keys.P));
            commandbarButtonParallelReportersParse.HasSeparator = true;
            referencesMenu.AddCommandbarButton("ShorthandChangeInParallelReporters", ReferencesToolboxLocalizations.ShorthandChangeInParallelReporters);
            referencesMenu.AddCommandbarButton("PageRangeAssign", ReferencesToolboxLocalizations.PageRangeAssign);

            // Reference Editor Attachment Pop-Up Menu

            var referenceEditorUriLocationsContextMenu = CommandbarMenu.Create(mainForm.GetReferenceEditorElectronicLocationsCommandbarManager().ToolbarsManager.Tools["ReferenceEditorUriLocationsContextMenu"] as PopupMenuTool);
            var commandBarButtonCopyLocationPathToClipboard = referenceEditorUriLocationsContextMenu.AddCommandbarButton("CopyLocationPathToClipboard", ReferencesToolboxLocalizations.CopyLocationPathToClipboard);

            // Fin

            base.OnHostingFormLoaded(hostingForm);
        }


        protected override void OnBeforePerformingCommand(SwissAcademic.Controls.BeforePerformingCommandEventArgs e)
        {
            Project project = Program.ActiveProjectShell.Project;

            switch (e.Key)
            {
                #region Reference-based commands
                case "ParallelReportersParse":
                    {
                        List<Reference> listSelectedReferences = Program.ActiveProjectShell.PrimaryMainForm.GetSelectedReferences();
                        e.Handled = true;
                        ParallelReporterParser.ParseReporters(project, listSelectedReferences);
                    }
                    break;
                case "ShorthandChangeInParallelReporters":
                    {
                        e.Handled = true;
                        ToolboxReference.ChangeShorthands();
                    }
                    break;
                #endregion

                #region ReferenceEditorUriLocationsPopupMenu

                case "CopyLocationPathToClipboard":
                    {
                        e.Handled = true;
                        ToolboxReference.CopyLocationClipboard();
                    }
                    break;

                #endregion
            }
            base.OnBeforePerformingCommand(e);
        }
    }
}
