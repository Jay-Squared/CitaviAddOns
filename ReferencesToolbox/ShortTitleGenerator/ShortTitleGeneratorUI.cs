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

namespace ShortTitleGenerator
{
    class UI
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

            if (mainForm.ActiveWorkspace == MainFormWorkspace.ReferenceEditor)
            {
                Reference.ShortTitleFilter = new ShortTitleGenerator();
            }

            var referencesMenu = mainForm.GetMainCommandbarManager().GetReferenceEditorCommandbar(MainFormReferenceEditorCommandbarId.Menu).GetCommandbarMenu(MainFormReferenceEditorCommandbarMenuId.References);

            referencesMenu.AddCommandbarButton("GenerateShortTitle", ShortTitleGeneratorLocalization.AssignShortTitle);


            base.OnHostingFormLoaded(hostingForm);
        }
        protected override void OnBeforePerformingCommand(SwissAcademic.Controls.BeforePerformingCommandEventArgs e)
        {
            Project project = Program.ActiveProjectShell.Project;

            switch (e.Key)
            {
                case "GenerateShortTitle":
                    {
                        e.Handled = true;

                        List<Reference> listSelectedReferences = Program.ActiveProjectShell.PrimaryMainForm.GetSelectedReferences();

                        var filter = new ShortTitleGenerator();

                        foreach (Reference reference in listSelectedReferences)
                        {
                            reference.ShortTitle = filter.GetFilterResult(reference, out bool handled);
                            reference.ShortTitleUpdateType = UpdateType.Automatic;
                        }
                    }
                    break;
            }
            base.OnBeforePerformingCommand(e);
        }
    }
}
